using Verse;

namespace RimWorld;

public class RitualObligationTrigger_MemberCorpseDestroyed : RitualObligationTrigger
{
	public override void Notify_MemberCorpseDestroyed(Pawn p)
	{
		if (Current.ProgramState == ProgramState.Playing && (!mustBePlayerIdeo || Faction.OfPlayer.ideos.Has(ritual.ideo)) && p.HomeFaction == Faction.OfPlayer && p.IsFreeNonSlaveColonist && !p.IsKidnapped() && (!ModsConfig.AnomalyActive || (!p.IsCreepJoiner && !p.IsSubhuman && !p.health.hediffSet.HasHediff(HediffDefOf.ShamblerCorpse))))
		{
			Precept_Ritual precept_Ritual = (Precept_Ritual)(p.ideo?.Ideo?.GetPrecept(PreceptDefOf.Funeral));
			if (precept_Ritual == null || precept_Ritual.activeObligations.NullOrEmpty() || precept_Ritual.completedObligations.NullOrEmpty() || !precept_Ritual.completedObligations.Any((RitualObligation o) => o.FirstValidTarget.Thing == p))
			{
				ritual.AddObligation(new RitualObligation(ritual, p));
			}
		}
	}
}
