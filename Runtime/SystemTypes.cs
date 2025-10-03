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
		public static Serializer WriteBig(this Serializer buffer, char value) => buffer.WriteBig((ushort)value); // UTF-16 code unit, BE
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, byte value) => buffer.WriteGeneric(value, static (dest, value) => dest[0] = value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, sbyte value) => buffer.WriteGeneric(value, static (dest, value) => dest[0] = (byte)value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ushort value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt16LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, short value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt16LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, uint value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt32LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, int value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt32LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, ulong value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt64LittleEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, long value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt64LittleEndian);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, ushort value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt16BigEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, short value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt16BigEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, uint value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt32BigEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, int value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt32BigEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, ulong value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteUInt64BigEndian);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, long value) => buffer.WriteGeneric(value, BinaryPrimitives.WriteInt64BigEndian);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, float value) => buffer.Write(BitConverter.SingleToInt32Bits(value));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, double value) => buffer.Write(BitConverter.DoubleToInt64Bits(value));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, float value) => buffer.WriteBig(BitConverter.SingleToInt32Bits(value));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer WriteBig(this Serializer buffer, double value) => buffer.WriteBig(BitConverter.DoubleToInt64Bits(value));
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, decimal value) {
			int[] bits = decimal.GetBits(value);
			var s = buffer.AllocateSpan(sizeof(decimal));
			BinaryPrimitives.WriteInt32LittleEndian(s[..4],  bits[0]);
			BinaryPrimitives.WriteInt32LittleEndian(s[4..8],  bits[1]);
			BinaryPrimitives.WriteInt32LittleEndian(s[8..12],  bits[2]);
			BinaryPrimitives.WriteInt32LittleEndian(s[12..16], bits[3]);
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
		public static Serializer Write(this Serializer buffer, DateTime value) => buffer.Write(value.Ticks).Write((byte)value.Kind);

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
			buffer.bufferLength -= maxStringBytes - written;
			
			writer.write(dest, written);
			return buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Serializer Write(this Serializer buffer, string value, Serializer.LengthWriter writer) => buffer.Write(value, Encoding.UTF8, writer);
	}
}