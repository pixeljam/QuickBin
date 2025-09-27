// Serializer.cs (new core)
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace QuickBin {
	public sealed class Serializer(int capacity = 0) : IEnumerable<byte> {
		private ArrayBufferWriter<byte> _abw = capacity > 0
			? new ArrayBufferWriter<byte>(capacity)
			: new ArrayBufferWriter<byte>();

		public int Length => _abw.WrittenCount;
		public IEnumerable<byte> Bytes => _abw.WrittenSpan.ToArray(); // keep old API behavior

		// Bulk append of spans (no per-byte loop)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Serializer WriteSpan(ReadOnlySpan<byte> src) {
			var dest = _abw.GetSpan(src.Length);
			src.CopyTo(dest);
			_abw.Advance(src.Length);
			_boolPlace = 0;
			return this;
		}

		// Primitive writers without stackalloc
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Serializer Write(int value) {
			var span = _abw.GetSpan(4);
			BinaryPrimitives.WriteInt32LittleEndian(span, value);
			_abw.Advance(4);
			_boolPlace = 0;
			return this;
		}
		// (repeat for uint, long, ulong, short, ushort, float via SingleToInt32Bits, double via DoubleToInt64Bits, etc.)

		// Existing string overloads can switch to Encoder.Convert into GetSpan chunks to avoid temp arrays (optional).

		// Expose an efficient handoff when you truly need a byte[]:
		public byte[] ToArray() => _abw.WrittenSpan.ToArray();
		internal ReadOnlySpan<byte> WrittenSpan => _abw.WrittenSpan;

		// pooled helpers (section 3)
		public static Serializer GetPooled(int capacityHint = 0) => SerializerPool.Get(capacityHint);
		public byte[] ToArrayAndReturn() => SerializerPool.ToArrayAndReturn(this);

		// --- length patch helpers (used by WriteTo in #1) ---
		public int ReserveU16LittleEndian() {
			var span = _abw.GetSpan(2);
			// leave zeros; return absolute position to patch
			int pos = _abw.WrittenCount;
			_abw.Advance(2);
			return pos;
		}
		public void PatchU16LittleEndian(int absolutePos, ushort value) {
			var whole = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_abw.WrittenSpan), _abw.WrittenCount);
			BinaryPrimitives.WriteUInt16LittleEndian(whole.Slice(absolutePos, 2), value);
		}

		private int _boolPlace = 0;
		public Serializer WriteFlag(bool value, bool forceNewByte=false) {
			if (forceNewByte || _boolPlace == 0) {
				// write new flag byte
				var s = _abw.GetSpan(1);
				s[0] = value ? (byte)1 : (byte)0;
				_abw.Advance(1);
				_boolPlace = 1;
			} else {
				var span = _abw.WrittenSpan; // last written byte
				Span<byte> last = span.Slice(span.Length-1,1);
				if (value) last[0] |= (byte)(1 << _boolPlace);
				_boolPlace = (_boolPlace + 1) & 7;
			}
			return this;
		}
	}
}