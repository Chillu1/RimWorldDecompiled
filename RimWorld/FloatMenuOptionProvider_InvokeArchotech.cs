using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_InvokeArchotech : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.IdeologyActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Building_ArchonexusCore archCore = clickedThing as Building_ArchonexusCore;
		if (archCore == null)
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(archCore, PathEndMode.InteractionCell, Danger.Deadly))
		{
			return new FloatMenuOption("CannotInvoke".Translate("Power".Translate()) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (!archCore.CanActivateNow)
		{
			return new FloatMenuOption("CannotInvoke".Translate("Power".Translate()) + ": " + "AlreadyInvoked".Translate(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Invoke".Translate("Power".Translate()), delegate
		{
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.ActivateArchonexusCore, archCore), JobTag.Misc);
		}), context.FirstSelectedPawn, archCore);
	}
}
