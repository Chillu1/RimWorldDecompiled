using Verse;
using Verse.AI;

namespace RimWorld;

public class CompTargetEffect_InstallImplantInOtherPawn : CompTargetEffect
{
	public CompProperties_TargetEffectInstallImplantInOtherPawn Props => (CompProperties_TargetEffectInstallImplantInOtherPawn)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (user.IsColonistPlayerControlled)
		{
			Job job = JobMaker.MakeJob(JobDefOf.InstallImplant, target, parent);
			job.count = 1;
			job.playerForced = true;
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}
}
