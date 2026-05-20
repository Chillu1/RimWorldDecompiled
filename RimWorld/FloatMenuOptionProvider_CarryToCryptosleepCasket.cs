using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryToCryptosleepCasket : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.Downed)
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
		{
			return null;
		}
		if (Building_CryptosleepCasket.FindCryptosleepCasketFor(clickedPawn, context.FirstSelectedPawn, ignoreOtherReservations: true) == null)
		{
			return null;
		}
		TaggedString taggedString = "CarryToCryptosleepCasket".Translate(clickedPawn.LabelCap, clickedPawn);
		if (clickedPawn.IsQuestLodger())
		{
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		}
		if (clickedPawn.GetExtraHostFaction() != null)
		{
			return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		}
		Action action = delegate
		{
			Building_CryptosleepCasket building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(clickedPawn, context.FirstSelectedPawn);
			if (building_CryptosleepCasket == null)
			{
				building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(clickedPawn, context.FirstSelectedPawn, ignoreOtherReservations: true);
			}
			if (building_CryptosleepCasket == null)
			{
				Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.CarryToCryptosleepCasket, clickedPawn, building_CryptosleepCasket);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		};
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, action, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
	}
}
