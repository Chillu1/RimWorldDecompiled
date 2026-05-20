using Verse;

namespace RimWorld;

public class ThoughtWorker_UnnaturalDarkness : ThoughtWorker_GameCondition
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.AnomalyActive || !base.CurrentStateInternal(p).Active)
		{
			return ThoughtState.Inactive;
		}
		if (p.MapHeld.weatherManager.curWeather != WeatherDefOf.UnnaturalDarkness_Stage2)
		{
			return ThoughtState.Inactive;
		}
		if (p.Inhumanized())
		{
			return ThoughtState.ActiveAtStage(1);
		}
		return ThoughtState.ActiveAtStage(0);
	}
}
