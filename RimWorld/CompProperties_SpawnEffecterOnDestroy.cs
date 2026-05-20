using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnEffecterOnDestroy : CompProperties
	{
		public EffecterDef effect;

		public CompProperties_SpawnEffecterOnDestroy()
		{
			compClass = typeof(CompSpawnEffecterOnDestroy);
		}
	}
}
