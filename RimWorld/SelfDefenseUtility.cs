using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SelfDefenseUtility
	{
		public const float FleeWhenDistToHostileLessThan = 8f;

		public static bool ShouldStartFleeing(Pawn pawn)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
			for (int i = 0; i < list.Count; i++)
			{
				if (ShouldFleeFrom(list[i], pawn, checkDistance: true, checkLOS: false))
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
					if (ShouldFleeFrom(list2[j], pawn, checkDistance: true, checkLOS: true))
					{
						foundThreat = true;
						break;
					}
				}
				return foundThreat;
			}, 9);
			return foundThreat;
		}

		public static bool ShouldFleeFrom(Thing t, Pawn pawn, bool checkDistance, bool checkLOS)
		{
			if (t == pawn || (checkDistance && !t.Position.InHorDistOf(pawn.Position, 8f)))
			{
				return false;
			}
			if (t.def.alwaysFlee)
			{
				return true;
			}
			if (!t.HostileTo(pawn))
			{
				return false;
			}
			IAttackTarget attackTarget = t as IAttackTarget;
			if (attackTarget == null || attackTarget.ThreatDisabled(pawn) || !(t is IAttackTargetSearcher) || (checkLOS && !GenSight.LineOfSight(pawn.Position, t.Position, pawn.Map)))
			{
				return false;
			}
			return true;
		}
	}
}
