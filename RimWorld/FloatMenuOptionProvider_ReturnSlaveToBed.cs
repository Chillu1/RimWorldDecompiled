using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_ReturnSlaveToBed : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.IsSlaveOfColony || clickedPawn.InMentalState)
		{
			return null;
		}
		if (!clickedPawn.Downed)
		{
			return null;
		}
		if (clickedPawn.InBed())
		{
			return null;
		}
		FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ReturnToSlaveBed".Translate(), delegate
		{
			Building_Bed building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Slave);
			if (building_Bed == null)
			{
				building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Slave);
			}
			if (building_Bed == null)
			{
				Messages.Message(string.Format("{0}: {1}", "CannotRescue".Translate(), "NoSlaveBed".Translate()), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.Rescue, clickedPawn, building_Bed);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
			}
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		string cannot = string.Format("{0}: {1}", "CannotRescue".Translate(), "NoSlaveBed".Translate());
		FloatMenuUtility.ValidateTakeToBedOption(context.FirstSelectedPawn, clickedPawn, floatMenuOption, cannot, GuestStatus.Slave);
		return floatMenuOption;
	}
}
