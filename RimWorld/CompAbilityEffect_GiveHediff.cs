using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_GiveHediff : CompAbilityEffect_WithDuration
	{
		public new CompProperties_AbilityGiveHediff Props => (CompProperties_AbilityGiveHediff)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			ApplyInner(target.Pawn, parent.pawn);
			if (Props.applyToSelf)
			{
				ApplyInner(parent.pawn, target.Pawn);
			}
		}

		protected void ApplyInner(Pawn target, Pawn other)
		{
			if (target == null)
			{
				return;
			}
			if (Props.replaceExisting)
			{
				Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
				if (firstHediffOfDef != null)
				{
					target.health.RemoveHediff(firstHediffOfDef);
				}
			}
			Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, target, Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
			HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null)
			{
				hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(target).SecondsToTicks();
			}
			HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
			if (hediffComp_Link != null)
			{
				hediffComp_Link.other = other;
				hediffComp_Link.drawConnection = (target == parent.pawn);
			}
			target.health.AddHediff(hediff);
		}
	}
}
