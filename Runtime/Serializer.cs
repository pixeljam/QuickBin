using System;
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
		
		private byte[] _buffer;
		internal int bufferLength; // The number of valid bytes in _buffer
		/// <summary>The total length of the all data written so far.</summary>
		public int Length => bufferLength + (hasPendingFlagByte ? 1 : 0);

		private byte flagAccumulator; // current byte being packed
		private int  flagBitIndex; // 0..7 number of bits already packed (0 means none pending)
		private bool hasPendingFlagByte => flagBitIndex > 0;

		/// <summary>The bytes in the Serializer (enumerable view).</summary>
		public IEnumerable<byte> Bytes {
			get {
				for (int i = 0; i < bufferLength; i++) yield return _buffer[i];
				if (hasPendingFlagByte) yield return flagAccumulator;
			}
		}
		
		public delegate void WriteAction(Span<byte> dest, int length);
		public sealed class LengthWriter {
			public WriteAction write;
			public int dataSize;
			
			public LengthWriter(WriteAction write, int dataSize) {
				this.write = write;
				this.dataSize = dataSize;
			}
		};
		public static readonly LengthWriter Len_i64 = new(static (dest, len) => BinaryPrimitives.WriteInt64LittleEndian(dest, len), sizeof(long));
		public static readonly LengthWriter Len_i32 = new(static (dest, len) => BinaryPrimitives.WriteInt32LittleEndian(dest, len), sizeof(int));
		public static readonly LengthWriter Len_i16 = new(static (dest, len) => BinaryPrimitives.WriteInt16LittleEndian(dest, (short)len), sizeof(short));
		public static readonly LengthWriter Len_i8 = new(static (dest, len) => dest[0] = (byte)(sbyte)len, sizeof(sbyte));
		public static readonly LengthWriter Len_u64 = new(static (dest, len) => BinaryPrimitives.WriteUInt64LittleEndian(dest, (ulong)len), sizeof(ulong));
		public static readonly LengthWriter Len_u32 = new(static (dest, len) => BinaryPrimitives.WriteUInt32LittleEndian(dest, (uint)len), sizeof(uint));
		public static readonly LengthWriter Len_u16 = new(static (dest, len) => BinaryPrimitives.WriteUInt16LittleEndian(dest, (ushort)len), sizeof(ushort));
		public static readonly LengthWriter Len_u8 = new(static (dest, len) => dest[0] = (byte)len, sizeof(byte));


		#region Constructors
			public Serializer() : this(0) { }

			/// <param name="capacity">Initial capacity hint in bytes.</param>
			public Serializer(int capacity) {
				if (capacity < 0) capacity = 0;
				_buffer = capacity > 0 ? new byte[capacity] : Array.Empty<byte>();
				bufferLength = 0;
				flagAccumulator = 0;
				flagBitIndex = 0;
			}
		#endregion Constructors

		#region Enumerable
			public IEnumerator<byte> GetEnumerator() => Bytes.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion Enumerable

		#region Array casting
			/// <summary>Implicitly materialize the written bytes.</summary>
			public static implicit operator byte[](Serializer s) => s.ToArray();
			/// <summary>Copies written bytes into a compact array.</summary>
			public byte[] ToArray() {
				// We don't want to mutate the serializer by doing this, but we do want to treat anything left in the flag accumulator as valid data.
				// We know we must have a free byte in which to put any unfinished flag byte, because we would have allocated it with the first flag.
				var length = Length; // Important that this includes pending flag bytes.
				if (length == 0) return Array.Empty<byte>();

				var arr = new byte[length];

				Buffer.BlockCopy(_buffer, 0, arr, 0, bufferLength);
				if (hasPendingFlagByte) arr[^1] = flagAccumulator;
				
				return arr;
			}
		#endregion Array casting
		
		#region Clearing
			/// <summary>Clears written data and flag state; retains allocated buffer for reuse.</summary>
			public Serializer Clear() {
				bufferLength = 0;
				flagAccumulator = 0;
				flagBitIndex = 0;
				return this;
			}
		#endregion Clearing

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int extraNeeded) {
			if (extraNeeded < 0) throw new ArgumentOutOfRangeException(nameof(extraNeeded));
			
			int required = bufferLength + extraNeeded;
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
		internal Span<byte> AllocateSpan(int size) {
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0) return Span<byte>.Empty;
			
			FlushPendingFlagByte(); // make sure flag groups don't get interleaved
			EnsureCapacity(size);
			var span = _buffer.AsSpan(bufferLength, size);
			bufferLength += size;
			
			return span;
		}

		#region Flags
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void CommitFlagByte() {
				_buffer[bufferLength++] = flagAccumulator;
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

		#region Patching
			public readonly ref struct ReservedLengthPrefixer<T> where T : unmanaged {
				internal readonly Span<byte> span;
				internal readonly int initialLength;
				
				// The unsafe constraint is actually incorrect. sizeof(T) is allowed in safe code where T : unmanaged
				// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/sizeof
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				internal unsafe ReservedLengthPrefixer(Serializer serializer) {
					span = serializer.AllocateSpan(sizeof(T));
					initialLength = serializer.Length;
				}
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				readonly internal int GetLength(Serializer serializer) => serializer.Length - initialLength;
			}
			
			/// <summary>Reserves a spot in the Serializer to write a length later.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer ReserveLength<T>(out ReservedLengthPrefixer<T> prefixer) where T : unmanaged {
				FlushPendingFlagByte();
				prefixer = new ReservedLengthPrefixer<T>(this);
				return this;
			}
			
			/// <summary>Reserves a number of bytes to be patched over later.</summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer ReserveBytes(int byteCount, out Span<byte> span) {
				if (byteCount <= 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
				FlushPendingFlagByte();
				span = AllocateSpan(byteCount);
				return this;
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<byte> reserved) {
				reserved.span[0] = (byte)reserved.GetLength(this);
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<sbyte> reserved) {
				reserved.span[0] = (byte)(sbyte)reserved.GetLength(this);
				return this;
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<ushort> reserved) {
				BinaryPrimitives.WriteUInt16LittleEndian(reserved.span, (ushort)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<short> reserved) {
				BinaryPrimitives.WriteInt16LittleEndian(reserved.span, (short)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<uint> reserved) {
				BinaryPrimitives.WriteUInt32LittleEndian(reserved.span, (uint)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<int> reserved) {
				BinaryPrimitives.WriteInt32LittleEndian(reserved.span, reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<ulong> reserved) {
				BinaryPrimitives.WriteUInt64LittleEndian(reserved.span, (ulong)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer Patch(ReservedLengthPrefixer<long> reserved) {
				BinaryPrimitives.WriteInt64LittleEndian(reserved.span, reserved.GetLength(this));
				return this;
			}
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<ushort> reserved) {
				BinaryPrimitives.WriteUInt16BigEndian(reserved.span, (ushort)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<short> reserved) {
				BinaryPrimitives.WriteInt16BigEndian(reserved.span, (short)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<uint> reserved) {
				BinaryPrimitives.WriteUInt32BigEndian(reserved.span, (uint)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<int> reserved) {
				BinaryPrimitives.WriteInt32BigEndian(reserved.span, reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<ulong> reserved) {
				BinaryPrimitives.WriteUInt64BigEndian(reserved.span, (ulong)reserved.GetLength(this));
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer PatchBig(ReservedLengthPrefixer<long> reserved) {
				BinaryPrimitives.WriteInt64BigEndian(reserved.span, reserved.GetLength(this));
				return this;
			}
		#endregion Patching

		#region Writers
			/// <summary>Writes a single unmanaged value by copying its bytes directly.</summary>
			/// <remarks><b>This is a platform dependent operation. The endianness of the written bytes will be that of the current platform.</b></remarks>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer WriteUnmanaged<T>(in T value) where T : unmanaged {
				// We use in T and Unsafe.AsRef() because we don't need to mutate value, and AsRef simply relabled the readonly ref created by in T as a writable ref T, so we get a cleaner API
				// You can't pass properties/temporaries as ref, so callers would need to hoist to locals first, with in T, the compiler will create that temp for us at no additional runtime cost.

				// Write the bytes of 'value' directly into dest
				MemoryMarshal.Write(AllocateSpan(Unsafe.SizeOf(T)), ref Unsafe.AsRef(in value));
				return this;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Serializer WriteUnmanagedPair<T>(in T first, in T second) where T : unmanaged {
				// We use in T and Unsafe.AsRef() because we don't need to mutate value, and AsRef simply relabled the readonly ref created by in T as a writable ref T, so we get a cleaner API
				// You can't pass properties/temporaries as ref, so callers would need to hoist to locals first, with in T, the compiler will create that temp for us at no additional runtime cost.

				int sz = Unsafe.SizeOf(T);
				ref byte baseRef = ref MemoryMarshal.GetReference(AllocateSpan(sz * 2));

				var firstSpan = MemoryMarshal.CreateSpan(ref baseRef, sz);
				MemoryMarshal.Write(firstSpan, ref Unsafe.AsRef(in first));

				var secondSpan = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, sz), sz);
				MemoryMarshal.Write(secondSpan, ref Unsafe.AsRef(in second));
				
				return this;
			}
			
			/// <summary>Writes an array of unmanaged values by copying their bytes directly.</summary>
			/// <remarks><b>This is a platform dependent operation. The endianness of the written bytes will be that of the current platform.</b></remarks>
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