using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class SelfDefenseUtility
{
	public static bool ShouldStartFleeing(Pawn pawn)
	{
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
		for (int i = 0; i < list.Count; i++)
		{
			if (FleeUtility.ShouldFleeFrom(list[i], pawn, checkDistance: true, checkLOS: false))
			{
				return true;
			}
		}
		bool foundThreat = false;
		Region region = pawn.GetRegion();
		if (region == null)
		{
			return false;
		}
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate(Region reg)
		{
			List<Thing> list2 = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			for (int j = 0; j < list2.Count; j++)
			{
				if (FleeUtility.ShouldFleeFrom(list2[j], pawn, checkDistance: true, checkLOS: true))
				{
					foundThreat = true;
					break;
				}
			}
			return foundThreat;
		}, 9);
		return foundThreat;
	}
}
