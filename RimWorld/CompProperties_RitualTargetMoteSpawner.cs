using Verse;

namespace RimWorld
{
	public class CompProperties_RitualTargetMoteSpawner : CompProperties
	{
		public ThingDef mote;

		public CompProperties_RitualTargetMoteSpawner()
		{
			compClass = typeof(CompRitualTargetMoteSpawner);
		}
	}
}
