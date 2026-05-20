using System.Collections.Generic;

namespace Verse.AI;

public class JobDriver_CastAbility : JobDriver_CastVerbOnce
{
	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOn(() => !job.ability.CanCast && !job.ability.Casting);
		AddFinishAction(delegate
		{
			if (job.ability != null && job.def.abilityCasting)
			{
				job.ability.StartCooldown(job.ability.def.cooldownTicksRange.RandomInRange);
			}
		});
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.pather.StopDead();
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
		Toil toil2 = Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
		if (job.ability != null && job.ability.def.showCastingProgressBar && job.verbToUse != null)
		{
			toil2.WithProgressBar(TargetIndex.A, () => job.verbToUse.WarmupProgress);
		}
		yield return toil2;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		job.ability?.Notify_StartedCasting();
	}

	public override string GetReport()
	{
		string text = "";
		text = ((job.ability == null || job.ability.def.targetRequired) ? base.GetReport() : ((string)"UsingVerbNoTarget".Translate(job.verbToUse.ReportLabel)));
		if (job.ability != null && job.ability.def.showCastingProgressBar)
		{
			text += " " + "DurationLeft".Translate(job.verbToUse.WarmupTicksLeft.ToStringSecondsFromTicks()) + ".";
		}
		return text;
	}
}
