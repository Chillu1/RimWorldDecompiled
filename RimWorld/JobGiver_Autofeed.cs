using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Autofeed : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.CanReserve(pawn))
		{
			return null;
		}
		Thing food;
		Pawn pawn2 = ChildcareUtility.FindAutofeedBaby(pawn, AutofeedMode.Urgent, out food);
		if (pawn2 == null || !pawn2.Spawned)
		{
			return null;
		}
		if (food != pawn && ChildcareUtility.ImmobileBreastfeederAvailable(pawn, pawn2, forced: false, out var feeder, out var _))
		{
			food = feeder;
		}
		if (food == null)
		{
			return null;
		}
		return ChildcareUtility.MakeAutofeedBabyJob(pawn, pawn2, food);
	}

	public override float GetPriority(Pawn pawn)
	{
		return 9.5f;
	}
}
