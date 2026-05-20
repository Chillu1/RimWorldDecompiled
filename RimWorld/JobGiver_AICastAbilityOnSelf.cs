using Verse;

namespace RimWorld;

public class JobGiver_AICastAbilityOnSelf : JobGiver_AICastAbility
{
	protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
	{
		LocalTargetInfo localTargetInfo = new LocalTargetInfo(caster);
		if (!ability.def.targetRequired || ability.CanApplyOn(localTargetInfo))
		{
			return localTargetInfo;
		}
		return LocalTargetInfo.Invalid;
	}
}
