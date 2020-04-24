using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FixBrokenDownBuilding : WorkGiver_Scanner
	{
		public static string NotInHomeAreaTrans;

		private static string NoComponentsToRepairTrans;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public static void ResetStaticData()
		{
			NotInHomeAreaTrans = "NotInHomeArea".Translate();
			NoComponentsToRepairTrans = "NoComponentsToRepair".Translate();
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.GetComponent<BreakdownManager>().brokenDownThings;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.Map.GetComponent<BreakdownManager>().brokenDownThings.Count == 0;
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (!building.def.building.repairable)
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (!t.IsBrokenDown())
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
			{
				JobFailReason.Is(NotInHomeAreaTrans);
				return false;
			}
			if (!pawn.CanReserve(building, 1, -1, null, forced))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (building.IsBurning())
			{
				return false;
			}
			if (FindClosestComponent(pawn) == null)
			{
				JobFailReason.Is(NoComponentsToRepairTrans);
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing t2 = FindClosestComponent(pawn);
			Job job = JobMaker.MakeJob(JobDefOf.FixBrokenDownBuilding, t, t2);
			job.count = 1;
			return job;
		}

		private Thing FindClosestComponent(Pawn pawn)
		{
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.ComponentIndustrial), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x));
		}
	}
}
