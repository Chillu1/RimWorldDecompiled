using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryPawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (context.FirstSelectedPawn.IsMutant)
		{
			return !context.FirstSelectedPawn.mutant.Def.canCarryPawns;
		}
		return true;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.Downed && !clickedPawn.IsSelfShutdown())
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotCarry".Translate(clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (!context.FirstSelectedPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return new FloatMenuOption("CannotCarry".Translate(context.FirstSelectedPawn) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Carry".Translate(clickedPawn), delegate
		{
			clickedPawn.SetForbidden(value: false, warnOnFail: false);
			Job job = JobMaker.MakeJob(JobDefOf.CarryDownedPawnDrafted, clickedPawn);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedPawn);
	}
}
