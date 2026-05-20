using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryPawnToExit : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!context.map.IsPlayerHome)
		{
			return context.map.exitMapGrid.MapUsesExitGrid;
		}
		return false;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.Downed)
		{
			return null;
		}
		if (clickedPawn.Faction != Faction.OfPlayer && !clickedPawn.IsPrisonerOfColony && !CaravanUtility.ShouldAutoCapture(clickedPawn, Faction.OfPlayer))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotCarryToExit".Translate(clickedPawn.Label, clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (context.FirstSelectedPawn.Map.IsPocketMap)
		{
			if (!RCellFinder.TryFindExitPortal(context.FirstSelectedPawn, out var portal))
			{
				return new FloatMenuOption("CannotCarryToExit".Translate(clickedPawn.Label, clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			}
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((clickedPawn.Faction == Faction.OfPlayer || clickedPawn.IsPrisonerOfColony) ? "CarryToExit".Translate(clickedPawn.Label, clickedPawn) : "CarryToExitAndCapture".Translate(clickedPawn.Label, clickedPawn), delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.CarryDownedPawnToPortal, portal, clickedPawn);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedPawn);
		}
		if (!RCellFinder.TryFindBestExitSpot(context.FirstSelectedPawn, out var exitSpot))
		{
			return new FloatMenuOption("CannotCarryToExit".Translate(clickedPawn.Label, clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((clickedPawn.Faction == Faction.OfPlayer || clickedPawn.IsPrisonerOfColony) ? "CarryToExit".Translate(clickedPawn.Label, clickedPawn) : "CarryToExitAndCapture".Translate(clickedPawn.Label, clickedPawn), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.CarryDownedPawnToExit, clickedPawn, exitSpot);
			job.count = 1;
			job.failIfCantJoinOrCreateCaravan = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedPawn);
	}
}
