using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PreceptWorker
	{
		public PreceptDef def;

		public virtual IEnumerable<PreceptThingChance> ThingDefs => ((IEnumerable<PreceptThingChanceClass>)def.buildingDefChances).Select((Func<PreceptThingChanceClass, PreceptThingChance>)((PreceptThingChanceClass dc) => dc));

		public virtual bool ShouldSkipThing(Ideo ideo, ThingDef thingDef)
		{
			return false;
		}

		public virtual float GetThingOrder(PreceptThingChance thingChance)
		{
			return 0f;
		}

		public virtual AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
		{
			return true;
		}

		[Obsolete("Will be removed in a future update")]
		public virtual AcceptanceReport CanUse(ThingDef def, Ideo ideo)
		{
			return CanUse(def, ideo, null);
		}

		public virtual IEnumerable<PreceptThingChance> ThingDefsForIdeo(Ideo ideo, FactionDef generatingFor = null)
		{
			foreach (PreceptThingChance thingDef in ThingDefs)
			{
				if ((bool)CanUse(thingDef.def, ideo, generatingFor))
				{
					yield return thingDef;
				}
			}
		}

		[Obsolete("Will be removed in a future update")]
		public virtual IEnumerable<PreceptThingChance> ThingDefsForIdeo(Ideo ideo)
		{
			return ThingDefsForIdeo(ideo, null);
		}
	}
}
