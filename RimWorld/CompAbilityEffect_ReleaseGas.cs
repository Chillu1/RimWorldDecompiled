using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_ReleaseGas : CompAbilityEffect
{
	private new CompProperties_AbilityReleaseGas Props => (CompProperties_AbilityReleaseGas)props;

	private int TotalGas => Mathf.CeilToInt(Props.cellsToFill * 255);

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		Pawn pawn = target.Pawn;
		if (Props.gasType == GasType.DeadlifeDust)
		{
			GasUtility.AddDeadifeGas(pawn.Position, pawn.Map, pawn.Faction, TotalGas);
		}
		else
		{
			GasUtility.AddGas(pawn.Position, pawn.Map, Props.gasType, TotalGas);
		}
		base.Apply(target, dest);
	}
}
