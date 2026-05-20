using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Strip : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Pawn pawn = clickedThing as Pawn;
		if (!StrippableUtility.CanBeStrippedByColony(clickedThing))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotStrip".Translate(clickedThing.LabelCap, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (pawn != null && pawn.HasExtraHomeFaction())
		{
			return new FloatMenuOption("CannotStrip".Translate(pawn.LabelCap, pawn) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Strip".Translate(clickedThing.LabelCap, clickedThing), delegate
		{
			clickedThing.SetForbidden(value: false, warnOnFail: false);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Strip, clickedThing), JobTag.Misc);
			StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(clickedThing);
		}), context.FirstSelectedPawn, clickedThing);
	}
}
