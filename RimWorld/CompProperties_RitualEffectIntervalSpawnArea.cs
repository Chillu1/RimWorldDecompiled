using Verse;

namespace RimWorld
{
	public class CompProperties_RitualEffectIntervalSpawnArea : CompProperties_RitualEffectIntervalSpawn
	{
		public IntVec2 area;

		public bool smoothEdges = true;

		public CompProperties_RitualEffectIntervalSpawnArea()
		{
			compClass = typeof(CompRitualEffect_IntervalSpawnArea);
		}
	}
}
