using System;
using System.Text;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace QuickBin {
	public static partial class QuickBinExtensions {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, bool value) => buffer.Write(value ? (byte)1 : (byte)0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, char value) => buffer.Write((ushort)value); // UTF-16 code unit, LE
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, byte value) => buffer.WriteGeneric(sizeof(byte), value, static (dest, value) => dest[0] = value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, sbyte value) => buffer.WriteGeneric(sizeof(sbyte), value, static (dest, value) => dest[0] = (byte)value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, short value) => buffer.WriteGeneric(sizeof(short), value, BinaryPrimitives.WriteInt16LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ushort value) => buffer.WriteGeneric(sizeof(ushort), value, BinaryPrimitives.WriteUInt16LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, int value) => buffer.WriteGeneric(sizeof(int), value, BinaryPrimitives.WriteInt32LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, uint value) => buffer.WriteGeneric(sizeof(uint), value, BinaryPrimitives.WriteUInt32LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, long value) => buffer.WriteGeneric(sizeof(long), value, BinaryPrimitives.WriteInt64LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ulong value) => buffer.WriteGeneric(sizeof(ulong), value, BinaryPrimitives.WriteUInt64LittleEndian);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, float value) => buffer.Write(BitConverter.SingleToInt32Bits(value));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, double value) => buffer.Write(BitConverter.DoubleToInt64Bits(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, decimal value) {
			// Decimal.GetBits returns four ints (lo, mid, hi, flags). Persist as 16 bytes LE.
			int[] bits = decimal.GetBits(value);
			var s = buffer.AllocateSpan(sizeof(decimal));
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(0, 4),  bits[0]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(4, 4),  bits[1]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(8, 4),  bits[2]);
			BinaryPrimitives.WriteInt32LittleEndian(s.Slice(12, 4), bits[3]);
			return buffer;
		}
		
		/// <summary>Directly appends a span of bytes into the Serializer verbatim.</summary>
		/// <remarks>This overload does not write the number of bytes in the span into the serializer.</remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ReadOnlySpan<byte> value) {
			if (value.Length == 0) return buffer;
			var dest = buffer.AllocateSpan(value.Length);
			value.CopyTo(dest);
			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, DateTime value) => buffer.Write(value.Ticks);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, TimeSpan value) => buffer.Write(value.Ticks);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, Version value) => buffer.Write(value.Major).Write(value.Minor).Write(value.Build).Write(value.Revision);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ReadOnlySpan<byte> value, Serializer.LengthWriter writer) {
			var dest = buffer.AllocateSpan(value.Length + writer.dataSize);
			writer.write(dest, value.Length);
			value.CopyTo(dest[writer.dataSize..]);
			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, string value, Encoding encoding, Serializer.LengthWriter writer) {
			if (string.IsNullOrEmpty(value)) {
				writer.write(buffer.AllocateSpan(writer.dataSize), 0);
				return buffer;
			}
			
			var maxStringBytes = encoding.GetMaxByteCount(value.Length);
			var dest = buffer.AllocateSpan(maxStringBytes + writer.dataSize);

			var enc = encoding.GetEncoder();
			var written = enc.GetBytes(value, dest[writer.dataSize..], true);
			buffer.length -= maxStringBytes - written;
			
			writer.write(dest, written);
			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, string value, Serializer.LengthWriter writer) => buffer.Write(value, Encoding.UTF8, writer);
	}
}