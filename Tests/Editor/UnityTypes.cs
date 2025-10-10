using NUnit.Framework;
using QuickBin.ChainExtensions;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QuickBin.Tests {
	public static partial class UnityTypes {
		[Test]
		public static void _Vector2() {
			Vector2 a_in = new(-10, 3);
			Vector2 b_in = new(-4, 11);
			Vector2 c_in = new(12, -5);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Vector2[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Vector2 a_out)
				.Read(out Vector2 b_out)
				.Read(out Vector2 c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Vector2Int() {
			Vector2Int a_in = new(-10, 3);
			Vector2Int b_in = new(-4, 11);
			Vector2Int c_in = new(12, -5);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Vector2Int[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Vector2Int a_out)
				.Read(out Vector2Int b_out)
				.Read(out Vector2Int c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Vector3() {
			Vector3 a_in = new(-10, 3, 22);
			Vector3 b_in = new(-4, 11, -23);
			Vector3 c_in = new(12, -5, 24);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Vector3[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Vector3 a_out)
				.Read(out Vector3 b_out)
				.Read(out Vector3 c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Vector3Int() {
			Vector3Int a_in = new(-10, 3, 22);
			Vector3Int b_in = new(-4, 11, -23);
			Vector3Int c_in = new(12, -5, 24);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Vector3Int[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Vector3Int a_out)
				.Read(out Vector3Int b_out)
				.Read(out Vector3Int c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Vector4() {
			Vector4 a_in = new(-10, 3, 22, 35);
			Vector4 b_in = new(-4, 11, -23, -36);
			Vector4 c_in = new(12, -5, 24, -37);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Vector4[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Vector4 a_out)
				.Read(out Vector4 b_out)
				.Read(out Vector4 c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Quaternion() {
			var a_in = new Quaternion(-10, 3, 22, 35).normalized;
			var b_in = new Quaternion(-4, 11, -23, -36).normalized;
			var c_in = new Quaternion(12, -5, 24, -37).normalized;
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Quaternion[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Quaternion a_out)
				.Read(out Quaternion b_out)
				.Read(out Quaternion c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Color() {
			Color a_in = new(0.10f, 0.3f, 0.22f, 0.35f);
			Color b_in = new(0.4f, 0.11f, 0.23f, 0.36f);
			Color c_in = new(0.12f, 0.5f, 0.24f, 0.37f);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Color[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Color a_out)
				.Read(out Color b_out)
				.Read(out Color c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Color32() {
			Color32 a_in = new(10, 3, 22, 35);
			Color32 b_in = new(4, 11, 23, 36);
			Color32 c_in = new(12, 5, 24, 37);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Color32[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Color32 a_out)
				.Read(out Color32 b_out)
				.Read(out Color32 c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Rect() {
			Rect a_in = new(-10, 3, 22, 35);
			Rect b_in = new(4, -11, 23, 36);
			Rect c_in = new(-12, -5, 24, 37);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Rect[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Rect a_out)
				.Read(out Rect b_out)
				.Read(out Rect c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _RectInt() {
			RectInt a_in = new(-10, 3, 22, 35);
			RectInt b_in = new(4, -11, 23, 36);
			RectInt c_in = new(-12, -5, 24, 37);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc RectInt[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out RectInt a_out)
				.Read(out RectInt b_out)
				.Read(out RectInt c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Matrix4x4() {
			Matrix4x4 a_in = new(
				new(1, 3, 22, 35),
				new(-5, 1, 0.2f, 4),
				new(0, 0, 1, 0),
				new(3.2f, -0.3f, 1, 1)
			);
			Matrix4x4 b_in = new(
				new(1, 3, 22, 35),
				new(-5, 1, 0.3f, 4),
				new(0, 0, 1, 0),
				new(3.2f, -0.3f, 1, 1)
			);
			Matrix4x4 c_in = new(
				new(1, 3, 22, 35),
				new(-5, 1, 0.4f, 4),
				new(0, 0, 1, 0),
				new(3.2f, -0.3f, 1, 1)
			);
			
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Matrix4x4[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Matrix4x4 a_out)
				.Read(out Matrix4x4 b_out)
				.Read(out Matrix4x4 c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _Bounds() {
			Bounds a_in = new(new(1, 3, 22), new(5, 1, 0.3f));
			Bounds b_in = new(new(4, -11, 23), new (3.2f, 0.3f, 1));
			Bounds c_in = new(new(-12, -5, 24), new(37, 1, 36));
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Bounds[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out Bounds a_out)
				.Read(out Bounds b_out)
				.Read(out Bounds c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _BoundsInt() {
			BoundsInt a_in = new(new(1, 3, 22), new(5, 1, 3));
			BoundsInt b_in = new(new(4, -11, 23), new (3, 3, 1));
			BoundsInt c_in = new(new(-12, -5, 24), new(37, 1, 36));
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc BoundsInt[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out BoundsInt a_out)
				.Read(out BoundsInt b_out)
				.Read(out BoundsInt c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}

		[Test]
		public static void _KeyFrame() {
			Keyframe a_in = new(3f, 4f);
			Keyframe b_in = new(10f, 12f);
			Keyframe c_in = new(0.5f, -0.5f);
			var buffer = new Serializer()
				.Write(a_in)
				.Write(stackalloc Keyframe[2] {b_in, c_in});
			
			var deserializer = new Deserializer(buffer)
				.Read(out Keyframe a_out)
				.Read(out Keyframe b_out)
				.Read(out Keyframe c_out);
			
			Assert.AreEqual(a_out, a_in);
			Assert.AreEqual(b_out, b_in);
			Assert.AreEqual(c_out, c_in);
		}
		
		[Test]
		public static void _AnimationCurve() {
			AnimationCurve a_in = new(new Keyframe[] { new(0f, 0f), new(1f, 2f), new(3f, 0.2f) });
			AnimationCurve b_in = new(new Keyframe[] { new(0f, 1f), new(2f, 2f), new(4f, 0.3f) });
			AnimationCurve c_in = new(new Keyframe[] { new(0f, 0.5f), new(0.1f, 2f), new(0.2f, 0.4f), new(0.5f, 0.4f) });
			var buffer = new Serializer()
				.Write(a_in)
				.Write(new AnimationCurve[] {b_in , c_in});
			
			new Deserializer(buffer)
				.Read(out AnimationCurve a_out)
				.Read(out AnimationCurve b_out)
				.Read(out AnimationCurve c_out);
			
			Assert.AreEqual(a_out.length, a_in.length);
			for (int i = 0; i < a_out.length; i++) {
				Assert.AreEqual(a_out[i], a_in[i]);
			}
			
			Assert.AreEqual(b_out.length, b_in.length);
			for (int i = 0; i < b_out.length; i++) {
				Assert.AreEqual(b_out[i], b_in[i]);
			}
			
			Assert.AreEqual(c_out.length, c_in.length);
			for (int i = 0; i < c_out.length; i++) {
				Assert.AreEqual(c_out[i], c_in[i]);
			}
		}
	}
}