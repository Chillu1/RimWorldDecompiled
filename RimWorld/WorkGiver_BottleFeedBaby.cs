using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_BottleFeedBaby : WorkGiver_FeedBabyManually
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!CanCreateManualFeedingJob(pawn, t, forced))
		{
			return null;
		}
		Pawn baby = (Pawn)t;
		Thing thing = ChildcareUtility.FindBabyFoodForBaby(pawn, baby);
		if (thing != null)
		{
			return ChildcareUtility.MakeBottlefeedJob(baby, thing);
		}
		JobFailReason.Is("NoBabyFood".Translate());
		return null;
	}
}
