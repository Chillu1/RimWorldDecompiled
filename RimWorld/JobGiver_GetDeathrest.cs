using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GetDeathrest : ThinkNode_JobGiver
{
	public float maxNeedPercent = 0.05f;

	public override float GetPriority(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		Lord lord = pawn.GetLord();
		if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
		{
			return 0f;
		}
		if (pawn.needs == null || !pawn.needs.TryGetNeed(out Need_Deathrest need) || need.CurLevelPercentage > maxNeedPercent)
		{
			return 0f;
		}
		return 7.75f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		if (pawn.needs == null || !pawn.needs.TryGetNeed(out Need_Deathrest need))
		{
			return null;
		}
		if (need.CurLevelPercentage > maxNeedPercent)
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
				goto IL_008d;
			}
		}
		building_Bed = null;
		goto IL_008d;
		IL_008d:
		if (building_Bed != null)
		{
			return JobMaker.MakeJob(JobDefOf.Deathrest, building_Bed);
		}
		return JobMaker.MakeJob(JobDefOf.Deathrest, FindGroundSleepSpotFor(pawn));
	}

	private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
	{
		Map map = pawn.Map;
		IntVec3 position = pawn.Position;
		for (int i = 0; i < 2; i++)
		{
			int radius = ((i == 0) ? 4 : 12);
			if (CellFinder.TryRandomClosewalkCellNear(position, map, radius, out var result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
			{
				return result;
			}
		}
		return CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_GetDeathrest obj = (JobGiver_GetDeathrest)base.DeepCopy(resolve);
		obj.maxNeedPercent = maxNeedPercent;
		return obj;
	}
}
