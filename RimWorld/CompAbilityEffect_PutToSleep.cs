using Verse;
using Verse.AI;

namespace RimWorld;

public class CompAbilityEffect_PutToSleep : CompAbilityEffect
{
	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			pawn.needs.rest.CurLevel = 0f;
			Job job = JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position);
			job.forceSleep = true;
			pawn.jobs.StartJob(job, JobCondition.InterruptForced);
		}
	}
}
