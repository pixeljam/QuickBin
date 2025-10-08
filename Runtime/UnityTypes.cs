using System;
using System.Linq;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine;
using QuickBin.ChainExtensions;

namespace QuickBin {
	public static partial class QuickBinExtensions {
		#region Vector2
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector2 value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector2> values) => buffer.WriteUnmanagedArray(values);
		
			public static Deserializer Read(this Deserializer buffer, out Vector2 produced) => buffer
				.Read(out float x)
				.Read(out float y)
				.Assign(new(x, y), out produced);
		#endregion Vector2

		#region Vector2Int
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector2Int value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector2Int> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Vector2Int produced) => buffer
				.Read(out int x)
				.Read(out int y)
				.Assign(new(x, y), out produced);
		#endregion Vector2Int
		
		#region Vector3
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector3 value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector3> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Vector3 produced) => buffer
				.Read(out float x)
				.Read(out float y)
				.Read(out float z)
				.Assign(new(x,y,z), out produced);
		#endregion Vector3

		#region Vector3Int
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector3Int value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector3Int> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Vector3Int produced) => buffer
				.Read(out int x)
				.Read(out int y)
				.Read(out int z)
				.Assign(new(x, y), out produced);
		#endregion Vector3Int

		#region Vector4
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector4 value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector4> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Vector4 produced) => buffer
				.Read(out float x)
				.Read(out float y)
				.Read(out float z)
				.Read(out float w)
				.Assign(new(x, y, z, w), out produced);
		#endregion Vector4
		
		#region Quaternion
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Quaternion value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Quaternion> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Quaternion produced) => buffer
				.Read(out float x)
				.Read(out float y)
				.Read(out float z)
				.Read(out float w)
				.Assign(new(x, y, z, w), out produced);
		#endregion Quaternion

		#region Color
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Color value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Color> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Color produced) => buffer
				.Read(out float r)
				.Read(out float g)
				.Read(out float b)
				.Read(out float a)
				.Assign(new(r, g, b, a), out produced);
		#endregion Color

		#region Color32
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Color32 value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Color32> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Color32 produced) => buffer
				.Read(out byte r)
				.Read(out byte g)
				.Read(out byte b)
				.Read(out byte a)
				.Assign(new(r, g, b, a), out produced);
		#endregion Color32

		#region Rect
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Rect value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Rect> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Rect produced) => buffer
				.Read(out float x)
				.Read(out float y)
				.Read(out float width)
				.Read(out float height)
				.Assign(new(x, y, width, height), out produced);
		#endregion Rect

		#region RectInt
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, RectInt value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<RectInt> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out RectInt produced) => buffer
				.Read(out int x)
				.Read(out int y)
				.Read(out int width)
				.Read(out int height)
				.Assign(new(x, y, width, height), out produced);
		#endregion RectInt

		#region Matrix4x4
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Matrix4x4 value) => buffer.WriteUnmanaged(value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Matrix4x4> values) => buffer.WriteUnmanagedArray(values);

			public static Deserializer Read(this Deserializer buffer, out Matrix4x4 produced) => buffer
				.Read(out Vector4 c1)
				.Read(out Vector4 c2)
				.Read(out Vector4 c3)
				.Read(out Vector4 c4)
				.Assign(new(c1, c2, c3, c4), out produced);
		#endregion Matrix4x4

		#region Bounds
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Bounds value) => buffer.WriteUnmanagedPair(value.center, value.size);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Bounds> values) {
				if (values.Length == 0) return buffer;

				int v3Bytes = Unsafe.SizeOf<Vector3>();

				ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * v3Bytes * 2));

				for (int i = 0, offset = 0; i < values.Length; i++) {
					var v = values[i];
					var center = v.center;
					var size = v.size;
					
					Serializer.ByteWrite(ref baseRef, ref offset, ref center);
					Serializer.ByteWrite(ref baseRef, ref offset, ref size);
				}

				return buffer;
			}

			public static Deserializer Read(this Deserializer buffer, out Bounds produced) => buffer
				.Read(out Vector3 center)
				.Read(out Vector3 size)
				.Assign(new(center, size), out produced);
		#endregion
		
		#region BoundsInt
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, BoundsInt value) => buffer.WriteUnmanagedPair(value.center, value.size);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<BoundsInt> values) {
				if (values.Length == 0) return buffer;

				int v3Bytes = Unsafe.SizeOf<BoundsInt>();
				ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * v3Bytes * 2));

				for (int i = 0, offset = 0; i < values.Length; i++) {
					var v = values[i];
					var center = v.center;
					var size = v.size;

					Serializer.ByteWrite(ref baseRef, ref offset, ref center);
					Serializer.ByteWrite(ref baseRef, ref offset, ref size);
				}

				return buffer;
			}

			public static Deserializer Read(this Deserializer buffer, out BoundsInt produced) => buffer
				.Read(out Vector3Int center)
				.Read(out Vector3Int size)
				.Assign(new(center, size), out produced);
		#endregion BoundsInt
		
		#region AnimationCurve
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, AnimationCurve value) => buffer
				.Write(value.keys.Length)
				.Write(value.keys);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<AnimationCurve> curves) {
				if (curves.Length == 0) return buffer;

				// Its extremely unlikely anyone will ever need to bulk write Animation Curves, this will do 2 allocate span per curve, so it's not as fast as it could be.
				for (int i = 0; i < curves.Length; i++)
					buffer.Write(curves[i]); // writes count + payload via the bulk keyframe writer

				return buffer;
			}
		
			public static Deserializer Read(this Deserializer buffer, out AnimationCurve produced) => buffer
				.Read(out int length)
				.ForEach(out var keyframes, buffer => buffer.Read(out Keyframe key).Output(key), length)
				.Assign(buffer.Overflowed ? default : new(keyframes.ToArray()), out produced);
		#endregion AnimationCurve
		
		#region KeyFrame
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Keyframe value) => 
				buffer.Write(stackalloc float[]{ value.time, value.value, value.inTangent, value.outTangent, value.inWeight, value.outWeight });
			
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Keyframe> values) {
				if (values.Length == 0) return buffer;

				var dest = MemoryMarshal.Cast<byte, float>(buffer.AllocateSpan(values.Length * 6 * sizeof(float)));

				for (int i = 0, j = 0; i < values.Length; i++) {
					var k = values[i];
					dest[j++] = k.time;
					dest[j++] = k.value;
					dest[j++] = k.inTangent;
					dest[j++] = k.outTangent;
					dest[j++] = k.inWeight;
					dest[j++] = k.outWeight;
				}
				
				return buffer;
			}

			public static Deserializer Read(this Deserializer buffer, out Keyframe produced) => buffer
				.Read(out float time)
				.Read(out float value)
				.Read(out float inTangent)
				.Read(out float outTangent)
				.Read(out float inWeight)
				.Read(out float outWeight)
				.Assign(buffer.Overflowed ? default : new(time, value, inTangent, outTangent, inWeight, outWeight), out produced);
		#endregion KeyFrame
	}
}