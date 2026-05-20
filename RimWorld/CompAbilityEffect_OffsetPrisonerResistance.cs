using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_OffsetPrisonerResistance : CompAbilityEffect
{
	public new CompProperties_AbilityOffsetPrisonerResistance Props => (CompProperties_AbilityOffsetPrisonerResistance)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			pawn.guest.resistance = Mathf.Max(pawn.guest.resistance + Props.offset, 0f);
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!pawn.IsPrisonerOfColony)
			{
				return false;
			}
			if (pawn != null && pawn.guest.resistance < float.Epsilon)
			{
				return false;
			}
			if (pawn.Downed)
			{
				return false;
			}
		}
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null && !AbilityUtility.ValidateHasResistance(pawn, throwMessages, parent))
		{
			return false;
		}
		return true;
	}
}
