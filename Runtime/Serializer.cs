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
		// --- backing store ---
		byte[] _buffer;
		int _count;

		// --- flag packing state (for WriteFlag) ---
		byte _flagAccumulator;   // current byte being packed
		int  _flagBitIndex;      // 0..7 number of bits already packed (0 means none pending)
		bool _hasPendingFlagByte;

		// --- public compatibility API ---

		/// <summary>The bytes in the Serializer (enumerable view).</summary>
		public IEnumerable<byte> Bytes {
			get {
				for (int i = 0; i < _count; i++) yield return _buffer[i];
			}
		}

		/// <summary>The number of bytes in the Serializer.</summary>
		public int Length => _count;

		/// <summary>
		/// Back-compat helper for rare debugging (e.g., ConvertToHexadecimal()).
		/// Creates a fresh List with the current contents.
		/// </summary>
		public List<byte> buffer => new List<byte>(_buffer.AsSpan(0, _count).ToArray());

		/// <summary>Implicitly materialize the written bytes.</summary>
		public static implicit operator byte[](Serializer s) => s.ToArray();

		// --- construction / lifetime ---

		public Serializer() : this(0) { }

		/// <param name="capacity">Initial capacity hint in bytes.</param>
		public Serializer(int capacity) {
			if (capacity < 0) capacity = 0;
			_buffer = capacity > 0 ? new byte[capacity] : Array.Empty<byte>();
			_count = 0;
			_flagAccumulator = 0;
			_flagBitIndex = 0;
			_hasPendingFlagByte = false;
		}

		/// <summary>Clears written data and flag state; retains allocated buffer for reuse.</summary>
		public Serializer Clear() {
			_count = 0;
			_flagAccumulator = 0;
			_flagBitIndex = 0;
			_hasPendingFlagByte = false;
			return this;
		}

		/// <summary>Copies written bytes into a compact array.</summary>
		public byte[] ToArray() {
			FlushPendingFlagByte();
			if (_count == 0) return Array.Empty<byte>();
			var arr = new byte[_count];
			Buffer.BlockCopy(_buffer, 0, arr, 0, _count);
			return arr;
		}

		// --- IEnumerable<byte> ---

		public IEnumerator<byte> GetEnumerator() {
			for (int i = 0; i < _count; i++) yield return _buffer[i];
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		// --- internal core (used by QuickBinExtensions) ---

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void EnsureCapacity(int extraNeeded) {
			if (extraNeeded < 0) throw new ArgumentOutOfRangeException(nameof(extraNeeded));
			int required = _count + extraNeeded;
			if (required <= _buffer.Length) return;
			int newCap = _buffer.Length == 0 ? 256 : _buffer.Length;
			while (newCap < required) newCap = newCap < 1024 * 1024 ? newCap * 2 : newCap + (1024 * 1024); // exponential then linear
			Array.Resize(ref _buffer, newCap);
		}

		/// <summary>Returns a writable span of requested size at the current end, advancing Count.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Span<byte> GetSpan(int size) {
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0) return Span<byte>.Empty;
			FlushPendingFlagByteIfForeignWrite(); // make sure flag groups don't get interleaved
			EnsureCapacity(size);
			var span = _buffer.AsSpan(_count, size);
			_count += size;
			return span;
		}

		/// <summary>Span over the already written region (mutable for patching).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Span<byte> WrittenSpanMutable() => MemoryMarshal.CreateSpan(ref _buffer[0], _count);

		/// <summary>
		/// Generic writer for fixed-size primitives via a ByteWriter delegate (uses a pre-sized span).
		/// </summary>
		internal Serializer WriteGeneric<T>(int size, T value, ByteWriter<T> f) {
			var dest = GetSpan(size);
			f(dest, value);
			// any non-flag write breaks flag packing: handled by GetSpan() call above
			return this;
		}

		/// <summary>
		/// Single-byte writer via a small converter (no stackalloc, no delegates in hot path).
		/// </summary>
		internal Serializer WriteGeneric<T>(T value, Func<T, byte> f) {
			var dest = GetSpan(1);
			dest[0] = f(value);
			return this;
		}

		/// <summary>Bulk copy of arbitrary byte data.</summary>
		internal Serializer WriteGeneric(ReadOnlySpan<byte> value) {
			if (value.Length == 0) return this;
			var dest = GetSpan(value.Length);
			value.CopyTo(dest);
			return this;
		}

		// --- flag packing ---

		/// <summary>
		/// Packs booleans into a single byte (up to 8 per byte). Set <paramref name="forceNewByte"/> to start a new flag group.
		/// </summary>
		public Serializer WriteFlag(bool value, bool forceNewByte = false) {
			if (forceNewByte) FlushPendingFlagByte(true);

			// Start a new accumulator if none pending
			if (!_hasPendingFlagByte) {
				_flagAccumulator = 0;
				_flagBitIndex = 0;
				_hasPendingFlagByte = true;
			}

			if (value) _flagAccumulator |= (byte)(1 << _flagBitIndex);

			_flagBitIndex++;
			if (_flagBitIndex >= 8) {
				// commit the full flag byte
				var s = GetSpan(1);
				s[0] = _flagAccumulator;
				_hasPendingFlagByte = false;
				_flagAccumulator = 0;
				_flagBitIndex = 0;
			}
			return this;
		}

		/// <summary>Flushes a partially filled flag byte into the buffer (if any).</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void FlushPendingFlagByte(bool force = false) {
			if (_hasPendingFlagByte && (_flagBitIndex > 0 || force)) {
				var s = GetSpan(1);
				s[0] = _flagAccumulator;
				_hasPendingFlagByte = false;
				_flagAccumulator = 0;
				_flagBitIndex = 0;
			}
		}

		/// <summary>
		/// Called before any non-flag write to ensure we don't interleave raw data into a pending flag group.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void FlushPendingFlagByteIfForeignWrite() => FlushPendingFlagByte();

		// --- reserve & patch helpers (useful for length-prefixed blobs) ---

		/// <summary>Reserve two bytes (u16 LE) and return absolute position to patch later.</summary>
		public int ReserveU16LittleEndian() {
			FlushPendingFlagByteIfForeignWrite();
			int pos = _count;
			var s = GetSpan(2);
			s[0] = 0; s[1] = 0;
			return pos;
		}

		/// <summary>Reserve four bytes (u32 LE) and return absolute position to patch later.</summary>
		public int ReserveU32LittleEndian() {
			FlushPendingFlagByteIfForeignWrite();
			int pos = _count;
			var s = GetSpan(4);
			s[0] = s[1] = s[2] = s[3] = 0;
			return pos;
		}

		public void PatchU16LittleEndian(int absolutePos, ushort value) {
			if ((uint)absolutePos > (uint)(_count - 2)) throw new ArgumentOutOfRangeException(nameof(absolutePos));
			var whole = WrittenSpanMutable();
			BinaryPrimitives.WriteUInt16LittleEndian(whole.Slice(absolutePos, 2), value);
		}

		public void PatchU32LittleEndian(int absolutePos, uint value) {
			if ((uint)absolutePos > (uint)(_count - 4)) throw new ArgumentOutOfRangeException(nameof(absolutePos));
			var whole = WrittenSpanMutable();
			BinaryPrimitives.WriteUInt32LittleEndian(whole.Slice(absolutePos, 4), value);
		}

		// --- Primitive writers ---

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(byte value) {
			var s = GetSpan(1);
			s[0] = value;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(sbyte value) {
			var s = GetSpan(1);
			s[0] = unchecked((byte)value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(short value) {
			var s = GetSpan(2);
			BinaryPrimitives.WriteInt16LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(ushort value) {
			var s = GetSpan(2);
			BinaryPrimitives.WriteUInt16LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(int value) {
			var s = GetSpan(4);
			BinaryPrimitives.WriteInt32LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(uint value) {
			var s = GetSpan(4);
			BinaryPrimitives.WriteUInt32LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(long value) {
			var s = GetSpan(8);
			BinaryPrimitives.WriteInt64LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(ulong value) {
			var s = GetSpan(8);
			BinaryPrimitives.WriteUInt64LittleEndian(s, value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(float value) {
			// Avoid BitConverter allocations; encode via IEEE 754 bits then write int32 LE
			return Write(unchecked((int)BitConverter.SingleToInt32Bits(value)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(double value) {
			// Encode via IEEE 754 bits then write int64 LE
			return Write(unchecked((long)BitConverter.DoubleToInt64Bits(value)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(decimal value) {
			// Decimal.GetBits returns four ints (lo, mid, hi, flags). Persist as 16 bytes LE.
			int[] bits = decimal.GetBits(value);
			var s = GetSpan(16);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(0, 4),  bits[0]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(4, 4),  bits[1]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(8, 4),  bits[2]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(12, 4), bits[3]);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(char value) {
			// UTF-16 code unit, LE
			return Write((ushort)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(bool value) {
			// For structured bit-packing, prefer WriteFlag(). This exists for payload booleans.
			return Write(value ? (byte)1 : (byte)0);
		}

		// When you need to dump raw bytes directly:
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(ReadOnlySpan<byte> bytes) {
			if (bytes.Length == 0) return this;
			var s = GetSpan(bytes.Length);
			bytes.CopyTo(s);
			return this;
		}

		// --- pooled helpers ---

		/// <summary>Get a Serializer from the pool (optionally with a capacity hint).</summary>
		public static Serializer GetPooled(int capacityHint = 0) => SerializerPool.Get(capacityHint);

		/// <summary>Materialize to byte[] and return the Serializer to the pool.</summary>
		public byte[] ToArrayAndReturn() => SerializerPool.ToArrayAndReturn(this);
	}

	/// <summary>Very small, thread-unsafe pool; use from main thread in your capture loop.</summary>
	static class SerializerPool {
		static readonly Stack<Serializer> _pool = new();

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
			return new Serializer(capacityHint);
		}

		public static byte[] ToArrayAndReturn(Serializer s) {
			var arr = s.ToArray();
			_pool.Push(s);
			return arr;
		}
	}
}