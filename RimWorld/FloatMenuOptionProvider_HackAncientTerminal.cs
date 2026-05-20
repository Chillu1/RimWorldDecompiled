using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_HackAncientTerminal : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!ModsConfig.IdeologyActive || clickedThing.def != ThingDefOf.AncientEnemyTerminal)
		{
			return null;
		}
		if (!clickedThing.TryGetComp(out CompHackable comp) || comp.Props.onlyRemotelyHackable)
		{
			return null;
		}
		if (!comp.CanHackNow(context.FirstSelectedPawn).Accepted)
		{
			return null;
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Hack".Translate(clickedThing.Label), delegate
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmHackEnemyTerminal".Translate(ThingDefOf.AncientEnemyTerminal.label), delegate
			{
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Hack, clickedThing), JobTag.Misc);
			}));
		}), context.FirstSelectedPawn, new LocalTargetInfo(clickedThing));
	}
}
