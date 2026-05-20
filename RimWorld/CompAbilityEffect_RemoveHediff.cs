using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_RemoveHediff : CompAbilityEffect
	{
		public new CompProperties_AbilityRemoveHediff Props => (CompProperties_AbilityRemoveHediff)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			if (Props.applyToSelf)
			{
				RemoveHediff(parent.pawn);
			}
			if (target.Pawn != null && Props.applyToTarget && target.Pawn != parent.pawn)
			{
				RemoveHediff(target.Pawn);
			}
		}

		private void RemoveHediff(Pawn pawn)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
		}
	}
}
