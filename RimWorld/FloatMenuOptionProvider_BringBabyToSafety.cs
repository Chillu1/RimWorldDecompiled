using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_BringBabyToSafety : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	public override bool CanTargetDespawned => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.BiotechActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!HealthAIUtility.CanRescueNow(context.FirstSelectedPawn, clickedPawn, forced: true))
		{
			return null;
		}
		if (clickedPawn.IsPrisonerOfColony || clickedPawn.IsSlaveOfColony || clickedPawn.IsColonyMech)
		{
			return null;
		}
		if (!ChildcareUtility.CanSuckle(clickedPawn, out var _))
		{
			return null;
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PutSomewhereSafe".Translate(clickedPawn.LabelCap, clickedPawn), delegate
		{
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(ChildcareUtility.MakeBringBabyToSafetyJob(context.FirstSelectedPawn, clickedPawn), JobTag.Misc);
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
	}
}
