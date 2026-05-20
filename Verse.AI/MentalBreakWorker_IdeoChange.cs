namespace Verse.AI;

public class MentalBreakWorker_IdeoChange : MentalBreakWorker
{
	public override bool BreakCanOccur(Pawn pawn)
	{
		if (!ModsConfig.IdeologyActive || Find.IdeoManager.classicMode)
		{
			return false;
		}
		return base.BreakCanOccur(pawn);
	}
}
