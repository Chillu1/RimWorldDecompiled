using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobGiver_AICastAbility : ThinkNode_JobGiver
{
	protected AbilityDef ability;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.CurJob?.ability?.def == this.ability)
		{
			return null;
		}
		Ability ability = pawn.abilities?.GetAbility(this.ability);
		if (ability == null || !ability.CanCast)
		{
			return null;
		}
		LocalTargetInfo target = GetTarget(pawn, ability);
		if (!target.IsValid)
		{
			return null;
		}
		return ability.GetJob(target, target);
	}

	protected abstract LocalTargetInfo GetTarget(Pawn caster, Ability ability);

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AICastAbility obj = (JobGiver_AICastAbility)base.DeepCopy(resolve);
		obj.ability = ability;
		return obj;
	}
}
