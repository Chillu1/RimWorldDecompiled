using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIJumpToJobTarget : ThinkNode_JobGiver
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
		if (!CanJumpToTarget(pawn, target))
		{
			return null;
		}
		IntVec3 result = target.Cell;
		float num = pawn.Position.DistanceTo(result);
		VerbProperties verbProps = ability.verb.verbProps;
		if (num < verbProps.minRange || num > ability.verb.EffectiveRange || !GenSight.LineOfSight(pawn.Position, result, pawn.Map))
		{
			return null;
		}
		if (target.HasThing && !RCellFinder.TryFindGoodAdjacentSpotToTouch(pawn, target.Thing, out result))
		{
			return null;
		}
		if (ability.verb.ValidateTarget(target, showMessages: false))
		{
			Job job = ability.GetJob(result, result);
			pawn.jobs.StartJob(job, JobCondition.Ongoing, null, resumeCurJobAfterwards: true);
			FleckMaker.Static(result, pawn.Map, FleckDefOf.FeedbackGoto);
		}
		return null;
	}

	public virtual bool CanJumpToTarget(Pawn pawn, LocalTargetInfo target)
	{
		return true;
	}
}
