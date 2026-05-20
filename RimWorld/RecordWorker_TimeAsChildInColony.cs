using Verse;

namespace RimWorld;

public class RecordWorker_TimeAsChildInColony : RecordWorker
{
	public override bool ShouldMeasureTimeNow(Pawn pawn)
	{
		if (ModsConfig.BiotechActive && pawn.Spawned && pawn.RaceProps.Humanlike && pawn.Map.IsPlayerHome)
		{
			return pawn.DevelopmentalStage.Juvenile();
		}
		return false;
	}
}
