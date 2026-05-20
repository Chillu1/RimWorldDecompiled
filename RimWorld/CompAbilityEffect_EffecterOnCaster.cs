using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_EffecterOnCaster : CompAbilityEffect
{
	public new CompProperties_AbilityEffecterOnCaster Props => (CompProperties_AbilityEffecterOnCaster)props;

	public override void PostApplied(List<LocalTargetInfo> targets, Map map)
	{
		base.PostApplied(targets, map);
		Effecter effecter = Props.effecterDef.Spawn(parent.pawn, map, Props.scale);
		if (Props.maintainTicks > 0)
		{
			map.effecterMaintainer.AddEffecterToMaintain(effecter, new TargetInfo(parent.pawn), parent.pawn, Props.maintainTicks);
		}
		else
		{
			effecter.Cleanup();
		}
	}
}
