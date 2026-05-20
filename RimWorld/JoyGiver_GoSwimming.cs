using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_GoSwimming : JoyGiver
{
	private const float MinSwimmingTemperature = 10f;

	public static bool HappyToSwimOutsideOnMap(Map map)
	{
		if (map.mapTemperature.OutdoorTemp < 10f)
		{
			return false;
		}
		if (map.Biome.inVacuum)
		{
			return false;
		}
		if (!map.gameConditionManager.AllowEnjoyableOutsideNow(map))
		{
			return false;
		}
		return true;
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!ModLister.CheckOdyssey("Swimming"))
		{
			return null;
		}
		if (PawnUtility.WillSoonHaveBasicNeed(pawn))
		{
			return null;
		}
		if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), RegionValidator, 100, out var result))
		{
			return null;
		}
		if (!result.TryFindRandomCellInRegionUnforbidden(pawn, CellValidator, out var result2))
		{
			return null;
		}
		if (!SwimPathFinder.TryFindSwimPath(pawn, result2, out var result3))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(def.jobDef, result3[0]);
		job.targetQueueA = new List<LocalTargetInfo>();
		for (int i = 1; i < result3.Count; i++)
		{
			job.targetQueueA.Add(result3[i]);
		}
		job.locomotionUrgency = LocomotionUrgency.Walk;
		return job;
		bool CellValidator(IntVec3 x)
		{
			if (!PawnUtility.KnownDangerAt(x, pawn.Map, pawn) && x.GetTerrain(pawn.Map).IsWater && x.GetTerrain(pawn.Map).toxicBuildupFactor == 0f && !x.Fogged(pawn.Map))
			{
				return x.Standable(pawn.Map);
			}
			return false;
		}
		static bool EnjoyableInsideNow(Room room)
		{
			return room.Temperature > 10f;
		}
		bool RegionValidator(Region x)
		{
			if ((x.Room.PsychologicallyOutdoors ? HappyToSwimOutsideOnMap(pawn.Map) : EnjoyableInsideNow(x.Room)) && !x.IsForbiddenEntirely(pawn) && x.TryFindRandomCellInRegionUnforbidden(pawn, CellValidator, out var _))
			{
				if (pawn.ConcernedByVacuum)
				{
					return x.Room.Vacuum < 0.5f;
				}
				return true;
			}
			return false;
		}
	}
}
