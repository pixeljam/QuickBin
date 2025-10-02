using System;
using System.Text;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickBin {
	/// <summary>
	/// High-performance binary writer with a single growable byte[] backing store.
	/// - Bulk appends (no per-byte List.Add)
	/// - Reusable via Clear() and a small pool (SerializerPool)
	/// - Flag packing without mutating past bytes
	/// - Supports reserving & patching previously written lengths
	/// </summary>
	public sealed class Serializer : IEnumerable<byte> {
		private const int MIBIBYTE = 1024 * 1024;
		private const int MAX_STRING_CHUNK_SIZE = 4096;
		private byte[] _buffer;
		/// <summary>The number of bytes in the Serializer.</summary>
		public int Length { get; private set; }

		private byte flagAccumulator; // current byte being packed
		private int  flagBitIndex; // 0..7 number of bits already packed (0 means none pending)
		private bool hasPendingFlagByte => flagBitIndex > 0;

		/// <summary>The bytes in the Serializer (enumerable view).</summary>
		public IEnumerable<byte> Bytes {
			get {
				for (int i = 0; i < Length; i++) yield return _buffer[i];
				if (hasPendingFlagByte) yield return flagAccumulator;
			}
		}
		
		public delegate void WriteAction<T>(Span<byte> dest, T length);
		public sealed class LengthWriter<T> {
			public WriteAction<T> write;
			public int dataSize;
			
			public LengthWriter(WriteAction<T> write, int dataSize) {
				this.write = write;
				this.dataSize = dataSize;
			}
		};
		public static readonly LengthWriter<int> Len_i32 = new(BinaryPrimitives.WriteInt32LittleEndian, sizeof(int));
		public static readonly LengthWriter<uint> Len_u32 = new(BinaryPrimitives.WriteUInt32LittleEndian, sizeof(uint));
		public static readonly LengthWriter<ushort> Len_u16 = new(BinaryPrimitives.WriteUInt16LittleEndian, sizeof(ushort));
		public static readonly LengthWriter<byte> Len_u8 = new(static (dest, len) => dest[0] = len, sizeof(byte));


		#region Constructors
			public Serializer() : this(0) { }

			/// <param name="capacity">Initial capacity hint in bytes.</param>
			public Serializer(int capacity) {
				if (capacity < 0) capacity = 0;
				_buffer = capacity > 0 ? new byte[capacity] : Array.Empty<byte>();
				Length = 0;
				flagAccumulator = 0;
				flagBitIndex = 0;
			}
		#endregion Constructors

		#region Enumerable
			public IEnumerator<byte> GetEnumerator() => Bytes;
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion Enumerable

		#region Array casting
			/// <summary>Implicitly materialize the written bytes.</summary>
			public static implicit operator byte[](Serializer s) => s.ToArray();
			/// <summary>Copies written bytes into a compact array.</summary>
			public byte[] ToArray() {
				// We don't want to mutate the serializer by doing this, but we do want to treat anything left in the flag accumulator as valid data.
				// We know we must have a free byte in which to put this unfinished flag byte, because we allocated when it was started.
				var lengthWithTrailingFlags = Length;
				if (hasPendingFlagByte) lengthWithTrailingFlags++;
				if (lengthWithTrailingFlags == 0) return Array.Empty<byte>();
				
				var arr = new byte[lengthWithTrailingFlags];
				
				Buffer.BlockCopy(_buffer, 0, arr, 0, Length);
				if (hasPendingFlagByte) arr[^1] = flagAccumulator;
				
				return arr;
			}
		#endregion Array casting
		
		#region Clearing
			/// <summary>Clears written data and flag state; retains allocated buffer for reuse.</summary>
			public Serializer Clear() {
				Length = 0;
				flagAccumulator = 0;
				flagBitIndex = 0;
				return this;
			}
		#endregion Clearing

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int extraNeeded) {
			if (extraNeeded < 0) throw new ArgumentOutOfRangeException(nameof(extraNeeded));
			
			int required = Length + extraNeeded;
			if (required <= _buffer.Length) return;
			
			int newCap = _buffer.Length == 0 ? 256 : _buffer.Length;
			while (newCap < required) {
				newCap = newCap < MIBIBYTE
					? newCap << 1 // Exponentially double
					: newCap + MIBIBYTE; // Linearly add mibibytes
			}
			
			Array.Resize(ref _buffer, newCap);
		}

		/// <summary>Returns a writable span of requested size at the current end, advancing Count.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Span<byte> AllocateSpan(int size) {
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0) return Span<byte>.Empty;
			
			FlushPendingFlagByte(); // make sure flag groups don't get interleaved
			EnsureCapacity(size);
			var span = _buffer.AsSpan(Length, size);
			Length += size;
			
			return span;
		}

		/// <summary>Span over the already written region (mutable for patching).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Span<byte> WrittenSpanMutable() => MemoryMarshal.CreateSpan(ref _buffer[0], Length);

		#region Flags
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void CommitFlagByte() {
				_buffer[Length++] = flagAccumulator;
				flagAccumulator = 0;
				flagBitIndex = 0;
			}

			/// <summary>Flushes a partially filled flag byte into the buffer (if any).</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void FlushPendingFlagByte(bool force = false) {
				if (hasPendingFlagByte && (flagBitIndex > 0 || force))
					CommitFlagByte();
			}

			/// <summary>
			/// Packs booleans into a single byte (up to 8 per byte). Set <paramref name="forceNewByte"/> to start a new flag group.
			/// </summary>
			public Serializer WriteFlag(bool value, bool forceNewByte = false) {
				if (forceNewByte) FlushPendingFlagByte(true);

				if (!hasPendingFlagByte) EnsureCapacity(1);

				if (value) flagAccumulator |= (byte)(1 << flagBitIndex);
				flagBitIndex++;
				if (flagBitIndex >= 8) CommitFlagByte();

				return this;
			}
		#endregion Flags

		#region Endian
			/// <summary>Reserve two bytes (u16 LE) and return absolute position to patch later.</summary>
			public int ReserveU16LittleEndian() {
				FlushPendingFlagByte();
				int pos = Length;
				var s = AllocateSpan(2);
				s[0] = 0; s[1] = 0;
				return pos;
			}

			/// <summary>Reserve four bytes (u32 LE) and return absolute position to patch later.</summary>
			public int ReserveU32LittleEndian() {
				FlushPendingFlagByte();
				int pos = Length;
				var s = AllocateSpan(4);
				s[0] = s[1] = s[2] = s[3] = 0;
				return pos;
			}

			public void PatchU16LittleEndian(int absolutePos, ushort value) {
				if ((uint)absolutePos > (uint)(Length - 2)) throw new ArgumentOutOfRangeException(nameof(absolutePos));
				var whole = WrittenSpanMutable();
				BinaryPrimitives.WriteUInt16LittleEndian(whole.Slice(absolutePos, 2), value);
			}

			public void PatchU32LittleEndian(int absolutePos, uint value) {
				if ((uint)absolutePos > (uint)(Length - 4)) throw new ArgumentOutOfRangeException(nameof(absolutePos));
				var whole = WrittenSpanMutable();
				BinaryPrimitives.WriteUInt32LittleEndian(whole.Slice(absolutePos, 4), value);
			}
		#endregion Endian

		#region Writers
			/// <summary>Generic writer for fixed-size primitives via a ByteWriter delegate (uses a pre-sized span).</summary>
			internal Serializer WriteGeneric<T>(int size, T value, ByteWriter<T> f) {
				var dest = AllocateSpan(size);
				f(dest, value);
				// any non-flag write breaks flag packing: handled by AllocateSpan() call above
				return this;
			}

			/// <summary>Single-byte writer via a small converter (no stackalloc, no delegates in hot path).</summary>
			internal Serializer WriteGeneric<T>(T value, Func<T, byte> f) {
				var dest = AllocateSpan(1);
				dest[0] = f(value);
				return this;
			}

			/// <summary>Bulk copy of arbitrary byte data.</summary>
			internal Serializer WriteGeneric(ReadOnlySpan<byte> value) {
				if (value.Length == 0) return this;
				var dest = AllocateSpan(value.Length);
				value.CopyTo(dest);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(byte value) {
				var s = AllocateSpan(sizeof(byte));
				s[0] = value;
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(sbyte value) {
				var s = AllocateSpan(sizeof(sbyte));
				s[0] = unchecked((byte)value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(short value) {
				var s = AllocateSpan(sizeof(short));
				BinaryPrimitives.WriteInt16LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(ushort value) {
				var s = AllocateSpan(sizeof(ushort));
				BinaryPrimitives.WriteUInt16LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(int value) {
				var s = AllocateSpan(sizeof(int));
				BinaryPrimitives.WriteInt32LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(uint value) {
				var s = AllocateSpan(sizeof(uint));
				BinaryPrimitives.WriteUInt32LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(long value) {
				var s = AllocateSpan(sizeof(long));
				BinaryPrimitives.WriteInt64LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(ulong value) {
				var s = AllocateSpan(sizeof(ulong));
				BinaryPrimitives.WriteUInt64LittleEndian(s, value);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(float value) => Write(unchecked((int)BitConverter.SingleToInt32Bits(value)));
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(double value) => Write(unchecked((long)BitConverter.DoubleToInt64Bits(value)));

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(decimal value) {
				// Decimal.GetBits returns four ints (lo, mid, hi, flags). Persist as 16 bytes LE.
				int[] bits = decimal.GetBits(value);
				var s = AllocateSpan(sizeof(decimal));
				BinaryPrimitives.WriteInt32LittleEndian(s.Slice(0, 4),  bits[0]);
				BinaryPrimitives.WriteInt32LittleEndian(s.Slice(4, 4),  bits[1]);
				BinaryPrimitives.WriteInt32LittleEndian(s.Slice(8, 4),  bits[2]);
				BinaryPrimitives.WriteInt32LittleEndian(s.Slice(12, 4), bits[3]);
				return this;
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(char value) => Write((ushort)value); // UTF-16 code unit, LE

			// For structured bit-packing, prefer WriteFlag().
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(bool value) => Write(value ? (byte)1 : (byte)0);
			
			/// <summary>Directly appends a span of bytes into the Serializer verbatim.</summary>
			/// <remarks>This overload does not write the number of bytes in the span into the serializer.</remarks>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(ReadOnlySpan<byte> bytes) {
				if (bytes.Length == 0) return this;
				var s = AllocateSpan(bytes.Length);
				bytes.CopyTo(s);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(DateTime value) => Write(value.Ticks);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(TimeSpan value) => Write(value.Ticks);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(Version value) => Write(value.Major).Write(value.Minor).Write(value.Build).Write(value.Revision);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(ReadOnlySpan<byte> value, LengthWriter writer) {
				var dest = AllocateSpan(value.Length + writer.dataSize);
				writer.write(dest, value.Length);
				value.CopyTo(dest[writer.dataSize..]);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(string value, Encoding encoding, LengthWriter writer) {
				if (string.IsNullOrEmpty(value)) {
					writer.write(AllocateSpan(writer.dataSize), 0);
					return this;
				}
				
				var maxStringBytes = encoding.GetMaxByteCount(value.Length);
				var dest = AllocateSpan(maxStringBytes + writer.dataSize);

				var enc = encoding.GetEncoder();
				var written = enc.GetBytes(value, dest[writer.dataSize..], true);
				Length -= maxStringBytes - written;
				
				writer.write(dest, written);
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Write(string value, LengthWriter writer) => Write(value, Encoding.UTF8, writer);

			// Write any unmanaged struct in one bulk copy
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public unsafe Serializer WriteUnmanaged<T>(in T value) where T : unmanaged {
				// Write the bytes of 'value' directly into dest
				MemoryMarshal.Write(AllocateSpan(sizeof(T)), ref Unsafe.AsRef(value));
				return this;
			}

			// Unmanaged array bulk write (no per-element calls)
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public unsafe Serializer WriteUnmanagedArray<T>(ReadOnlySpan<T> values, LengthWriter writer = null) where T : unmanaged {
				int bytes = values.Length * sizeof(T);
				int payloadStart = 0;
				if (writer != null) payloadStart = writer.dataSize;
				
				var dest = AllocateSpan(bytes + payloadStart);
				
				writer?.write(dest, bytes);
				if (bytes == 0) return this;
				
				MemoryMarshal.AsBytes(values).CopyTo(dest[payloadStart..]);
				return this;
			}
		#endregion Writers

		#region Pooling
			/// <summary>Get a Serializer from the pool (optionally with a capacity hint).</summary>
			public static Serializer GetPooled(int capacityHint = 0) => SerializerPool.Get(capacityHint);

			/// <summary>Materialize to byte[] and return the Serializer to the pool.</summary>
			public byte[] ToArrayAndReturn() => SerializerPool.ToArrayAndReturn(this);
		#endregion Pooling
	}

	#region Pool
		/// <summary>Very small, thread-unsafe pool; use from main thread in your capture loop.</summary>
		public static class SerializerPool {
			private static readonly Stack<Serializer> _pool = new();

			public static Serializer Get(int capacityHint = 0) {
				if (_pool.Count > 0) {
					var s = _pool.Pop();
					s.Clear();
					// opportunistic grow if we know we're about to write a lot
					if (capacityHint > 0) {
						// EnsureCapacity is internal; do a cheap reserve by writing/rewinding
						// We avoid touching internals: just return; growth will happen lazily.
					}
					return s;
				}
				return new(capacityHint);
			}

			public static byte[] ToArrayAndReturn(Serializer s) {
				var arr = s.ToArray();
				_pool.Push(s);
				return arr;
			}
		}
	#endregion Pool
}