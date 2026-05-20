using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CapturePawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.CanBeCaptured())
		{
			return null;
		}
		if (!HealthAIUtility.CanRescueNow(context.FirstSelectedPawn, clickedPawn, forced: true))
		{
			return null;
		}
		TaggedString taggedString = "Capture".Translate(clickedPawn.LabelCap, clickedPawn);
		if (!clickedPawn.guest.Recruitable)
		{
			taggedString += " (" + "Unrecruitable".Translate() + ")";
		}
		if (clickedPawn.Faction != null && clickedPawn.Faction != Faction.OfPlayer && !clickedPawn.Faction.Hidden && !clickedPawn.Faction.HostileTo(Faction.OfPlayer) && !clickedPawn.IsPrisonerOfColony)
		{
			taggedString += ": " + "AngersFaction".Translate().CapitalizeFirst();
		}
		FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
		{
			Building_Bed building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
			if (building_Bed == null)
			{
				building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Prisoner);
			}
			if (building_Bed == null)
			{
				Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.Capture, clickedPawn, building_Bed);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
				if (clickedPawn.Faction != null && clickedPawn.Faction != Faction.OfPlayer && !clickedPawn.Faction.Hidden && !clickedPawn.Faction.HostileTo(Faction.OfPlayer) && !clickedPawn.IsPrisonerOfColony)
				{
					Messages.Message("MessageCapturingWillAngerFaction".Translate(clickedPawn.Named("PAWN")).AdjustedFor(clickedPawn), clickedPawn, MessageTypeDefOf.CautionInput, historical: false);
				}
			}
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		string cannot = string.Format("{0}: {1}", "CannotCapture".Translate(), "NoPrisonerBed".Translate());
		FloatMenuUtility.ValidateTakeToBedOption(context.FirstSelectedPawn, clickedPawn, floatMenuOption, cannot, GuestStatus.Prisoner);
		return floatMenuOption;
	}
}
