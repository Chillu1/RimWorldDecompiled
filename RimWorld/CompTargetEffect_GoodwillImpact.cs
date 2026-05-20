using Verse;

namespace RimWorld;

public class CompTargetEffect_GoodwillImpact : CompTargetEffect
{
	protected CompProperties_TargetEffect_GoodwillImpact PropsGoodwillImpact => (CompProperties_TargetEffect_GoodwillImpact)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		Pawn pawn = target as Pawn;
		Faction faction = ((pawn != null) ? pawn.HomeFaction : target.Faction);
		if (user.Faction == Faction.OfPlayer && faction != null && !faction.HostileTo(user.Faction) && (pawn == null || !pawn.IsSlaveOfColony))
		{
			Faction.OfPlayer.TryAffectGoodwillWith(faction, PropsGoodwillImpact.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulItem);
		}
	}
}
