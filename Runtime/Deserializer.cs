using System;
using System.Collections.Generic;
using System.Text;
using QuickBin.ChainExtensions;

namespace QuickBin {
	public sealed class Deserializer : IEnumerable<byte> {
		public readonly byte[] buffer;
		private int boolPlace = 0;
		private byte flagByte = 0;
		
		/// <summary>Returns the entire contents of the buffer, ignoring the ReadIndex and ForbiddenIndex.</summary>
		public IEnumerable<byte> Source => buffer;
		
		/// <summary>The next index in the buffer that will be read from.</summary>
		public int ReadIndex {get; private set;}
		/// <summary>The index of the first byte that is not readable by this Deserializer.</summary>
		public int ForbiddenIndex {get;}
		
		/// <summary>Whether or not the Deserializer has read all of the bytes that it is allowed to.</summary>
		public bool IsExhausted => ReadIndex >= ForbiddenIndex;
		
		/// <summary>The length of the buffer, including bytes that are not readable by this Deserializer.</summary>
		public int InternalLength => buffer.Length;
		/// <summary>The number of remaining bytes that can be read by this Deserializer.</summary>
		public int Remaining => ForbiddenIndex - ReadIndex;
		/// <summary>Whether the Deserializer has attempted to read more bytes than are in the buffer.</summary>
		public bool Overflowed {get; private set;}
		/// <summary>Indexes the buffer offset by the ReadIndex.</summary>
		/// <param name="index">The index of the byte to get.</param>
		/// <returns>The byte the specified number of indices after the ReadIndex.</returns>
		public byte this[int index] => buffer[ReadIndex + index];
		
		/// <summary>Creates a Deserializer from a byte array.</summary>
		/// <param name="buffer">The byte array to deserialize from.</param>
		/// <param name="readIndex">The index to start reading from.</param>
		/// <param name="forbiddenIndex">The index of the first byte that is not readable by this Deserializer.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is null.</exception>
		/// <remarks>By default, Deserialize initializes to little-endian byte order.</remarks>
		public Deserializer(byte[] buffer, int readIndex = 0, int? forbiddenIndex = null) {
			this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
			ReadIndex = readIndex;
			ForbiddenIndex = forbiddenIndex ?? buffer.Length;
		}

		public static implicit operator byte[](Deserializer deserializer) => deserializer.buffer;
		public static implicit operator List<byte>(Deserializer deserializer) => new(deserializer.buffer);
		
		public IEnumerator<byte> GetEnumerator() => new ArraySegment<byte>(buffer, ReadIndex, Remaining).GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		
		[Obsolete("Use Assign(buffer.Overflowed ? default : new(), out produced) instead.")]
		public Deserializer Validate<T>(Func<T> constructor, out T variable, Func<T> onOverflow = null) =>
			this.Assign(Overflowed ? (onOverflow == null ? default : onOverflow()) : constructor(), out variable);

		private static byte[] _Extract(ReadOnlySpan<byte> span) => span.ToArray();
		internal static readonly ByteReader<byte[]> Extract = _Extract;

		// The ReadGeneric method is the core of the Deserializer.
		// It handles advancing the ReadIndex, checking for overflow, and reading values from the buffer.
		internal Deserializer ReadGeneric<T>(int width, ByteReader<T> f, out T produced) {
			var nextIndex = ReadIndex + width;
			// It's okay if nextIndex == buffer.Length, because ReadIndex represents the next byte to read, not the last byte read.
			if (nextIndex > buffer.Length || nextIndex > ForbiddenIndex) {
				Overflowed = true;
				produced = default;
				return this;
			}

			produced = f(buffer.AsSpan(ReadIndex, width));
			ReadIndex = nextIndex;
			boolPlace = 0;
			return this;
		}

		internal Deserializer ReadSpan(int width, out ReadOnlySpan<byte> produced) {
			var nextIndex = ReadIndex + width;
			// It's okay if nextIndex == buffer.Length, because ReadIndex represents the next byte to read, not the last byte read.
			if (nextIndex > buffer.Length || nextIndex > ForbiddenIndex) {
				Overflowed = true;
				produced = default;
				return this;
			}

			produced = buffer.AsSpan(ReadIndex, width);
			ReadIndex = nextIndex;
			boolPlace = 0;
			return this;
		}
		
		/// <summary>A method that reads the length of a byte array from the Deserializer.</summary>
		/// <param name="buffer">The Deserializer to read the length from.</param>
		/// <param name="len">The length of the byte array.</param>
		/// <returns>Whether the Deserializer overflowed.</returns>
		public delegate bool LengthReader(Deserializer buffer, out int len);
		private static bool _Len_i64(Deserializer buffer, out int len) => buffer.Read(out long   _len).Assign((int)_len, out len).Output(buffer.Overflowed);
		private static bool _Len_u64(Deserializer buffer, out int len) => buffer.Read(out ulong  _len).Assign((int)_len, out len).Output(buffer.Overflowed);
		private static bool _Len_i32(Deserializer buffer, out int len) => buffer.Read(out int    _len).Assign(     _len, out len).Output(buffer.Overflowed);
		private static bool _Len_u32(Deserializer buffer, out int len) => buffer.Read(out uint   _len).Assign((int)_len, out len).Output(buffer.Overflowed);
		private static bool _Len_i16(Deserializer buffer, out int len) => buffer.Read(out short  _len).Assign(     _len, out len).Output(buffer.Overflowed);
		private static bool _Len_u16(Deserializer buffer, out int len) => buffer.Read(out ushort _len).Assign(     _len, out len).Output(buffer.Overflowed);
		private static bool _Len_i8 (Deserializer buffer, out int len) => buffer.Read(out sbyte  _len).Assign(     _len, out len).Output(buffer.Overflowed);
		private static bool _Len_u8 (Deserializer buffer, out int len) => buffer.Read(out byte   _len).Assign(     _len, out len).Output(buffer.Overflowed);

		// C# is so stupid. Passing static methods in as arguments causes delegate instances to be allocated on the heap. Every. Single. Time.
		// To work around that, we just make them in advance and expose those instead.
		public static readonly LengthReader Len_i64 = _Len_i64;
		public static readonly LengthReader Len_u64 = _Len_u64;
		public static readonly LengthReader Len_i32 = _Len_i32;
		public static readonly LengthReader Len_u32 = _Len_u32;
		public static readonly LengthReader Len_i16 = _Len_i16;
		public static readonly LengthReader Len_u16 = _Len_u16;
		public static readonly LengthReader Len_i8  = _Len_i8;
		public static readonly LengthReader Len_u8  = _Len_u8;
		
		/// <summary>Reads booleans from the same byte if possible.</summary>
		/// <param name="produced">The boolean that was read.</param>
		/// <param name="forceNewByte">Whether force reading from the next byte, even if there's still space for flags in the current byte.</param>
		/// <returns>This Deserializer.</returns>
		public Deserializer ReadFlag(out bool produced, bool forceNewByte = false) {
			if (forceNewByte)
				boolPlace = 0;
			
			if (boolPlace == 0)
				this.Read(out flagByte);
			
			produced = (flagByte & (1 << boolPlace)) != 0;
			
			boolPlace++;
			boolPlace %= 8;
			return this;
		}
	}
}