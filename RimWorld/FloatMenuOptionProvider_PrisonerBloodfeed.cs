using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_PrisonerBloodfeed : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!context.FirstSelectedPawn.IsBloodfeeder())
		{
			return false;
		}
		if (context.FirstSelectedPawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null)
		{
			return false;
		}
		return true;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.IsBloodfeeder())
		{
			return null;
		}
		if (!clickedPawn.IsPrisonerOfColony || !clickedPawn.guest.PrisonerIsSecure || clickedPawn.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Bloodfeed))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotBloodfeedOn".Translate(clickedPawn.Named("PAWN")) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		AcceptanceReport acceptanceReport = JobGiver_GetHemogen.CanFeedOnPrisoner(context.FirstSelectedPawn, clickedPawn);
		if (!acceptanceReport.Accepted)
		{
			if (acceptanceReport.Reason.NullOrEmpty())
			{
				return null;
			}
			return new FloatMenuOption("CannotBloodfeedOn".Translate(clickedPawn.Named("PAWN")) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BloodfeedOn".Translate(clickedPawn.Named("PAWN")), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.PrisonerBloodfeed, clickedPawn);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedPawn);
	}
}
