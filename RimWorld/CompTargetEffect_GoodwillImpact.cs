using Verse;

namespace RimWorld
{
	public class CompTargetEffect_GoodwillImpact : CompTargetEffect
	{
		protected CompProperties_TargetEffect_GoodwillImpact PropsGoodwillImpact => (CompProperties_TargetEffect_GoodwillImpact)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = target as Pawn;
			Faction faction = ((pawn != null) ? pawn.FactionOrExtraMiniOrHomeFaction : target.Faction);
			if (user.Faction != null && faction != null && !faction.HostileTo(user.Faction))
			{
				faction.TryAffectGoodwillWith(user.Faction, PropsGoodwillImpact.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, "GoodwillChangedReason_UsedItem".Translate(parent.LabelShort, target.LabelShort, parent.Named("ITEM"), target.Named("TARGET")), target);
			}
		}
	}
}
