using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompTargetEffect_Resurrect : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				Job job = JobMaker.MakeJob(JobDefOf.Resurrect, target, parent);
				job.count = 1;
				user.jobs.TryTakeOrderedJob(job);
			}
		}
	}
}
