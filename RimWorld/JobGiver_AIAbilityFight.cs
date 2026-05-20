using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIAbilityFight : JobGiver_AIFightEnemy
{
	private AbilityDef ability;

	private bool skipIfCantTargetNow = true;

	protected override bool OnlyUseAbilityVerbs => true;

	protected override bool OnlyUseRangedSearch => true;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIAbilityFight obj = (JobGiver_AIAbilityFight)base.DeepCopy(resolve);
		obj.ability = ability;
		obj.skipIfCantTargetNow = skipIfCantTargetNow;
		return obj;
	}

	protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
	{
		dest = IntVec3.Invalid;
		Thing enemyTarget = pawn.mindState.enemyTarget;
		Ability ability = pawn.abilities.GetAbility(this.ability);
		return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
		{
			caster = pawn,
			target = enemyTarget,
			verb = ability.verb,
			maxRangeFromTarget = ability.verb.EffectiveRange,
			wantCoverFromTarget = false,
			preferredCastPosition = pawn.Position
		}, out dest);
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.abilities.GetAbility(ability).OnCooldown && skipIfCantTargetNow)
		{
			return null;
		}
		return base.TryGiveJob(pawn);
	}

	protected override bool ShouldLoseTarget(Pawn pawn)
	{
		if (base.ShouldLoseTarget(pawn))
		{
			return true;
		}
		return !CanTarget(pawn, pawn.mindState.enemyTarget);
	}

	protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
	{
		if (base.ExtraTargetValidator(pawn, target))
		{
			return CanTarget(pawn, target);
		}
		return false;
	}

	private bool CanTarget(Pawn pawn, Thing target)
	{
		if (!this.ability.verbProperties.targetParams.CanTarget(target))
		{
			return false;
		}
		Ability ability = pawn.abilities.GetAbility(this.ability);
		if (!ability.CanApplyOn((LocalTargetInfo)target))
		{
			return false;
		}
		if (skipIfCantTargetNow)
		{
			return ability.AICanTargetNow(target);
		}
		return true;
	}
}
