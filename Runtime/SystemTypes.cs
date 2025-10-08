using System;
using System.Text;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace QuickBin {
	using ChainExtensions;
	public static partial class QuickBinExtensions {
		#region bool
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, bool value) => buffer.Write(value ? (byte)1 : (byte)0);
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out bool produced) => buffer.ReadGeneric(sizeof(bool),   BitConverter.ToBoolean,      out produced);
		#endregion bool

		#region char
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, char value) => buffer.Write((ushort)value); // UTF-16 code unit, LE
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer WriteBig(this Serializer buffer, char value) => buffer.WriteBig((ushort)value); // UTF-16 code unit, BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out char produced) => buffer.ReadGeneric(sizeof(char),   BitConverter.ToChar,         out produced);
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

			/// <summary>Reads a string from the Deserializer.</summary>
			/// <param name="produced">The string that was read.</param>
			/// <param name="encoding">The encoding to use.</param>
			/// <param name="length">The length of the string in bytes. Defaults to the remaining bytes in the buffer.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out string produced, Encoding encoding, int? length = null) => buffer
				.ReadGeneric(length ?? buffer.Remaining, encoding.GetString, out produced);
			
			/// <summary>Reads a UTF-8 string from the Deserializer.</summary>
			/// <param name="produced">The string that was read.</param>
			/// <param name="length">The length of the string in bytes.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out string produced, int? length = null) => buffer
				.Read(out produced, Encoding.UTF8, length);
				
			/// <summary>Reads a string from the Deserializer.</summary>
			/// <param name="produced">The string that was read.</param>
			/// <param name="encoding">The encoding to use.</param>
			/// <param name="readLen">The method to read out the length of the string. (e.g. <c>Len_i32</c>)</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out string produced, Encoding encoding, Deserializer.LengthReader readLen) {
				if (readLen(buffer, out var len)) {
					produced = default;
					return buffer;
				}
				return buffer.Read(out produced, encoding, len);
			}
			
			/// <summary>Reads a UTF-8 string from the Deserializer.</summary>
			/// <param name="produced">The string that was read.</param>
			/// <param name="readLen">The method to read out the length of the string. (e.g. <c>Len_i32</c>)</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out string produced, Deserializer.LengthReader readLen) => buffer
				.Read(out produced, Encoding.UTF8, readLen);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out byte produced) => buffer.ReadGeneric(sizeof(byte),   (span) => span[0],           out produced);
			
				/// <summary>Reads a byte array from the Deserializer.</summary>
				/// <param name="produced">The byte array that was read.</param>
				/// <param name="length">The length of the byte array in bytes. Defaults to the remaining bytes in the buffer.</param>
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out byte[] produced, int? length = null) => buffer
					.ReadGeneric(length ?? buffer.Remaining, Deserializer.Extract, out produced);
			
				/// <summary>Reads a byte array from the Deserializer.</summary>
				/// <param name="produced">The byte array that was read.</param>
				/// <param name="readLen">The method to read out the length of the byte array. (e.g. <c>Len_i32</c>)</param>
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out byte[] produced, Deserializer.LengthReader readLen) {
					if (readLen(buffer, out var len)) {
						produced = default;
						return buffer;
					}
					return buffer.Read(out produced, len);
				}
				
				/// <summary>Reads a byte array from the Deserializer.</summary>
				/// <param name="produced">The byte array that was read.</param>
				/// <param name="length">The length of the byte array in bytes. Defaults to the remaining bytes in the buffer.</param>
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out ReadOnlySpan<byte> produced, int? length = null) => buffer
					.ReadSpan(length ?? buffer.Remaining, out produced);
			
				/// <summary>Reads a byte span from the Deserializer.</summary>
				/// <param name="produced">The byte span that was read.</param>
				/// <param name="readLen">The method to read out the length of the byte span. (e.g. <c>Len_i32</c>)</param>
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out ReadOnlySpan<byte> produced, Deserializer.LengthReader readLen) {
					if (readLen(buffer, out var len)) {
						produced = default;
						return buffer;
					}
					return buffer.Read(out produced, len);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out sbyte produced) => buffer.ReadGeneric(sizeof(sbyte),  (span) => (sbyte)span[0],    out produced);
			#endregion sbyte
		#endregion 8-bit

		#region 16-bit
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out ushort produced, Endianness endianness) => buffer.ReadGeneric(sizeof(ushort), endianness.read_u16, out produced);
			
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out ushort produced) => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out ushort produced) => buffer.Read(out produced, Endianness.big);
			#endregion ushort BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out short produced,  Endianness endianness) => buffer.ReadGeneric(sizeof(short),  endianness.read_i16, out produced);

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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out short produced)  => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out short produced)  => buffer.Read(out produced, Endianness.big);
			#endregion short BE
		#endregion 16-bit

		#region 32-bit
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out uint produced,   Endianness endianness) => buffer.ReadGeneric(sizeof(uint),   endianness.read_u32, out produced);
			
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out uint produced)   => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out uint produced)   => buffer.Read(out produced, Endianness.big);
			#endregion uint BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out int produced,    Endianness endianness) => buffer.ReadGeneric(sizeof(int),    endianness.read_i32, out produced);

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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out int produced)    => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out int produced)    => buffer.Read(out produced, Endianness.big);
			#endregion int BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out float produced, Endianness endianness) =>
				buffer.ReadGeneric(sizeof(float), endianness.read_i32, out int value).Assign(BitConverter.Int32BitsToSingle(value), out produced);

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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out float produced)  => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out float produced)  => buffer.Read(out produced, Endianness.big);
			#endregion float BE
		#endregion 32-bit

		#region 64-bit
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out ulong produced,  Endianness endianness) => buffer.ReadGeneric(sizeof(ulong),  endianness.read_u64, out produced);
			
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out ulong produced)  => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out ulong produced)  => buffer.Read(out produced, Endianness.big);
			#endregion ulong BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out long produced,   Endianness endianness) => buffer.ReadGeneric(sizeof(long),   endianness.read_i64, out produced);

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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out long produced)   => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out long produced)   => buffer.Read(out produced, Endianness.big);
			#endregion long BE
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static Deserializer Read(this Deserializer buffer, out double produced, Endianness endianness) =>
				buffer.ReadGeneric(sizeof(double), endianness.read_i64, out long value).Assign(BitConverter.Int64BitsToDouble(value), out produced);

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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer Read(this Deserializer buffer, out double produced) => buffer.Read(out produced, Endianness.little);
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
				
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public static Deserializer ReadBig(this Deserializer buffer, out double produced) => buffer.Read(out produced, Endianness.big);
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
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out decimal value) => buffer
					.Read(out int i1)
					.Read(out int i2)
					.Read(out int i3)
					.Read(out int i4)
					.Assign(new decimal(new int[] { i1, i2, i3, i4 }), out value);
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
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer ReadBig(this Deserializer buffer, out decimal value) => buffer
				.ReadBig(out int i1)
				.ReadBig(out int i2)
				.ReadBig(out int i3)
				.ReadBig(out int i4)
				.Assign(new decimal(new int[] { i1, i2, i3, i4 }), out value);
		#endregion decimal BE
		
		#region DateTime
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, DateTime value) => buffer.Write(value.Ticks).Write((byte)value.Kind);
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out DateTime produced) => buffer
				.Read(out long ticks)
				.Read(out byte kind)
				.Assign(ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks && Enum.IsDefined(typeof(DateTimeKind), kind) ? new(ticks, (DateTimeKind)kind) : default, out produced);
		#endregion DateTime

		#region TimeSpan
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, TimeSpan value) => buffer.Write(value.Ticks);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out TimeSpan produced) => buffer
				.Read(out long ticks)
				.Assign(new(ticks), out produced);
		#endregion TimeSpan

		#region Version
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Version value) => buffer.Write(stackalloc int[4] { value.Major, value.Minor, value.Build, value.Revision });
			
			const int SIGNLESS_MASK = 0b0111_1111_1111_1111;
			/// <note>Version does not support negative values, so the sign bit is completely ignored by this method.
			/// Note that this is not the same thing as the absolute value.</note>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out Version produced) => buffer
				.Read(out uint major)
				.Read(out uint minor)
				.Read(out uint build)
				.Read(out uint revision)
				.Assign(new(
					(int)(major & SIGNLESS_MASK),
					(int)(minor & SIGNLESS_MASK),
					(int)(build & SIGNLESS_MASK),
					(int)(revision & SIGNLESS_MASK)
				), out produced);
		#endregion Version
			
		#region Deserializer
			/// <summary>Creates a new Deserializer from a subsection of the current Deserializer.</summary>
			/// <param name="produced">The Deserializer that was created.</param>
			/// <param name="length">The length of the byte array in bytes. Defaults to the remaining bytes in the buffer.</param>
			/// <returns>This Deserializer.</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out Deserializer produced, int? length = null) => buffer
				.Assign(new(buffer, buffer.ReadIndex, buffer.ReadIndex + (length ?? buffer.Remaining)), out produced);
			
			/// <summary>Creates a new Deserializer from a subsection of the current Deserializer.</summary>
			/// <param name="produced">The Deserializer that was created.</param>
			/// <param name="readLen">The method to read out the length of the byte array. (e.g. <c>Len_i32</c>)</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Deserializer Read(this Deserializer buffer, out Deserializer produced, Deserializer.LengthReader readLen) {
				if (readLen(buffer, out var len)) {
					produced = new Deserializer(Array.Empty<byte>());
					return buffer;
				}
				return buffer.Read(out produced, len);
			}
		#endregion Deserializer
	}
}