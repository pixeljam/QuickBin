namespace QuickBin {
	static class SerializerPool {
		static readonly Stack<Serializer> pool = new();
		public static Serializer Get(int capacityHint=0) {
			if (pool.Count > 0) {
				var s = pool.Pop();
				s.Clear(); // implement Clear to reset ABW (recreate or track indices)
				return s;
			}
			return new(capacityHint);
		}
		public static byte[] ToArrayAndReturn(Serializer s) {
			var arr = s.ToArray();
			pool.Push(s);
			return arr;
		}
	}
}