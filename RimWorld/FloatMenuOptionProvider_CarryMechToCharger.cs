using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryMechToCharger : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.BiotechActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (clickedPawn == context.FirstSelectedPawn)
		{
			return null;
		}
		if (!clickedPawn.IsSelfShutdown())
		{
			return null;
		}
		Building_MechCharger charger = JobGiver_GetEnergy_Charger.GetClosestCharger(clickedPawn, context.FirstSelectedPawn, forced: false);
		if (charger == null)
		{
			charger = JobGiver_GetEnergy_Charger.GetClosestCharger(clickedPawn, context.FirstSelectedPawn, forced: true);
		}
		if (charger == null)
		{
			return new FloatMenuOption("CannotCarryToRecharger".Translate(clickedPawn.Named("PAWN")) + ": " + "CannotCarryToRechargerNoneAvailable".Translate(), null);
		}
		if (!context.FirstSelectedPawn.CanReach(charger, PathEndMode.Touch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotCarryToRecharger".Translate(clickedPawn.Named("PAWN")) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToRechargerOrdered".Translate(clickedPawn.Named("PAWN")), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.HaulMechToCharger, clickedPawn, charger, charger.InteractionCell);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn));
	}
}
