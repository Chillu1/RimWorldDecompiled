using Verse;

namespace RimWorld
{
	public class CompProperties_Shearable : CompProperties
	{
		public int shearIntervalDays;

		public int woolAmount = 1;

		public ThingDef woolDef;

		public CompProperties_Shearable()
		{
			compClass = typeof(CompShearable);
		}
	}
}
