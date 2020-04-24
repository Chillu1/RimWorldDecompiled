using Verse;

namespace RimWorld
{
	public class WorkGiver_TendOtherUrgent : WorkGiver_TendOther
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (base.HasJobOnThing(pawn, t, forced))
			{
				return HealthAIUtility.ShouldBeTendedNowByPlayerUrgent((Pawn)t);
			}
			return false;
		}
	}
}
