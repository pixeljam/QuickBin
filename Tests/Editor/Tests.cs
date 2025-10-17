using NUnit.Framework;
using QuickBin.ChainExtensions;
using System;
using System.Linq;
using System.Text;

namespace QuickBin.Tests {
	public static partial class MainFeatures {
		[Test]
		public static void MaxValues() {
			var buffer = new Serializer()
				.Write(byte.MaxValue)
				.Write(sbyte.MaxValue)
				.Write(ushort.MaxValue)
				.Write(short.MaxValue)
				.Write(uint.MaxValue)
				.Write(int.MaxValue)
				.Write(ulong.MaxValue)
				.Write(long.MaxValue);
			
			new Deserializer(buffer)
				.Read(out byte a)
				.Read(out sbyte b)
				.Read(out ushort c)
				.Read(out short d)
				.Read(out uint e)
				.Read(out int f)
				.Read(out ulong g)
				.Read(out long h);
			
			Assert.AreEqual(byte.MaxValue, a);
			Assert.AreEqual(sbyte.MaxValue, b);
			Assert.AreEqual(ushort.MaxValue, c);
			Assert.AreEqual(short.MaxValue, d);
			Assert.AreEqual(uint.MaxValue, e);
			Assert.AreEqual(int.MaxValue, f);
			Assert.AreEqual(ulong.MaxValue, g);
			Assert.AreEqual(long.MaxValue, h);
		}
		
		[Test]
		public static void FlagBools() {
			var testString = "Hello, world!";
			
			var buffer = new Serializer()
				.WriteFlag(false)
				.WriteFlag(true)
				.WriteFlag(true)
				.WriteFlag(false)
				.WriteFlag(true)
				.WriteFlag(false)
				.WriteFlag(false)
				.WriteFlag(false)
				.WriteFlag(true)
				.WriteFlag(true)
				.WriteFlag(false, true)
				.WriteFlag(true)
				.Write(10.5f)
				.WriteFlag(true)
				.Write(testString, Serializer.Len_i32)
				.WriteFlag(false)
				.WriteFlag(true);
			
			Assert.AreEqual((byte)0b0001_0110, ((byte[])buffer)[0]);
			Assert.AreEqual((byte)0b0000_0011, ((byte[])buffer)[1]);
			Assert.AreEqual((byte)0b0000_0010, ((byte[])buffer)[2]);
			
			new Deserializer(buffer)
				.ReadFlag(out bool a)
				.ReadFlag(out bool b)
				.ReadFlag(out bool c)
				.ReadFlag(out bool d)
				.ReadFlag(out bool e)
				.ReadFlag(out bool f)
				.ReadFlag(out bool g)
				.ReadFlag(out bool h)
				.ReadFlag(out bool i)
				.ReadFlag(out bool j)
				.ReadFlag(out bool k, true)
				.ReadFlag(out bool l)
				.Read(out float m)
				.ReadFlag(out bool n)
				.Read(out int length)
				.Read(out string o, length)
				.ReadFlag(out bool p)
				.ReadFlag(out bool q);
			
			Assert.IsFalse(a);
			Assert.IsTrue(b);
			Assert.IsTrue(c);
			Assert.IsFalse(d);
			Assert.IsTrue(e);
			Assert.IsFalse(f);
			Assert.IsFalse(g);
			Assert.IsFalse(h);
			Assert.IsTrue(i);
			Assert.IsTrue(j);
			Assert.IsFalse(k);
			Assert.IsTrue(l);
			Assert.AreEqual(10.5f, m);
			Assert.IsTrue(n);
			Assert.AreEqual(testString, o);
			Assert.IsFalse(p);
			Assert.IsTrue(q);
		}
		
