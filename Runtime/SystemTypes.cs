using System;
using System.Text;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace QuickBin {
	public static partial class QuickBinExtensions {
		#region bool
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, bool value) => buffer.Write(value ? (byte)1 : (byte)0);
		#endregion bool

		#region char
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, char value) => buffer.Write((ushort)value); // UTF-16 code unit, LE
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, char value) => buffer.WriteBig((ushort)value); // UTF-16 code unit, BE
		#endregion char

		#region string
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
		#endregion
		
		#region 8-bit
			#region byte
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, byte value) {
					buffer.AllocateSpan(sizeof(byte))[0] = value;
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<byte> values) {
					if (values.Length == 0) return buffer;
					values.CopyTo(buffer.AllocateSpan(values.Length));
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<byte> value, Serializer.LengthWriter writer) {
					var dest = buffer.AllocateSpan(value.Length + writer.dataSize);
					writer.write(dest, value.Length);
					value.CopyTo(dest[writer.dataSize..]);
					return buffer;
				}
			#endregion byte

			#region sbyte
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, sbyte value) {
					buffer.AllocateSpan(sizeof(byte))[0] = (byte)value;
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<sbyte> values) {
					if (values.Length == 0) return buffer;
					var src = MemoryMarshal.AsBytes(values);
					src.CopyTo(buffer.AllocateSpan(src.Length));
					return buffer;
				}
			#endregion sbyte
		#endregion 8-bit

		#region 16-bit
			#region ushort LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ushort value) {
					BinaryPrimitives.WriteUInt16LittleEndian(buffer.AllocateSpan(sizeof(ushort)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<ushort> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion ushort LE

			#region ushort BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ushort value) {
					BinaryPrimitives.WriteUInt16BigEndian(buffer.AllocateSpan(sizeof(ushort)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<ushort> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(ushort);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteUInt16BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion ushort BE

			#region short LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, short value) {
					BinaryPrimitives.WriteInt16LittleEndian(buffer.AllocateSpan(sizeof(short)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<short> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion short LE

			#region short BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, short value) {
					BinaryPrimitives.WriteInt16BigEndian(buffer.AllocateSpan(sizeof(short)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<short> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(short);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteInt16BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion short BE
		#endregion 16-bit

		#region 32-bit
			#region uint LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, uint value) {
					BinaryPrimitives.WriteUInt32LittleEndian(buffer.AllocateSpan(sizeof(uint)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<uint> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion uint LE

			#region uint BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, uint value) {
					BinaryPrimitives.WriteUInt32BigEndian(buffer.AllocateSpan(sizeof(uint)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<uint> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(uint);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteUInt32BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion uint BE

			#region int LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, int value) {
					BinaryPrimitives.WriteInt32LittleEndian(buffer.AllocateSpan(sizeof(int)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<int> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion int LE

			#region int BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, int value) {
					BinaryPrimitives.WriteInt32BigEndian(buffer.AllocateSpan(sizeof(int)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<int> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(int);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteInt32BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion int BE

			#region float LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, float value) {
					BinaryPrimitives.WriteInt32LittleEndian(buffer.AllocateSpan(sizeof(int)), BitConverter.SingleToInt32Bits(value));
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<float> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion float LE

			#region float BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, float value) {
					BinaryPrimitives.WriteInt32BigEndian(buffer.AllocateSpan(sizeof(int)), BitConverter.SingleToInt32Bits(value));
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<float> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(int);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteInt32BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), BitConverter.SingleToInt32Bits(values[i]));
					return buffer;
				}
			#endregion float BE
		#endregion 32-bit

		#region 64-bit
			#region ulong LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ulong value) {
					BinaryPrimitives.WriteUInt64LittleEndian(buffer.AllocateSpan(sizeof(ulong)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<ulong> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion ulong LE

			#region ulong BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ulong value) {
					BinaryPrimitives.WriteUInt64BigEndian(buffer.AllocateSpan(sizeof(ulong)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<ulong> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(ulong);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteUInt64BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion ulong BE

			#region long LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, long value) {
					BinaryPrimitives.WriteInt64LittleEndian(buffer.AllocateSpan(sizeof(long)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<long> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion long LE

			#region long BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, long value) {
					BinaryPrimitives.WriteInt64BigEndian(buffer.AllocateSpan(sizeof(long)), value);
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<long> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(long);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteInt64BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), values[i]);
					return buffer;
				}
			#endregion long BE

			#region double LE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, double value) {
					BinaryPrimitives.WriteInt64LittleEndian(buffer.AllocateSpan(sizeof(long)), BitConverter.DoubleToInt64Bits(value));
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer Write(this Serializer buffer, ReadOnlySpan<double> values) {
					if (values.Length == 0) return buffer;
					var bytes = MemoryMarshal.AsBytes(values);
					bytes.CopyTo(buffer.AllocateSpan(bytes.Length));
					return buffer;
				}
			#endregion double LE

			#region double BE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, double value) {
					BinaryPrimitives.WriteInt64BigEndian(buffer.AllocateSpan(sizeof(long)), BitConverter.DoubleToInt64Bits(value));
					return buffer;
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Serializer WriteBig(this Serializer buffer, ReadOnlySpan<double> values) {
					if (values.Length == 0) return buffer;
					const int SZ = sizeof(long);
					ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * SZ));
					for (int i = 0, offset = 0; i < values.Length; i++, offset += SZ)
						BinaryPrimitives.WriteInt64BigEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), SZ), BitConverter.DoubleToInt64Bits(values[i]));
					return buffer;
				}
			#endregion double BE
		#endregion 64-bit
		
		#region decimal LE
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, decimal value) {
				int[] bits = decimal.GetBits(value);
				var s = buffer.AllocateSpan(sizeof(decimal));
				BinaryPrimitives.WriteInt32LittleEndian(s[..4],    bits[0]);
				BinaryPrimitives.WriteInt32LittleEndian(s[4..8],   bits[1]);
				BinaryPrimitives.WriteInt32LittleEndian(s[8..12],  bits[2]);
				BinaryPrimitives.WriteInt32LittleEndian(s[12..16], bits[3]);
				return buffer;
			}
		#endregion decimal LE

		#region decimal BE
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, decimal value) {
				int[] bits = decimal.GetBits(value);
				var s = buffer.AllocateSpan(sizeof(decimal));
				BinaryPrimitives.WriteInt32BigEndian(s[..4],    bits[0]);
				BinaryPrimitives.WriteInt32BigEndian(s[4..8],   bits[1]);
				BinaryPrimitives.WriteInt32BigEndian(s[8..12],  bits[2]);
				BinaryPrimitives.WriteInt32BigEndian(s[12..16], bits[3]);
				return buffer;
			}
		#endregion decimal BE
		
		#region DateTime
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, DateTime value) => buffer.Write(value.Ticks).Write((byte)value.Kind);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, DateTime value) => buffer.WriteBig(value.Ticks).Write((byte)value.Kind);
		#endregion DateTime

		#region TimeSpan
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, TimeSpan value) => buffer.Write(value.Ticks);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, TimeSpan value) => buffer.WriteBig(value.Ticks);
		#endregion TimeSpan

		#region Version
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Version value) => buffer.Write(stackalloc int[4] { value.Major, value.Minor, value.Build, value.Revision });
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, Version value) => buffer.WriteBig(stackalloc int[4] { value.Major, value.Minor, value.Build, value.Revision });
		#endregion Version
	}
}