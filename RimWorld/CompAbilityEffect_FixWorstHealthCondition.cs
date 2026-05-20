using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_FixWorstHealthCondition : CompAbilityEffect
	{
		public new CompProperties_AbilityFixWorstHealthCondition Props => (CompProperties_AbilityFixWorstHealthCondition)props;

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (target.Pawn == null)
			{
				return false;
			}
			if (target.Pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.isBad && x.def.everCurableByItem && x.Visible))
			{
				return base.CanApplyOn(target, dest);
			}
			return false;
		}

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			TaggedString taggedString = HealthUtility.FixWorstHealthCondition(target.Pawn);
			if (base.SendLetter)
			{
				Find.LetterStack.ReceiveLetter(Props.customLetterLabel, Props.customLetterText.Formatted(parent.pawn, target.Pawn, taggedString), LetterDefOf.PositiveEvent, new LookTargets(target.Pawn));
			}
		}
	}
}
