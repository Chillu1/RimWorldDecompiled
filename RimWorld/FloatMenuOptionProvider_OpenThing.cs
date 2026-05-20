using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_OpenThing : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!(clickedThing is IOpenable { CanOpen: not false }))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.OnCell, Danger.Deadly))
		{
			return new FloatMenuOption("CannotOpen".Translate(clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (!context.FirstSelectedPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return new FloatMenuOption("CannotOpen".Translate(clickedThing) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
		}
		if (!context.FirstSelectedPawn.Drafted && clickedThing.Map.designationManager.DesignationOn(clickedThing, DesignationDefOf.Open) != null)
		{
			return null;
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Open".Translate(clickedThing), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.Open, clickedThing);
			job.ignoreDesignations = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
	}
}
