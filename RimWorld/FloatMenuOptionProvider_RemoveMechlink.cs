using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_RemoveMechlink : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.BiotechActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Corpse corpse = clickedThing as Corpse;
		if (corpse == null)
		{
			return null;
		}
		if (!corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
		{
			return null;
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Extract".Translate() + " " + HediffDefOf.MechlinkImplant.label, delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.RemoveMechlink, corpse);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, new LocalTargetInfo(corpse));
	}
}