		[Test]
		public static void StringEncoding() {
			var testString = "Hello, World!";
			
			var buffer = new Serializer()
				.Write(testString, Serializer.Len_i64)
				.Write(testString, Serializer.Len_i32)
				.Write(testString, Serializer.Len_i16)
				.Write(testString, Serializer.Len_i8)
				.Write(testString, Serializer.Len_u64)
				.Write(testString, Serializer.Len_u32)
				.Write(testString, Serializer.Len_u16)
				.Write(testString, Serializer.Len_u8)
				.Write(testString, Encoding.ASCII, Serializer.Len_i32)
				.Write(testString, Encoding.BigEndianUnicode, Serializer.Len_i32)
				.Write(testString, Encoding.Unicode, Serializer.Len_i32)
				.Write(testString, Encoding.UTF32, Serializer.Len_i32)
				.Write(testString, Encoding.UTF7, Serializer.Len_i32)
				.Write(testString, Encoding.UTF8, Serializer.Len_i32);
			
			new Deserializer(buffer)
				.Read(out string str_long, Deserializer.Len_i64)
				.Read(out string str_int, Deserializer.Len_i32)
				.Read(out string str_short, Deserializer.Len_i16)
				.Read(out string str_sbyte, Deserializer.Len_i8)
				.Read(out string str_ulong, Deserializer.Len_u64)
				.Read(out string str_uint, Deserializer.Len_u32)
				.Read(out string str_ushort, Deserializer.Len_u16)
				.Read(out string str_byte, Deserializer.Len_u8)
				.Read(out string ascii, Encoding.ASCII, Deserializer.Len_i32)
				.Read(out string bigEndianUnicode, Encoding.BigEndianUnicode, Deserializer.Len_i32)
				.Read(out string unicode, Encoding.Unicode, Deserializer.Len_i32)
				.Read(out string utf32, Encoding.UTF32, Deserializer.Len_i32)
				.Read(out string utf7, Encoding.UTF7, Deserializer.Len_i32)
				.Read(out string utf8, Encoding.UTF8, Deserializer.Len_i32);
			
			Assert.AreEqual(testString, str_long);
			Assert.AreEqual(testString, str_int);
			Assert.AreEqual(testString, str_short);
			Assert.AreEqual(testString, str_sbyte);
			Assert.AreEqual(testString, str_ulong);
			Assert.AreEqual(testString, str_uint);
			Assert.AreEqual(testString, str_ushort);
			Assert.AreEqual(testString, str_byte);
			
			Assert.AreEqual(testString, ascii);
			Assert.AreEqual(testString, bigEndianUnicode);
			Assert.AreEqual(testString, unicode);
			Assert.AreEqual(testString, utf32);
			Assert.AreEqual(testString, utf7);
			Assert.AreEqual(testString, utf8);
		}
		
		[Test]
		public static void Overflow() {
			var buffer = new Serializer()
				.Write(10)
				.Write("Foo", Serializer.Len_i32);

			var reader = new Deserializer(buffer)
				.Read(out int good_a)
				.Read(out string good_b, Deserializer.Len_i32);
			
			Assert.AreEqual(10, good_a);
			Assert.AreEqual("Foo", good_b);
			Assert.IsFalse(reader.Overflowed);

			reader = new Deserializer(buffer)
				.Read(out int bad_a)
				.Read(out string bad_b, Deserializer.Len_i32)
				.Read(out int bad_c)
				.Read(out string bad_d, Deserializer.Len_i32);
			
			Assert.AreEqual(10, bad_a);
			Assert.AreEqual("Foo", bad_b);
			Assert.AreEqual(default(int), bad_c);
			Assert.AreEqual(default(string), bad_d);
			Assert.IsTrue(reader.Overflowed);
		}
		
		[Test]
		public static void WriteReadMany() {
			var arr = new int[] {1, 2, 3, 4, 5};
			
			var serializer = new Serializer()
				.Write((ushort)arr.Length)
				.ForEach(arr, (buffer, value) => buffer.Write(value));
			
			var deserializer = new Deserializer(serializer)
				.Read(out ushort count)
				.ForEach(out var produced, buffer => buffer.Read(out int value).Output(value), count);
			
			Assert.AreEqual(arr, produced.ToArray());
		}
		
		[Test]
		public static void WriteReadArray() {
			var arr = new byte[] {1, 2, 3, 4, 5};
			
			var serializer = new Serializer()
				.Write(arr, Serializer.Len_u8)
				.Write(arr);
			
			var deserializer = new Deserializer(serializer)
				.Read(out byte[] lengthArr, Deserializer.Len_u8)
				.Read(out byte[] noLengthArr);
			
			Assert.AreEqual(arr, lengthArr);
			Assert.AreEqual(arr, noLengthArr);
			
			deserializer = new Deserializer(serializer)
				.Read(out byte[] everything);
			
			Assert.AreEqual(arr.Prepend((byte)arr.Length).Concat(arr), everything);
		}
		
		[Test]
		public static void Endianness() {
			var buffer = new Serializer()
				.Write((ushort)0x1234)
				.WriteBig((ushort)0x1234)
				.Write((ushort)0x1234);
			
			byte[] bytes = buffer;
			
			Assert.AreEqual(new byte[] {0x34, 0x12}, bytes[0..2]);
			Assert.AreEqual(new byte[] {0x12, 0x34}, bytes[2..4]);
			Assert.AreEqual(new byte[] {0x34, 0x12}, bytes[4..6]);
			
			new Deserializer(bytes)
				.Read(out ushort littleEndianA)
				.ReadBig(out ushort bigEndian)
				.Read(out ushort littleEndianB);
			
			Assert.AreEqual(0x1234, littleEndianA);
			Assert.AreEqual(0x1234, bigEndian);
			Assert.AreEqual(0x1234, littleEndianB);
		}
		
		[Test]
		public static void VersionGarbage() {
			var buffer = new Serializer()
				.Write(0b1001_0110_1111_0000)
				.Write(0b1101_0110_0011_0100)
				.Write(0b1001_0000_1011_0111)
				.Write(0b0001_1100_1101_0001);
			
			new Deserializer(buffer)
				.Read(out Version version);
			
			Assert.IsTrue(version.Equals(new Version(0b0001_0110_1111_0000, 0b0101_0110_0011_0100, 0b0001_0000_1011_0111, 0b0001_1100_1101_0001)));
		}
		
