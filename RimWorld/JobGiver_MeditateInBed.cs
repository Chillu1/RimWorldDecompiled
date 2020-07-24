using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MeditateInBed : JobGiver_Meditate
	{
		protected override bool ValidatePawnState(Pawn pawn)
		{
			if (pawn.CurrentBed() != null)
			{
				return pawn.Awake();
			}
			return false;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			LocalTargetInfo targetC = MeditationUtility.BestFocusAt(pawn.Position, pawn);
			Job job = JobMaker.MakeJob(JobDefOf.Meditate, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position), targetC);
			job.ignoreJoyTimeAssignment = true;
			return job;
		}
	}
}
