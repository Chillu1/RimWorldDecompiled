using Verse;

namespace RimWorld;

public class CompAbilityEffect_EffecterOnTarget : CompAbilityEffect
{
	public new CompProperties_AbilityEffecterOnTarget Props => (CompProperties_AbilityEffecterOnTarget)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Effecter effecter = ((!target.HasThing) ? Props.effecterDef.Spawn(target.Cell, parent.pawn.Map, Props.scale) : Props.effecterDef.Spawn(target.Thing, parent.pawn.Map, Props.scale));
		if (Props.maintainForTicks > 0)
		{
			parent.AddEffecterToMaintain(effecter, target.Cell, Props.maintainForTicks);
		}
		else
		{
			effecter.Cleanup();
		}
	}
}
