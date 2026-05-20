using Verse;

namespace RimWorld
{
	public class InventoryStockEntry : IExposable
	{
		public ThingDef thingDef;

		public int count;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref count, "count", 0);
		}
	}
}
