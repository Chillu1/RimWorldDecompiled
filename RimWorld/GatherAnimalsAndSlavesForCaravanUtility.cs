using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatherAnimalsAndSlavesForCaravanUtility
	{
		public static bool IsFollowingAnyone(Pawn p)
		{
			return p.mindState.duty.focus.HasThing;
		}

		public static void SetFollower(Pawn p, Pawn follower)
		{
			p.mindState.duty.focus = follower;
			p.mindState.duty.radius = 10f;
		}

		public static void CheckArrived(Lord lord, List<Pawn> pawns, IntVec3 meetingPoint, string memo, Predicate<Pawn> shouldCheckIfArrived, Predicate<Pawn> extraValidator = null)
		{
			bool flag = true;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (shouldCheckIfArrived(pawn) && (!pawn.Spawned || !pawn.Position.InHorDistOf(meetingPoint, 10f) || !pawn.CanReach(meetingPoint, PathEndMode.ClosestTouch, Danger.Deadly) || (extraValidator != null && !extraValidator(pawn))))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				lord.ReceiveMemo(memo);
			}
		}
	}
}
