using Verse;

namespace RimWorld;

public class HorrorMonolithAdvancedTransition : MusicTransition
{
	private const int TransitionTicksDuration = 3600;

	public override bool IsTransitionSatisfied()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		if (Find.Anomaly.Level > 0)
		{
			return Find.Anomaly.TicksSinceLastLevelChange <= 3600;
		}
		return false;
	}
}
