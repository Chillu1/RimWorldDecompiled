using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleeTerror : ThinkNode_JobGiver
{
	private FloatRange fleeDistRange = new FloatRange(50f, 75f);

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = pawn.MentalState?.causedByPawn;
		if (pawn2 == null)
		{
			return null;
		}
		return FleeUtility.FleeJob(pawn, pawn2, Mathf.CeilToInt(fleeDistRange.RandomInRange));
	}
}
