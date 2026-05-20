using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_PutOutFireOnPawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (clickedPawn == null || clickedPawn.Dead || !clickedPawn.IsBurning())
		{
			return null;
		}
		if ((clickedPawn.Faction == null || clickedPawn.Faction != context.FirstSelectedPawn.Faction) && (clickedPawn.HostFaction == null || (clickedPawn.HostFaction != context.FirstSelectedPawn.Faction && clickedPawn.HostFaction != context.FirstSelectedPawn.HostFaction)))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
		{
			return new FloatMenuOption(string.Format("{0}: {1}", "CannotGenericWorkCustom".Translate(WorkGiverDefOf.FightFires.label), "NoPath".Translate().CapitalizeFirst()), null);
		}
		Thing fire = clickedPawn.GetAttachment(ThingDefOf.Fire);
		if (fire == null)
		{
			return null;
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PrioritizeGeneric".Translate(WorkGiverDefOf.FightFires.gerund, clickedPawn.Label).CapitalizeFirst(), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.BeatFire, fire);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, fire);
	}
}
