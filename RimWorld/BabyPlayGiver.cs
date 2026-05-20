using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class BabyPlayGiver
	{
		public BabyPlayDef def;

		public BabyPlayGiver()
		{
		}

		public abstract bool CanDo(Pawn pawn, Pawn baby);

		public virtual Job TryGiveJob(Pawn pawn, Pawn baby)
		{
			Job job = JobMaker.MakeJob(def.jobDef, baby);
			job.count = 1;
			return job;
		}
	}
}
