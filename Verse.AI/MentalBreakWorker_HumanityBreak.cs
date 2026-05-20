using RimWorld;

namespace Verse.AI;

public class MentalBreakWorker_HumanityBreak : MentalBreakWorker
{
	public override bool BreakCanOccur(Pawn pawn)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (base.BreakCanOccur(pawn))
		{
			return !pawn.Inhumanized();
		}
		return false;
	}

	public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
	{
		if (base.TryStart(pawn, reason, causedByMood))
		{
			if (!pawn.Inhumanized())
			{
				pawn.health.AddHediff(HediffDefOf.Inhumanized);
			}
			return true;
		}
		return false;
	}
}
