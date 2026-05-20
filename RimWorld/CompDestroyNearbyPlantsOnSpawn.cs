using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompDestroyNearbyPlantsOnSpawn : ThingComp
{
	public CompProperties_DestroyNearbyPlantsOnSpawn Props => (CompProperties_DestroyNearbyPlantsOnSpawn)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (respawningAfterLoad)
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, Props.radius, useCenter: true))
		{
			if (!item.InBounds(parent.Map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(parent.Map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				Thing thing = thingList[num];
				if (thing is Plant && !thing.def.preventSkyfallersLandingOn)
				{
					thing.Kill();
				}
			}
		}
	}
}
