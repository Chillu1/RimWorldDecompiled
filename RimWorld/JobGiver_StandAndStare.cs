using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_StandAndStare : ThinkNode_JobGiver
{
	protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

	protected Danger maxDanger = Danger.Some;

	public override ThinkNode DeepCopy(bool resolve)
	{
		JobGiver_StandAndStare obj = (JobGiver_StandAndStare)base.DeepCopy(resolve);
		obj.locomotionUrgency = locomotionUrgency;
		obj.maxDanger = maxDanger;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		LocalTargetInfo? localTargetInfo = pawn.mindState?.duty?.focusSecond;
		if (localTargetInfo.HasValue)
		{
			LocalTargetInfo localTargetInfo2 = localTargetInfo.GetValueOrDefault().Cell;
			if (pawn.Position != localTargetInfo2.Cell)
			{
				return null;
			}
		}
		if (pawn.Downed)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.StandAndStare);
		job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
		return job;
	}
}
