using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_MeditateInBed : JobGiver_Meditate
{
	protected override bool ValidatePawnState(Pawn pawn)
	{
		if (pawn.CurrentBed() != null && pawn.Awake())
		{
			return MeditationUtility.ShouldMeditateInBed(pawn);
		}
		return false;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!MeditationUtility.CanMeditateNow(pawn))
		{
			return null;
		}
		LocalTargetInfo targetC = (ModsConfig.RoyaltyActive ? MeditationUtility.BestFocusAt(pawn.Position, pawn) : LocalTargetInfo.Invalid);
		Job job = JobMaker.MakeJob(JobDefOf.Meditate, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position), targetC);
		job.ignoreJoyTimeAssignment = true;
		return job;
	}
}
