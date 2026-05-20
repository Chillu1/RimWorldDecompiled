using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_RescuePawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!HealthAIUtility.CanRescueNow(context.FirstSelectedPawn, clickedPawn, forced: true))
		{
			return null;
		}
		if (clickedPawn.mindState.WillJoinColonyIfRescued)
		{
			return null;
		}
		if (clickedPawn.IsPrisonerOfColony || clickedPawn.IsSlaveOfColony || clickedPawn.IsColonyMech)
		{
			return null;
		}
		if (clickedPawn.Faction != null && clickedPawn.Faction.HostileTo(Faction.OfPlayer))
		{
			return null;
		}
		ChildcareUtility.BreastfeedFailReason? reason;
		bool num = ChildcareUtility.CanSuckle(clickedPawn, out reason);
		bool flag = HealthAIUtility.ShouldSeekMedicalRest(clickedPawn) || !clickedPawn.ageTracker.CurLifeStage.alwaysDowned;
		if (num || !flag)
		{
			return null;
		}
		Pawn_PlayerSettings playerSettings = clickedPawn.playerSettings;
		if (playerSettings != null && playerSettings.medCare == MedicalCareCategory.NoCare)
		{
			return new FloatMenuOption("CannotRescuePawn".Translate(clickedPawn.Named("PAWN")) + ": " + "MedicalCareDisabled".Translate(), null);
		}
		FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Rescue".Translate(clickedPawn.LabelCap, clickedPawn), delegate
		{
			Building_Bed building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false);
			if (building_Bed == null)
			{
				building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: true);
			}
			if (building_Bed == null)
			{
				string text = ((!clickedPawn.RaceProps.Animal) ? ((string)"NoNonPrisonerBed".Translate()) : ((string)"NoAnimalBed".Translate()));
				Messages.Message("CannotRescue".Translate() + ": " + text, clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.Rescue, clickedPawn, building_Bed);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
			}
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		string key = (clickedPawn.RaceProps.Animal ? "NoAnimalBed" : "NoNonPrisonerBed");
		string cannot = string.Format("{0}: {1}", "CannotRescue".Translate(), key.Translate().CapitalizeFirst());
		FloatMenuUtility.ValidateTakeToBedOption(context.FirstSelectedPawn, clickedPawn, floatMenuOption, cannot);
		return floatMenuOption;
	}
}
