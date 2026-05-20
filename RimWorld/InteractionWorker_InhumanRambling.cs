using Verse;

namespace RimWorld;

public class InteractionWorker_InhumanRambling : InteractionWorker
{
	private const float SelectionWeight_HumanityBreak = 999f;

	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return 0f;
		}
		if (initiator.MentalStateDef != MentalStateDefOf.HumanityBreak)
		{
			return 0f;
		}
		return 999f;
	}
}
