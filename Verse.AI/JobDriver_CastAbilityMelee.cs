using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_CastAbilityMelee : JobDriver_CastAbility
{
	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOn(() => !job.ability.CanCast && !job.ability.Casting);
		Ability ability = ((Verb_CastAbility)job.verbToUse).ability;
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !ability.CanApplyOn(job.targetA));
		yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		job.ability?.Notify_StartedCasting();
	}
}
