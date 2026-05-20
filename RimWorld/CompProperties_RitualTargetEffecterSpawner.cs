using Verse;

namespace RimWorld
{
	public class CompProperties_RitualTargetEffecterSpawner : CompProperties
	{
		public EffecterDef effecter;

		public float minRitualProgress;

		public CompProperties_RitualTargetEffecterSpawner()
		{
			compClass = typeof(CompRitualTargetEffecterSpawner);
		}
	}
}
