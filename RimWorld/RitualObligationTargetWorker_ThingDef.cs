using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_ThingDef : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_ThingDef()
		{
		}

		public RitualObligationTargetWorker_ThingDef(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			if (def.thingDefs.NullOrEmpty())
			{
				yield break;
			}
			for (int i = 0; i < def.thingDefs.Count; i++)
			{
				ThingDef thingDef = def.thingDefs[i];
				List<Thing> things = map.listerThings.ThingsOfDef(thingDef);
				for (int j = 0; j < things.Count; j++)
				{
					if (CanUseTarget(things[j], obligation).canUse)
					{
						yield return things[j];
					}
				}
			}
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			return target.HasThing && def.thingDefs.Contains(target.Thing.def) && (!def.colonistThingsOnly || (target.Thing.Faction != null && target.Thing.Faction.IsPlayer));
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			for (int i = 0; i < def.thingDefs.Count; i++)
			{
				yield return def.thingDefs[i].label;
			}
		}
	}
}
