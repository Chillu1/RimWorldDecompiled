using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AISkipToJobTarget : ThinkNode_JobGiver
{
	public AbilityDef ability;

	public TargetIndex targetIndex = TargetIndex.A;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Ability ability = pawn.abilities?.GetAbility(this.ability);
		if (ability == null || !ability.CanCast)
		{
			return null;
		}
		Job curJob = pawn.CurJob;
		if (curJob == null || curJob.def == this.ability.jobDef)
		{
			return null;
		}
		LocalTargetInfo target = curJob.GetTarget(targetIndex);
		if (!CanSkipToTarget(pawn, target))
		{
			return null;
		}
		IntVec3 result = target.Cell;
		if (target.HasThing && !RCellFinder.TryFindGoodAdjacentSpotToTouch(pawn, target.Thing, out result))
		{
			return null;
		}
		if (ability.verb.ValidateTarget(target, showMessages: false))
		{
			Job job = ability.GetJob(pawn, result);
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
		}
		return null;
	}

	protected virtual bool CanSkipToTarget(Pawn pawn, LocalTargetInfo target)
	{
		return target.Cell.Fogged(pawn.Map);
	}
}
