using Verse;

namespace RimWorld
{
	public class WorkGiver_EnterGrowthVat : WorkGiver_EnterBuilding
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.GrowthVat);

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !ModsConfig.BiotechActive;
		}
	}
}
