using Verse;

namespace RimWorld
{
	public class CompProperties_Milkable : CompProperties
	{
		public int milkIntervalDays;

		public int milkAmount = 1;

		public ThingDef milkDef;

		public bool milkFemaleOnly = true;

		public CompProperties_Milkable()
		{
			compClass = typeof(CompMilkable);
		}
	}
}
