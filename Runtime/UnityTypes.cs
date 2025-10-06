using UnityEngine;
using QuickBin.ChainExtensions;
using System.Linq;
using System.Runtime.CompilerServices;

namespace QuickBin {
	public static partial class QuickBinExtensions {
		#region Vector2
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector2 value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector2> values) => buffer.WriteUnmanagedArray(values);
		#endregion Vector2

		#region Vector2Int
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector2Int value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector2Int> values) => buffer.WriteUnmanagedArray(values);
		#endregion Vector2Int
		
		#region Vector3
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector3 value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector3> values) => buffer.WriteUnmanagedArray(values);
		#endregion Vector3

		#region Vector3Int
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector3Int value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector3Int> values) => buffer.WriteUnmanagedArray(values);
		#endregion Vector3Int

		#region Vector4
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Vector4 value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Vector4> values) => buffer.WriteUnmanagedArray(values);
		#endregion Vector4
		
		#region Quaternion
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Quaternion value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Quaternion> values) => buffer.WriteUnmanagedArray(values);
		#endregion Quaternion

		#region Color
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Color value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Color> values) => buffer.WriteUnmanagedArray(values);
		#endregion Color

		#region Color32
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Color32 value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Color32> values) => buffer.WriteUnmanagedArray(values);
		#endregion Color32

		#region Rect
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Rect value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Rect> values) => buffer.WriteUnmanagedArray(values);
		#endregion Rect

		#region RectInt
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, RectInt value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<RectInt> values) => buffer.WriteUnmanagedArray(values);
		#endregion RectInt

		#region Matrix4x4
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Matrix4x4 value) => buffer.WriteUnmanaged(ref value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Matrix4x4> values) => buffer.WriteUnmanagedArray(values);
		#endregion Matrix4x4

		#region Bounds
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Bounds value) => buffer.WriteUnmanagedPair(value.center, value.size);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Bounds> values) {
				if (values.Length == 0) return buffer;

				int v3Bytes = Unsafe.SizeOf<Vector3>();
				int pairBytes = v3Bytes * 2;

				ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * pairBytes));

				for (int i = 0, offset = 0; i < values.Length; i++, offset += pairBytes) {
					var v = values[i];

					var s0 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), v3Bytes);
					MemoryMarshal.Write(s0, ref v.center);

					var s1 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + v3Bytes), v3Bytes);
					MemoryMarshal.Write(s1, ref v.size);
				}

				return buffer;
			}
		#endregion
		
		#region BoundsInt
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, BoundsInt value) => buffer.WriteUnmanagedPair(value.center, value.size);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<BoundsInt> values) {
				if (values.Length == 0) return buffer;

				int v3Bytes = Unsafe.SizeOf<BoundsInt>();
				int pairBytes = v3Bytes * 2;

				ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * pairBytes));

				for (int i = 0, offset = 0; i < values.Length; i++, offset += pairBytes) {
					var v = values[i];

					var s0 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset), v3Bytes);
					MemoryMarshal.Write(s0, ref v.center);

					var s1 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + v3Bytes), v3Bytes);
					MemoryMarshal.Write(s1, ref v.size);
				}

				return buffer;
			}
		#endregion BoundsInt
		
		#region AnimationCurve
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, AnimationCurve value) => buffer
				.Write(value.keys.Length)
				.Write((ReadOnlySpan<KeyFrame>)value.keys);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<AnimationCurve> curves) {
				if (curves.Length == 0) return buffer;

				// Its extremely unlikely anyone will ever need to bulk write Animation Curves, this will do 1 allocate span per curve, so it's not as fast as it could be.
				for (int i = 0; i < curves.Length; i++)
					buffer.Write(curves[i]); // writes count + payload via the bulk keyframe writer

				return buffer;
			}
		#endregion AnimationCurve
		
		#region KeyFrame
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, Keyframe value) => 
				buffer.Write(stackalloc float[]{ value.time, value.value, value.inTangent, value.outTangent, value.inWeight, value.outWeight });

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Serializer Write(this Serializer buffer, ReadOnlySpan<Keyframe> values) {
				if (values.Length == 0) return buffer;

				const int sz = sizeof(int); // writing floats via Int32 bits (LE)
				int stride = 6 * sz; // 6 floats

				ref byte baseRef = ref MemoryMarshal.GetReference(buffer.AllocateSpan(values.Length * stride));

				for (int i = 0, offset = 0; i < values.Length; i++, offset += stride) {
					var k = values[i];

					var s0 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 0 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s0, BitConverter.SingleToInt32Bits(k.time));

					var s1 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 1 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s1, BitConverter.SingleToInt32Bits(k.value));

					var s2 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 2 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s2, BitConverter.SingleToInt32Bits(k.inTangent));

					var s3 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 3 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s3, BitConverter.SingleToInt32Bits(k.outTangent));

					var s4 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 4 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s4, BitConverter.SingleToInt32Bits(k.inWeight));

					var s5 = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref baseRef, offset + 5 * sz), sz);
					BinaryPrimitives.WriteInt32LittleEndian(s5, BitConverter.SingleToInt32Bits(k.outWeight));
				}
				
				return buffer;
			}
		#endregion KeyFrame
		
		public static Deserializer Read(this Deserializer buffer, out Vector2 produced) => buffer
			.Read(out float x)
			.Read(out float y)
			.Assign(new(x, y), out produced);

		public static Deserializer Read(this Deserializer buffer, out Vector3 produced) => buffer
			.Read(out float x)
			.Read(out float y)
			.Read(out float z)
			.Assign(new(x,y,z), out produced);

		public static Deserializer Read(this Deserializer buffer, out Vector4 produced) => buffer
			.Read(out float x)
			.Read(out float y)
			.Read(out float z)
			.Read(out float w)
			.Assign(new(x, y, z, w), out produced);

		public static Deserializer Read(this Deserializer buffer, out Vector2Int produced) => buffer
			.Read(out int x)
			.Read(out int y)
			.Assign(new(x, y), out produced);

		public static Deserializer Read(this Deserializer buffer, out Vector3Int produced) => buffer
			.Read(out int x)
			.Read(out int y)
			.Read(out int z)
			.Assign(new(x, y), out produced);

		public static Deserializer Read(this Deserializer buffer, out Quaternion produced) => buffer
			.Read(out float x)
			.Read(out float y)
			.Read(out float z)
			.Read(out float w)
			.Assign(new(x, y, z, w), out produced);

		public static Deserializer Read(this Deserializer buffer, out Color produced) => buffer
			.Read(out float r)
			.Read(out float g)
			.Read(out float b)
			.Read(out float a)
			.Assign(new(r, g, b, a), out produced);

		public static Deserializer Read(this Deserializer buffer, out Color32 produced) => buffer
			.Read(out byte r)
			.Read(out byte g)
			.Read(out byte b)
			.Read(out byte a)
			.Assign(new(r, g, b, a), out produced);

		public static Deserializer Read(this Deserializer buffer, out Matrix4x4 produced) => buffer
			.Read(out Vector4 c1)
			.Read(out Vector4 c2)
			.Read(out Vector4 c3)
			.Read(out Vector4 c4)
			.Assign(new(c1, c2, c3, c4), out produced);

		public static Deserializer Read(this Deserializer buffer, out Rect produced) => buffer
			.Read(out float x)
			.Read(out float y)
			.Read(out float width)
			.Read(out float height)
			.Assign(new(x, y, width, height), out produced);

		public static Deserializer Read(this Deserializer buffer, out RectInt produced) => buffer
			.Read(out int x)
			.Read(out int y)
			.Read(out int width)
			.Read(out int height)
			.Assign(new(x, y, width, height), out produced);

		public static Deserializer Read(this Deserializer buffer, out Bounds produced) => buffer
			.Read(out Vector3 center)
			.Read(out Vector3 size)
			.Assign(new(center, size), out produced);

		public static Deserializer Read(this Deserializer buffer, out BoundsInt produced) => buffer
			.Read(out Vector3Int center)
			.Read(out Vector3Int size)
			.Assign(new(center, size), out produced);
		
		public static Deserializer Read(this Deserializer buffer, out AnimationCurve produced) => buffer
			.Read(out int length)
			.ForEach(out var keyframes, buffer => buffer.Read(out Keyframe key).Output(key), length)
			.Assign(buffer.Overflowed ? default : new(keyframes.ToArray()), out produced);

		public static Deserializer Read(this Deserializer buffer, out Keyframe produced) => buffer
			.Read(out float time)
			.Read(out float value)
			.Read(out float inTangent)
			.Read(out float outTangent)
			.Read(out float inWeight)
			.Read(out float outWeight)
			.Assign(buffer.Overflowed ? default : new(time, value, inTangent, outTangent, inWeight, outWeight), out produced);
	}
}