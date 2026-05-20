using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryToShuttle : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.RoyaltyActive;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!CanBeCarriedToShuttle(clickedPawn))
		{
			return null;
		}
		if (!clickedPawn.Spawned)
		{
			return null;
		}
		if (!clickedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
		{
			return null;
		}
		Thing shuttleThing = GenClosest.ClosestThingReachable(clickedPawn.Position, clickedPawn.Map, ThingRequest.ForDef(ThingDefOf.Shuttle), PathEndMode.ClosestTouch, TraverseParms.For(context.FirstSelectedPawn), 9999f, IsValidShuttle);
		if (shuttleThing == null)
		{
			return null;
		}
		if (context.FirstSelectedPawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
		{
			return new FloatMenuOption("CannotLoadIntoShuttle".Translate(shuttleThing) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToShuttle".Translate(clickedPawn), CarryToShuttleAct), context.FirstSelectedPawn, clickedPawn);
		void CarryToShuttleAct()
		{
			CompTransporter compTransporter = shuttleThing.TryGetComp<CompTransporter>();
			if (!compTransporter.LoadingInProgressOrReadyToLaunch)
			{
				TransporterUtility.InitiateLoading(Gen.YieldSingle(compTransporter));
			}
			Job job = JobMaker.MakeJob(JobDefOf.HaulToTransporter, clickedPawn, shuttleThing);
			job.ignoreForbidden = true;
			job.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
		bool IsValidShuttle(Thing thing)
		{
			return thing.TryGetComp<CompShuttle>()?.IsAllowedNow(clickedPawn) ?? false;
		}
	}

	private bool CanBeCarriedToShuttle(Pawn pawn)
	{
		if (pawn.IsPrisonerOfColony)
		{
			return pawn.guest.PrisonerIsSecure;
		}
		if (!pawn.AnimalOrWildMan())
		{
			return pawn.Downed;
		}
		return true;
	}
}
