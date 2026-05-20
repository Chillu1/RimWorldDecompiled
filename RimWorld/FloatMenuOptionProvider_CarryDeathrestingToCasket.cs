using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryDeathrestingToCasket : FloatMenuOptionProvider
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
		if (!clickedPawn.Deathresting || clickedPawn.InBed())
		{
			return null;
		}
		if (!clickedPawn.IsColonist && !clickedPawn.IsPrisonerOfColony)
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotCarry".Translate(clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		Thing bestBedOrCasket = GenClosest.ClosestThingReachable(clickedPawn.PositionHeld, context.FirstSelectedPawn.Map, ThingRequest.ForDef(ThingDefOf.DeathrestCasket), PathEndMode.ClosestTouch, TraverseParms.For(context.FirstSelectedPawn), 9999f, (Thing casket) => casket.Faction == Faction.OfPlayer && RestUtility.IsValidBedFor(casket, clickedPawn, context.FirstSelectedPawn, checkSocialProperness: true, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations: false, clickedPawn.GuestStatus));
		if (bestBedOrCasket == null)
		{
			bestBedOrCasket = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false);
		}
		if (bestBedOrCasket == null)
		{
			return new FloatMenuOption("CannotCarry".Translate(clickedPawn) + ": " + "NoCasketOrBed".Translate(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToSpecificThing".Translate(bestBedOrCasket), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.DeliverToBed, clickedPawn, bestBedOrCasket);
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
	}
}
