using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ConstructRemoveFoundation : WorkGiver_ConstructAffectFloor
{
	protected override DesignationDef DesDef => DesignationDefOf.RemoveFoundation;

	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		Job job = RemoveFloorJob(pawn, c);
		if (job != null)
		{
			return job;
		}
		Job job2 = RemoveFoundationJob(pawn, c);
		if (job2 != null)
		{
			return job2;
		}
		return JobMaker.MakeJob(JobDefOf.RemoveFoundation, c);
	}

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		if (!base.HasJobOnCell(pawn, c, forced))
		{
			return false;
		}
		if (!pawn.Map.terrainGrid.CanRemoveFoundationAt(c))
		{
			return false;
		}
		if (AnyBuildingBlockingFoundationRemoval(c, pawn.Map))
		{
			return false;
		}
		return true;
	}

	public static bool AnyBuildingBlockingFoundationRemoval(IntVec3 c, Map map)
	{
		if (!map.terrainGrid.CanRemoveFoundationAt(c))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(map))
		{
			ThingDef buildDef = thing.def;
			if (thing is Blueprint_Build blueprint_Build)
			{
				buildDef = blueprint_Build.BuildDef;
			}
			else if (thing is Blueprint_Install blueprint_Install)
			{
				buildDef = blueprint_Install.MiniToInstallOrBuildingToReinstall.def;
			}
			else if (thing is Frame frame)
			{
				buildDef = frame.BuildDef;
			}
			if (buildDef.terrainAffordanceNeeded != null && ((map.terrainGrid.UnderTerrainAt(c) != null) ? (!map.terrainGrid.UnderTerrainAt(c).affordances.Contains(buildDef.terrainAffordanceNeeded)) : (!map.terrainGrid.TopTerrainAt(c).affordances.Contains(buildDef.terrainAffordanceNeeded))))
			{
				return true;
			}
		}
		return false;
	}

	private Job RemoveFloorJob(Pawn pawn, IntVec3 c)
	{
		if (pawn.Map.terrainGrid.UnderTerrainAt(c) == null || !pawn.Map.terrainGrid.CanRemoveTopLayerAt(c))
		{
			return null;
		}
		if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Floor))
		{
			return null;
		}
		if (pawn.WorkTypeIsDisabled(WorkGiverDefOf.ConstructRemoveFloors.workType))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.RemoveFloor, c);
		job.ignoreDesignations = true;
		return job;
	}

	private Job RemoveFoundationJob(Pawn pawn, IntVec3 c)
	{
		if (!pawn.Map.terrainGrid.CanRemoveFoundationAt(c))
		{
			return null;
		}
		if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Floor))
		{
			return null;
		}
		if (pawn.WorkTypeIsDisabled(WorkGiverDefOf.ConstructRemoveFloors.workType))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.RemoveFoundation, c);
		job.ignoreDesignations = true;
		return job;
	}
}
