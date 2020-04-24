using Verse;

namespace RimWorld
{
	public class CompProperties_EggLayer : CompProperties
	{
		public float eggLayIntervalDays = 1f;

		public IntRange eggCountRange = IntRange.one;

		public ThingDef eggUnfertilizedDef;

		public ThingDef eggFertilizedDef;

		public int eggFertilizationCountMax = 1;

		public bool eggLayFemaleOnly = true;

		public float eggProgressUnfertilizedMax = 1f;

		public CompProperties_EggLayer()
		{
			compClass = typeof(CompEggLayer);
		}
	}
}
