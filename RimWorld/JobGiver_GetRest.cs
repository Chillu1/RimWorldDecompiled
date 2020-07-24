using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
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
				if (curLevel < RestUtility.FallAsleepMaxLevel(pawn))
				{
					return 8f;
				}
				return 0f;
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
			Building_Bed building_Bed = ((lord == null || lord.CurLordToil == null || lord.CurLordToil.AllowRestingInBed) && !pawn.IsWildMan()) ? RestUtility.FindBedFor(pawn) : null;
			if (building_Bed != null)
			{
				return JobMaker.MakeJob(JobDefOf.LayDown, building_Bed);
			}
			return JobMaker.MakeJob(JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
		}

		private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
		{
			Map map = pawn.Map;
			for (int i = 0; i < 2; i++)
			{
				int radius = (i == 0) ? 4 : 12;
				if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out IntVec3 result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
				{
					return result;
				}
			}
			return CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 4, pawn);
		}
	}
}
