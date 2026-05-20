using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_DeliverPawnToPsychicRitualCell : JobGiver_DeliverPawnToCell
{
	public bool skipIfTargetCanReach;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_DeliverPawnToPsychicRitualCell obj = (JobGiver_DeliverPawnToPsychicRitualCell)base.DeepCopy(resolve);
		obj.skipIfTargetCanReach = skipIfTargetCanReach;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
		if (pawn2 == null || pawn2.Dead)
		{
			return null;
		}
		if (pawn2.GetLord() != pawn.GetLord())
		{
			return null;
		}
		if (skipIfTargetCanReach && !pawn2.Downed && !pawn2.IsPrisoner)
		{
			return null;
		}
		if (pawn2.mindState.duty == null)
		{
			return null;
		}
		LocalTargetInfo destination = GetDestination(pawn2);
		if (!destination.IsValid || pawn2.Position == destination.Cell)
		{
			return null;
		}
		if (!pawn.CanReach(pawn2, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.DeliverToCell, pawn2, destination).WithCount(1);
		job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
		job.expiryInterval = jobMaxDuration;
		return job;
	}
}
