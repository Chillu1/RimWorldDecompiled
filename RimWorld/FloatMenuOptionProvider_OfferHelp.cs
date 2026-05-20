using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_OfferHelp : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (clickedPawn.Dead || !clickedPawn.mindState.WillJoinColonyIfRescued)
		{
			return null;
		}
		TaggedString taggedString = (clickedPawn.IsPrisoner ? "FreePrisoner".Translate() : "OfferHelp".Translate());
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
		{
			return new FloatMenuOption(taggedString + ": " + "NoPath".Translate(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
		{
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.OfferHelp, clickedPawn), JobTag.Misc);
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
	}
}
