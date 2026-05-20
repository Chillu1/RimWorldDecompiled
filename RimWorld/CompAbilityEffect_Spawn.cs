using Verse;

namespace RimWorld;

public class CompAbilityEffect_Spawn : CompAbilityEffect
{
	public new CompProperties_AbilitySpawn Props => (CompProperties_AbilitySpawn)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		GenSpawn.Spawn(Props.thingDef, target.Cell, parent.pawn.Map);
		if (Props.sendSkipSignal)
		{
			CompAbilityEffect_Teleport.SendSkipUsedSignal(target, parent.pawn);
		}
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.Cell.Filled(parent.pawn.Map) || (!Props.allowOnBuildings && target.Cell.GetEdifice(parent.pawn.Map) != null))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}
}
