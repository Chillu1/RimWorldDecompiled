using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GetRest : ThinkNode_JobGiver
{
	private RestCategory minCategory;

	private float maxLevelPercentage = 1f;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_GetRest obj = (JobGiver_GetRest)base.DeepCopy(resolve);
		obj.minCategory = minCategory;
		obj.maxLevelPercentage = maxLevelPercentage;
		return obj;
	}

	public override float GetPriority(Pawn pawn)
	{
		Need_Rest rest = pawn.needs.rest;
		if (rest == null)
		{
			return 0f;
		}
		if ((int)rest.CurCategory < (int)minCategory)
		{
			return 0f;
		}
		if (rest.CurLevelPercentage > maxLevelPercentage)
		{
			return 0f;
		}
		if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
		{
			return 0f;
		}
		Lord lord = pawn.GetLord();
		if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
		{
			return 0f;
		}
		if (!RestUtility.CanFallAsleep(pawn))
		{
			return 0f;
		}
		TimeAssignmentDef timeAssignmentDef;
		if (pawn.RaceProps.Humanlike)
		{
			timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);
		}
		else
		{
			int num = GenLocalDate.HourOfDay(pawn);
			timeAssignmentDef = ((num >= 7 && num <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);
		}
		float curLevel = rest.CurLevel;
		if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
		{
			if (curLevel < 0.3f)
			{
				return 8f;
			}
			return 0f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Work)
		{
			return 0f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
		{
			if (curLevel < 0.16f)
			{
				return 8f;
			}
			return 0f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
		{
			if (curLevel < 0.3f)
			{
				return 8f;
			}
			return 0f;
		}
		if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
		{
			return 8f;
		}
		throw new NotImplementedException();
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Need_Rest rest = pawn.needs.rest;
		if (rest == null || (int)rest.CurCategory < (int)minCategory || rest.CurLevelPercentage > maxLevelPercentage)
		{
			return null;
		}
		if (RestUtility.DisturbancePreventsLyingDown(pawn))
		{
			return null;
		}
		Lord lord = pawn.GetLord();
		Building_Bed building_Bed;
		if ((lord == null || lord.CurLordToil == null || lord.CurLordToil.AllowRestingInBed) && !pawn.IsWildMan() && (!pawn.InMentalState || pawn.MentalState.AllowRestingInBed))
		{
			Pawn_RopeTracker roping = pawn.roping;
			if (roping == null || !roping.IsRoped)
			{
				building_Bed = RestUtility.FindBedFor(pawn);
				goto IL_0092;
			}
		}
		building_Bed = null;
		goto IL_0092;
		IL_0092:
		if (building_Bed != null)
		{
			return JobMaker.MakeJob(JobDefOf.LayDown, building_Bed);
		}
		if (TryFindGroundSleepSpotFor(pawn, out var cell))
		{
			return JobMaker.MakeJob(JobDefOf.LayDown, cell);
		}
		return null;
	}

	private bool TryFindGroundSleepSpotFor(Pawn pawn, out IntVec3 cell)
	{
		Map map = pawn.Map;
		IntVec3 position = pawn.Position;
		if (pawn.RaceProps.Dryad && pawn.connections != null)
		{
			foreach (Thing connectedThing in pawn.connections.ConnectedThings)
			{
				if (pawn.CanReach(connectedThing, PathEndMode.Touch, Danger.Deadly))
				{
					position = connectedThing.Position;
					break;
				}
			}
		}
		else if (IsValidCell(pawn, position))
		{
			cell = position;
			return true;
		}
		for (int i = 0; i < 2; i++)
		{
			int radius = ((i == 0) ? 4 : 12);
			if (CellFinder.TryRandomClosewalkCellNear(position, map, radius, out var result, (IntVec3 c) => IsValidCell(pawn, c)))
			{
				cell = result;
				return true;
			}
		}
		cell = CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4, (IntVec3 c) => IsValidCell(pawn, c));
		return IsValidCell(pawn, cell);
	}

	private static bool IsValidCell(Pawn pawn, IntVec3 cell)
	{
		if (!cell.IsForbidden(pawn) && !cell.GetTerrain(pawn.Map).avoidWander)
		{
			return pawn.CanReserve(cell);
		}
		return false;
	}
}
