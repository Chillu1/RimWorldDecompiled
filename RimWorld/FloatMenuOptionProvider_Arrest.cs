using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Arrest : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.CanBeArrestedBy(context.FirstSelectedPawn))
		{
			return null;
		}
		if (clickedPawn.Downed && clickedPawn.guilt.IsGuilty)
		{
			return null;
		}
		if (!context.FirstSelectedPawn.Drafted && (!clickedPawn.IsWildMan() || clickedPawn.IsPrisonerOfColony))
		{
			return null;
		}
		if (context.FirstSelectedPawn.InSameExtraFaction(clickedPawn, ExtraFactionType.HomeFaction) || context.FirstSelectedPawn.InSameExtraFaction(clickedPawn, ExtraFactionType.MiniFaction))
		{
			return new FloatMenuOption("CannotArrest".Translate() + ": " + "SameFaction".Translate(clickedPawn), null);
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly))
		{
			return new FloatMenuOption("CannotArrest".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		Action action = delegate
		{
			Building_Bed building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
			if (building_Bed == null)
			{
				building_Bed = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Prisoner);
			}
			if (building_Bed == null)
			{
				Messages.Message("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.Arrest, clickedPawn, building_Bed);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				if (clickedPawn.Faction != null && ((clickedPawn.Faction != Faction.OfPlayer && !clickedPawn.Faction.Hidden) || clickedPawn.IsQuestLodger()))
				{
					TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.ArrestingCreatesEnemies, clickedPawn.GetAcceptArrestChance(context.FirstSelectedPawn).ToStringPercent());
				}
			}
		};
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TryToArrest".Translate(clickedPawn.LabelCap, clickedPawn, clickedPawn.GetAcceptArrestChance(context.FirstSelectedPawn).ToStringPercent()), action, MenuOptionPriority.High, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
	}
}
