using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public class JobDriver_CastAbilityWorld : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_General.Do(delegate
		{
			pawn.stances.SetStance(new Stance_WarmupAbilityWorld(job.ability.def.verbProperties.warmupTime.SecondsToTicks(), null, job.ability.verb));
		});
		yield return Toils_General.Do(delegate
		{
			job.ability.Activate(job.globalTarget);
		});
	}

	public override string GetReport()
	{
		return "UsingVerb".Translate(job.ability.def.label, job.globalTarget.Label);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		job.ability?.Notify_StartedCasting();
	}
}
