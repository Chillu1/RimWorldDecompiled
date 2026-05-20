using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryingPawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return context.FirstSelectedPawn.IsCarryingPawn();
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		Pawn carriedPawn = (Pawn)context.FirstSelectedPawn.carryTracker.CarriedThing;
		FloatMenuOption option2;
		if (Capture(clickedThing, context, carriedPawn, out var option))
		{
			yield return option;
		}
		else if (CarryToBed(clickedThing, context, carriedPawn, out option2))
		{
			yield return option2;
		}
		if (CarryToTransporter(clickedThing, context, carriedPawn, out var option3))
		{
			yield return option3;
		}
		if (CarryToCasket(clickedThing, context, carriedPawn, out var option4))
		{
			yield return option4;
		}
		if (ModsConfig.AnomalyActive && CaptureEntity(clickedThing, context, carriedPawn, out var option5))
		{
			yield return option5;
		}
	}

	private bool CarryToBed(Thing clickedThing, FloatMenuContext context, Pawn carriedPawn, out FloatMenuOption option)
	{
		option = null;
		if (carriedPawn.IsPrisonerOfColony)
		{
			return false;
		}
		if (!RestUtility.IsValidBedFor(clickedThing, carriedPawn, context.FirstSelectedPawn, checkSocialProperness: false, allowMedBedEvenIfSetToNoCare: true, ignoreOtherReservations: true, carriedPawn.GuestStatus))
		{
			return false;
		}
		if (context.FirstSelectedPawn.HostileTo(carriedPawn))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "CarriedPawnHostile".Translate().CapitalizeFirst(), null);
			return true;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			return true;
		}
		option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(carriedPawn, clickedThing), delegate
		{
			clickedThing.SetForbidden(value: false, warnOnFail: false);
			Job job = JobMaker.MakeJob(JobDefOf.TakeDownedPawnToBedDrafted, context.FirstSelectedPawn.carryTracker.CarriedThing, clickedThing);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedThing);
		return true;
	}

	private bool Capture(Thing clickedThing, FloatMenuContext context, Pawn carriedPawn, out FloatMenuOption option)
	{
		option = null;
		if (!carriedPawn.CanBeCaptured())
		{
			return false;
		}
		if (!RestUtility.IsValidBedFor(clickedThing, carriedPawn, context.FirstSelectedPawn, checkSocialProperness: false, allowMedBedEvenIfSetToNoCare: true, ignoreOtherReservations: true, GuestStatus.Prisoner))
		{
			return false;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			return true;
		}
		TaggedString taggedString = "Capture".Translate(carriedPawn.LabelCap, carriedPawn);
		if (!carriedPawn.guest.Recruitable)
		{
			taggedString += string.Format(" ({0})", "Unrecruitable".Translate());
		}
		if (carriedPawn.Faction != null && carriedPawn.Faction != Faction.OfPlayer && !carriedPawn.Faction.Hidden && !carriedPawn.Faction.HostileTo(Faction.OfPlayer) && !carriedPawn.IsPrisonerOfColony)
		{
			taggedString += string.Format(": {0}", "AngersFaction".Translate().CapitalizeFirst());
		}
		option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
		{
			clickedThing.SetForbidden(value: false, warnOnFail: false);
			Job job = JobMaker.MakeJob(JobDefOf.CarryToPrisonerBedDrafted, context.FirstSelectedPawn.carryTracker.CarriedThing, clickedThing);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedThing);
		return true;
	}

	private bool CarryToTransporter(Thing clickedThing, FloatMenuContext context, Pawn carriedPawn, out FloatMenuOption option)
	{
		option = null;
		if (!clickedThing.TryGetComp(out CompTransporter transporter))
		{
			return false;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			return true;
		}
		if (transporter.Shuttle != null)
		{
			if (!transporter.Shuttle.IsAllowedNow(carriedPawn))
			{
				return false;
			}
			if (!transporter.Shuttle.IsPlayerShuttle && !transporter.LeftToLoadContains(carriedPawn))
			{
				option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "NotPartOfLaunchGroup".Translate(), null);
				return true;
			}
		}
		option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(carriedPawn, clickedThing), delegate
		{
			if (!transporter.LoadingInProgressOrReadyToLaunch)
			{
				TransporterUtility.InitiateLoading(Gen.YieldSingle(transporter));
			}
			Job job = JobMaker.MakeJob(JobDefOf.HaulToTransporter, carriedPawn, clickedThing);
			job.ignoreForbidden = true;
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedThing);
		return true;
	}

	private bool CarryToCasket(Thing clickedThing, FloatMenuContext context, Pawn carriedPawn, out FloatMenuOption option)
	{
		option = null;
		Building_CryptosleepCasket casket = clickedThing as Building_CryptosleepCasket;
		if (casket == null)
		{
			return false;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			return true;
		}
		if (casket.HasAnyContents)
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketOccupied".Translate(), null);
			return true;
		}
		if (carriedPawn.IsQuestLodger())
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketGuestsNotAllowed".Translate(), null);
			return true;
		}
		if (carriedPawn.GetExtraHostFaction() != null)
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketGuestPrisonersNotAllowed".Translate(), null);
			return true;
		}
		option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(carriedPawn, casket), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.CarryToCryptosleepCasketDrafted, carriedPawn, casket);
			job.count = 1;
			job.playerForced = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, casket);
		return true;
	}

	private bool CaptureEntity(Thing clickedThing, FloatMenuContext context, Pawn carriedPawn, out FloatMenuOption option)
	{
		option = null;
		if (!carriedPawn.TryGetComp<CompHoldingPlatformTarget>(out var comp) || !comp.CanBeCaptured || !comp.StudiedAtHoldingPlatform)
		{
			return false;
		}
		if (!clickedThing.TryGetComp(out CompEntityHolder comp2) || !comp2.Available)
		{
			return false;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			option = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			return true;
		}
		TaggedString taggedString = "Capture".Translate(carriedPawn.LabelCap, carriedPawn);
		if (!clickedThing.SafelyContains(carriedPawn))
		{
			float statValue = carriedPawn.GetStatValue(StatDefOf.MinimumContainmentStrength);
			taggedString += string.Format(" ({0} {1:F0}, {2} {3:F0})", "FloatMenuContainmentStrength".Translate().ToLower(), comp2.ContainmentStrength, "FloatMenuContainmentRequires".Translate(carriedPawn).ToLower(), statValue);
		}
		option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
		{
			clickedThing.SetForbidden(value: false, warnOnFail: false);
			Job job = JobMaker.MakeJob(JobDefOf.CarryToEntityHolderAlreadyHolding, clickedThing, context.FirstSelectedPawn.carryTracker.CarriedThing);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedThing);
		return true;
	}
}