		[Test]
		public static void LengthPatching() {
			var buffer = new Serializer()
				.Write(1234)
				.WriteFlag(true)
				.ReserveLength<ushort>(out var prefixer)
				.WriteFlag(true)
				.WriteFlag(false)
				.Write((long)18)
				.Patch(prefixer)
				.WriteFlag(false)
				.WriteFlag(true);
			
			new Deserializer(buffer)
				.Read(out int a)
				.ReadFlag(out bool b)
				.Read(out ushort length)
				.ReadFlag(out bool c)
				.ReadFlag(out bool d)
				.Read(out long e)
				.ReadFlag(out bool f)
				.ReadFlag(out bool g);
			
			Assert.AreEqual(1234, a);
			Assert.IsTrue(b);
			Assert.AreEqual((ushort)(1 + sizeof(long)), length);
			Assert.IsTrue(c);
			Assert.IsFalse(d);
			Assert.AreEqual(18, e);
			Assert.IsFalse(f);
			Assert.IsTrue(g);
		}
		
		[Test]
		public static void BulkWriting() {
			var buffer = new Serializer()
				.Write(stackalloc byte[] { 0, 1, 2 })
				.Write(stackalloc sbyte[] { 3, -4, 5 })
				.Write(stackalloc ushort[] { 6, 7, 8 })
				.Write(stackalloc short[] { 9, -10, 11 })
				.Write(stackalloc uint[] { 12, 13, 14 })
				.Write(stackalloc int[] { 15, -16, 17 })
				.Write(stackalloc ulong[] { 18, 19, 20 })
				.Write(stackalloc long[] { 21, -22, 23 })
				.Write(stackalloc float[] { 0.1f, -0.2f, 0.3f })
				.Write(stackalloc double[] { 0.4f, -0.5f, 0.6f });
			
			new Deserializer(buffer)
				.Read(out byte byte1)
				.Read(out byte byte2)
				.Read(out byte byte3)
				.Read(out sbyte sbyte1)
				.Read(out sbyte sbyte2)
				.Read(out sbyte sbyte3)
				.Read(out ushort ushort1)
				.Read(out ushort ushort2)
				.Read(out ushort ushort3)
				.Read(out short short1)
				.Read(out short short2)
				.Read(out short short3)
				.Read(out uint uint1)
				.Read(out uint uint2)
				.Read(out uint uint3)
				.Read(out int int1)
				.Read(out int int2)
				.Read(out int int3)
				.Read(out ulong ulong1)
				.Read(out ulong ulong2)
				.Read(out ulong ulong3)
				.Read(out long long1)
				.Read(out long long2)
				.Read(out long long3)
				.Read(out float float1)
				.Read(out float float2)
				.Read(out float float3)
				.Read(out double double1)
				.Read(out double double2)
				.Read(out double double3);
			
			Assert.AreEqual(0, byte1);
			Assert.AreEqual(1, byte2);
			Assert.AreEqual(2, byte3);
			Assert.AreEqual(3, sbyte1);
			Assert.AreEqual(-4, sbyte2);
			Assert.AreEqual(5, sbyte3);
			Assert.AreEqual(6, ushort1);
			Assert.AreEqual(7, ushort2);
			Assert.AreEqual(8, ushort3);
			Assert.AreEqual(9, short1);
			Assert.AreEqual(-10, short2);
			Assert.AreEqual(11, short3);
			Assert.AreEqual(12, uint1);
			Assert.AreEqual(13, uint2);
			Assert.AreEqual(14, uint3);
			Assert.AreEqual(15, int1);
			Assert.AreEqual(-16, int2);
			Assert.AreEqual(17, int3);
			Assert.AreEqual(18, ulong1);
			Assert.AreEqual(19, ulong2);
			Assert.AreEqual(20, ulong3);
			Assert.AreEqual(21, long1);
			Assert.AreEqual(-22, long2);
			Assert.AreEqual(23, long3);
			Assert.AreEqual(0.1f, float1);
			Assert.AreEqual(-0.2f, float2);
			Assert.AreEqual(0.3f, float3);
			Assert.AreEqual(0.4f, double1);
			Assert.AreEqual(-0.5f, double2);
			Assert.AreEqual(0.6f, double3);
		}
	}
	
	// The following test ensures that even if a type can be implicitly cast to another type, the correct Write method is called.
	// As it turns out, this test will fail if the Write and Read methods are implemented directly in the Serializer and Deserializer classes,
	// instead of as extension methods. You can read the explanation in the summary of the QuickBinExtensions class.
	internal sealed class Castable {
		public int Value;
		public Castable(int value) => Value = value;
		public static implicit operator bool(Castable castable) => castable.Value != 0;
	}
	
	internal static partial class QuickBinExtensions {
		public static Serializer Write(this Serializer buffer, Castable value) => buffer
			.Write(value.Value);
	}
	
	public static partial class MainFeatures {
		[Test]
		public static void WriteWithoutCasting() {
			var buffer = new Serializer()
				.Write(new Castable(0));
			
			Assert.AreEqual(sizeof(int), buffer.Length);
		}
	}
}