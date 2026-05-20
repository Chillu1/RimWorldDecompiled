using Verse;

namespace RimWorld;

public class CompAbilityEffect_GiveHediff : CompAbilityEffect_WithDuration
{
	public new CompProperties_AbilityGiveHediff Props => (CompProperties_AbilityGiveHediff)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		if (!Props.ignoreSelf || target.Pawn != parent.pawn)
		{
			if (!Props.onlyApplyToSelf && Props.applyToTarget)
			{
				ApplyInner(target.Pawn, parent.pawn);
			}
			if (Props.applyToSelf || Props.onlyApplyToSelf)
			{
				ApplyInner(parent.pawn, target.Pawn);
			}
		}
	}

	protected void ApplyInner(Pawn target, Pawn other)
	{
		if (target == null)
		{
			return;
		}
		if (TryResist(target))
		{
			MoteMaker.ThrowText(target.DrawPos, target.Map, "Resisted".Translate());
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
		if (Props.severity >= 0f)
		{
			hediff.Severity = Props.severity;
		}
		HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
		if (hediffComp_Link != null)
		{
			hediffComp_Link.other = other;
			hediffComp_Link.drawConnection = target == parent.pawn;
		}
		target.health.AddHediff(hediff);
	}

	protected virtual bool TryResist(Pawn pawn)
	{
		return false;
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (parent.pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		return target.Pawn != null;
	}
}
