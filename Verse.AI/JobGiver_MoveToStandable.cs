using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobGiver_MoveToStandable : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.Drafted)
		{
			return null;
		}
		if (pawn.pather.Moving)
		{
			return null;
		}
		if (!pawn.Position.Standable(pawn.Map))
		{
			return FindBetterPositionJob(pawn);
		}
		List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Pawn pawn2 && pawn2 != pawn && pawn2.Faction == pawn.Faction && pawn2.Drafted && !pawn2.pather.MovingNow)
			{
				return FindBetterPositionJob(pawn);
			}
		}
		return null;
	}

	private static Job FindBetterPositionJob(Pawn pawn)
	{
		IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(pawn.Position, pawn);
		if (intVec.IsValid && intVec != pawn.Position)
		{
			return JobMaker.MakeJob(JobDefOf.Goto, intVec);
		}
		return null;
	}
}
