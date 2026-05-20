using System;
using LudeonTK;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Verse.Glow
{
	public struct GlowUniqueState : IDisposable
	{
		[NativeDisableParallelForRestriction]
		public UnsafeHeap<IntVec3, GlowCellComparer> queue;

		[NativeDisableParallelForRestriction]
		public UnsafeList<GlowCell> area;

		[NativeDisableParallelForRestriction]
		public UnsafeBitArray blockers;

		public static GlowUniqueState AllocateNew()
		{
			GlowUniqueState result = new GlowUniqueState
			{
				queue = new UnsafeHeap<IntVec3, GlowCellComparer>(Allocator.Persistent, 324),
				area = new UnsafeList<GlowCell>(6561, Allocator.Persistent),
				blockers = new UnsafeBitArray(8, Allocator.Persistent)
			};
			result.area.Resize(6561, NativeArrayOptions.ClearMemory);
			return result;
		}

		public void PrepareComparer(ref GlowLight light)
		{
			queue.Comparator = new GlowCellComparer(area, in light);
		}

		public void Clear()
		{
			NativeArrayUtility.MemClear(area);
			queue.Clear();
			blockers.Clear();
		}

		public void Dispose()
		{
			NativeArrayUtility.EnsureDisposed(ref area);
			NativeArrayUtility.EnsureDisposed(ref queue);
			blockers.EnsureDisposed();
		}
	}
}
