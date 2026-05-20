using Verse;

namespace RimWorld
{
	public class CompProperties_Plantable : CompProperties
	{
		public ThingDef plantDefToSpawn;

		public CompProperties_Plantable()
		{
			compClass = typeof(CompPlantable);
		}
	}
}
