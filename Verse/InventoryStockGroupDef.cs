using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class InventoryStockGroupDef : Def
	{
		public List<ThingDef> thingDefs;

		public int min;

		public int max = 3;

		public ThingDef defaultThingDef;

		public ThingDef DefaultThingDef => defaultThingDef ?? thingDefs.First();

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (defaultThingDef != null && !thingDefs.Contains(defaultThingDef))
			{
				yield return "Default thing def " + defaultThingDef.defName + " should be in thingDefs but not found.";
			}
			if (min > max)
			{
				yield return "Min should be less than max.";
			}
			if (min < 0 || max < 0)
			{
				yield return "Min/max should be greater than zero.";
			}
			if (thingDefs.NullOrEmpty())
			{
				yield return "thingDefs cannot be null or empty.";
			}
		}
	}
}
