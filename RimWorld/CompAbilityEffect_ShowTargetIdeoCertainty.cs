using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_ShowTargetIdeoCertainty : CompAbilityEffect
	{
		public override bool HideTargetPawnTooltip => true;

		public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
		{
			if (target.Pawn != null)
			{
				Pawn pawn = target.Pawn;
				if (pawn.Ideo != null)
				{
					return "IdeoCertaintyTooltip".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), pawn.ideo.Certainty.ToStringPercent().Named("PERCENTAGE"));
				}
			}
			return null;
		}
	}
}
