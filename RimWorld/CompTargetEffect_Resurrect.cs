using Verse;
using Verse.AI;

namespace RimWorld;

public class CompTargetEffect_Resurrect : CompTargetEffect
{
	public CompProperties_TargetEffectResurrect Props => (CompProperties_TargetEffectResurrect)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (user.IsColonistPlayerControlled)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Resurrect, target, parent);
			job.count = 1;
			job.playerForced = true;
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}
}
