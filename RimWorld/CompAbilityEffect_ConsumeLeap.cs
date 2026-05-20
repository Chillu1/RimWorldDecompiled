using Verse;

namespace RimWorld;

public class CompAbilityEffect_ConsumeLeap : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
{
	private new CompProperties_ConsumeLeap Props => (CompProperties_ConsumeLeap)props;

	public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
	{
		if (parent.pawn.TryGetComp<CompDevourer>(out var comp))
		{
			comp.StartDigesting(origin, target);
		}
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		Pawn pawn = target.Pawn;
		if (pawn == null)
		{
			return false;
		}
		return pawn.BodySize <= Props.maxBodySize;
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.Pawn.BodySize > Props.maxBodySize)
		{
			return false;
		}
		return base.Valid(target, throwMessages);
	}
}
