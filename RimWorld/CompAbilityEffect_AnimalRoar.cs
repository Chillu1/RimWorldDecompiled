using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_AnimalRoar : CompAbilityEffect
{
	public new CompProperties_AbilityAnimalRoar Props => (CompProperties_AbilityAnimalRoar)props;

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.Pawn == null)
		{
			return false;
		}
		return Rand.Chance(Props.chanceFromHearingCurve.Evaluate(target.Pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing)));
	}

	public override void PostApplied(List<LocalTargetInfo> targets, Map map)
	{
		parent.pawn.caller?.DoCall(forceAggressive: true);
	}
}
