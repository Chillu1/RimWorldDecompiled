using Verse;

namespace RimWorld
{
	public class CompProperties_RitualEffectIntervalSpawnCircle : CompProperties_RitualEffectIntervalSpawn
	{
		public IntVec2 area;

		public float radius = 5f;

		public float concentration = 1f;

		public CompProperties_RitualEffectIntervalSpawnCircle()
		{
			compClass = typeof(CompRitualEffect_IntervalSpawnCircle);
		}
	}
}
