using Verse;

namespace RimWorld;

public class CorpseAttackTransition : MusicTransition
{
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
		return Find.Anomaly.HasActiveAwokenCorpse();
	}
}
