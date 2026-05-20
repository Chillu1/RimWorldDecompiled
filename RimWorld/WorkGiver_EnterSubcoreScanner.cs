using Verse;

namespace RimWorld
{
	public class WorkGiver_EnterSubcoreScanner : WorkGiver_EnterBuilding
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.SubcoreScanner);

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !ModsConfig.BiotechActive;
		}
	}
}
