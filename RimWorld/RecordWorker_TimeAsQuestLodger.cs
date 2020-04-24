using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeAsQuestLodger : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				return pawn.HasExtraHomeFaction();
			}
			return false;
		}
	}
}
