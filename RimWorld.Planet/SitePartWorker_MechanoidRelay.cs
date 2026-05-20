using Verse;

namespace RimWorld.Planet;

public class SitePartWorker_MechanoidRelay : SitePartWorker
{
	public override void Init(Site site, SitePart sitePart)
	{
		base.Init(site, sitePart);
		sitePart.things = new ThingOwner<Thing>(sitePart);
		sitePart.things.TryAdd(ThingMaker.MakeThing(ThingDefOf.MechRelay));
		for (int i = 0; i < sitePart.parms.stabilizerCount; i++)
		{
			sitePart.things.TryAdd(ThingMaker.MakeThing(ThingDefOf.MechStabilizer));
		}
	}
}
