using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Replant : WorkGiver_ConstructDeliverResources
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Blueprint);

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.Faction != pawn.Faction)
		{
			return null;
		}
		if (!(t is Blueprint_Install blueprint_Install) || !(blueprint_Install.def.entityDefToBuild is ThingDef { plant: not null } thingDef))
		{
			return null;
		}
		if (GenConstruct.FirstBlockingThing(blueprint_Install, pawn) != null)
		{
			return GenConstruct.HandleBlockingThingJob(blueprint_Install, pawn, forced);
		}
		Thing blockingThing;
		AcceptanceReport acceptanceReport = thingDef.CanEverPlantAt(t.Position, t.Map, out blockingThing, canWipePlantsExceptTree: true);
		if (!acceptanceReport)
		{
			JobFailReason.Is(acceptanceReport.Reason);
			return null;
		}
		if (!thingDef.CanNowPlantAt(t.Position, t.Map, canWipePlantsExceptTree: true))
		{
			return null;
		}
		if (thingDef.plant.interferesWithRoof && t.Position.Roofed(t.Map))
		{
			JobFailReason.Is("BlockedByRoof".Translate());
			return null;
		}
		Thing miniToInstallOrBuildingToReinstall = blueprint_Install.MiniToInstallOrBuildingToReinstall;
		IThingHolder parentHolder = miniToInstallOrBuildingToReinstall.ParentHolder;
		if (parentHolder != null && parentHolder is Pawn_CarryTracker pawn_CarryTracker)
		{
			JobFailReason.Is("BeingCarriedBy".Translate(pawn_CarryTracker.pawn));
			return null;
		}
		if (miniToInstallOrBuildingToReinstall.IsForbidden(pawn))
		{
			JobFailReason.Is(WorkGiver_ConstructDeliverResources.ForbiddenLowerTranslated);
			return null;
		}
		if (!pawn.CanReach(miniToInstallOrBuildingToReinstall, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()))
		{
			JobFailReason.Is(WorkGiver_ConstructDeliverResources.NoPathTranslated);
			return null;
		}
		if (!pawn.CanReserve(miniToInstallOrBuildingToReinstall))
		{
			Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(miniToInstallOrBuildingToReinstall, pawn);
			if (pawn2 != null)
			{
				JobFailReason.Is("ReservedBy".Translate(pawn2.LabelShort, pawn2));
			}
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Replant);
		job.targetA = miniToInstallOrBuildingToReinstall;
		job.targetB = blueprint_Install;
		job.plantDefToSow = thingDef;
		job.count = 1;
		job.haulMode = HaulMode.ToContainer;
		return job;
	}
}
