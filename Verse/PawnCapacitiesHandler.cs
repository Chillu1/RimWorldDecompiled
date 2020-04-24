using RimWorld;

namespace Verse
{
	public class PawnCapacitiesHandler
	{
		private enum CacheStatus
		{
			Uncached,
			Caching,
			Cached
		}

		private class CacheElement
		{
			public CacheStatus status;

			public float value;
		}

		private Pawn pawn;

		private DefMap<PawnCapacityDef, CacheElement> cachedCapacityLevels;

		public bool CanBeAwake => GetLevel(PawnCapacityDefOf.Consciousness) >= 0.3f;

		public PawnCapacitiesHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Clear()
		{
			cachedCapacityLevels = null;
		}

		public float GetLevel(PawnCapacityDef capacity)
		{
			if (pawn.health.Dead)
			{
				return 0f;
			}
			if (cachedCapacityLevels == null)
			{
				Notify_CapacityLevelsDirty();
			}
			CacheElement cacheElement = cachedCapacityLevels[capacity];
			if (cacheElement.status == CacheStatus.Caching)
			{
				Log.Error($"Detected infinite stat recursion when evaluating {capacity}");
				return 0f;
			}
			if (cacheElement.status == CacheStatus.Uncached)
			{
				cacheElement.status = CacheStatus.Caching;
				try
				{
					cacheElement.value = PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, capacity);
				}
				finally
				{
					cacheElement.status = CacheStatus.Cached;
				}
			}
			return cacheElement.value;
		}

		public bool CapableOf(PawnCapacityDef capacity)
		{
			return GetLevel(capacity) > capacity.minForCapable;
		}

		public void Notify_CapacityLevelsDirty()
		{
			if (cachedCapacityLevels == null)
			{
				cachedCapacityLevels = new DefMap<PawnCapacityDef, CacheElement>();
			}
			for (int i = 0; i < cachedCapacityLevels.Count; i++)
			{
				cachedCapacityLevels[i].status = CacheStatus.Uncached;
			}
		}
	}
}
