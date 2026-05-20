using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_WorshippedTerminal : SitePartWorker
	{
		public override void Init(Site site, SitePart sitePart)
		{
			base.Init(site, sitePart);
			sitePart.things = new ThingOwner<Thing>(sitePart);
			Thing item = ThingMaker.MakeThing(ThingDefOf.AncientTerminal_Worshipful);
			sitePart.things.TryAdd(item);
		}

		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			return base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets).Formatted(map.Parent.GetComponent<TimedMakeFactionHostile>().TicksLeft.Value.ToStringTicksToPeriod().Named("TIMER"));
		}
	}
}
