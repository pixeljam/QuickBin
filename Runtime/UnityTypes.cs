using UnityEngine;
using QuickBin.ChainExtensions;
using System.Linq;

namespace QuickBin {
	public static partial class QuickBinExtensions {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
    	public static Serializer Write(this Serializer buffer, Vector2 value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Vector3 value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Vector4 value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Vector2Int value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Vector3Int value) => buffer.WriteUnmanaged(value);

		public static Serializer Write(this Serializer buffer, Quaternion value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Color value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Color32 value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Rect value) => buffer.WriteUnmanaged(value);

		public static Serializer Write(this Serializer buffer, RectInt value) => buffer.WriteUnmanaged(value);
		
		public static Serializer Write(this Serializer buffer, Matrix4x4 value) => buffer.WriteUnmanaged(value);

		public static Serializer Write(this Serializer buffer, Bounds value) => buffer
			.Write(value.center)
			.Write(value.size);
		
		public static Serializer Write(this Serializer buffer, BoundsInt value) => buffer
			.Write(value.center)
			.Write(value.size);
		
		public static Serializer Write(this Serializer buffer, AnimationCurve value) => buffer
			.Write(value.keys.Length)
			.ForEach(value.keys, key => buffer.Write(key));
		
		public static Serializer Write(this Serializer buffer, Keyframe value) => buffer
			.Write(value.time)
			.Write(value.value)
			.Write(value.inTangent)
			.Write(value.outTangent)
			.Write(value.inWeight)
			.Write(value.outWeight);
		
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