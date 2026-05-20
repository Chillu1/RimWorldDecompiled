using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Fish : WorkGiver_Scanner
{
	public const int TriesToFindSpotThatIsntInWater = 60;

	public override Job NonScanJob(Pawn pawn)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		if (pawn.Ideo != null && !new HistoryEvent(HistoryEventDefOf.SlaughteredFish, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return null;
		}
		if (pawn.CanReserve(pawn.Position, 1, -1, ReservationLayerDefOf.Floor) && !pawn.Position.GetTerrain(pawn.Map).IsWater)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = pawn.Position + GenAdj.CardinalDirections[(i + pawn.thingIDNumber) % 4];
				if (intVec.InBounds(pawn.Map) && intVec.GetZone(pawn.Map) is Zone_Fishing { ShouldFishNow: not false } zone_Fishing && zone_Fishing.IsFishable(intVec) && !intVec.IsForbidden(pawn) && pawn.CanReserve(intVec, 1, -1, ReservationLayerDefOf.Floor))
				{
					return JobMaker.MakeJob(JobDefOf.Fish, intVec, pawn.Position);
				}
			}
		}
		Zone_Fishing zone_Fishing2 = null;
		float num = float.MaxValue;
		foreach (Zone allZone in pawn.Map.zoneManager.AllZones)
		{
			if (allZone is Zone_Fishing { ShouldFishNow: not false, HasAnyFishableCells: not false } zone_Fishing3 && (zone_Fishing2 == null || (float)pawn.Position.DistanceToSquared(zone_Fishing3.Cells[0]) < num))
			{
				zone_Fishing2 = zone_Fishing3;
				num = pawn.Position.DistanceToSquared(zone_Fishing3.Cells[0]);
			}
		}
		if (zone_Fishing2 != null)
		{
			int num2 = 0;
			bool flag = false;
			while (num2 < 60)
			{
				IntVec3 randomFishableCell = zone_Fishing2.RandomFishableCell;
				if (randomFishableCell.IsValid && pawn.CanReserveAndReach(randomFishableCell, PathEndMode.Touch, Danger.Some, 1, -1, ReservationLayerDefOf.Floor))
				{
					IntVec3 intVec2 = BestStandSpotFor(pawn, randomFishableCell);
					if (intVec2.IsValid && (flag || !intVec2.GetTerrain(pawn.Map).IsWater))
					{
						return JobMaker.MakeJob(JobDefOf.Fish, randomFishableCell, intVec2);
					}
				}
				num2++;
				if (num2 >= 60 && !flag)
				{
					flag = true;
					num2 = 0;
				}
			}
		}
		return null;
	}

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		if (pawn.Ideo != null && !new HistoryEvent(HistoryEventDefOf.SlaughteredFish, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		return base.HasJobOnCell(pawn, c, forced);
	}

	public static IntVec3 BestStandSpotFor(Pawn pawn, IntVec3 fishSpot, bool avoidStandingInWater = true)
	{
		IntVec3 result = IntVec3.Invalid;
		float num = float.MinValue;
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = fishSpot + GenAdj.CardinalDirections[i];
			if (intVec.InBounds(pawn.Map) && intVec.Standable(pawn.Map) && !intVec.IsForbidden(pawn) && pawn.CanReserveAndReach(intVec, PathEndMode.Touch, Danger.Some, 1, -1, ReservationLayerDefOf.Floor))
			{
				if (!avoidStandingInWater)
				{
					return intVec;
				}
				float num2 = (intVec.GetTerrain(pawn.Map).avoidWander ? 0.5f : 1f);
				if (num2 > num)
				{
					num = num2;
					result = intVec;
				}
				else if (Mathf.Approximately(num2, num) && Rand.Bool)
				{
					result = intVec;
				}
			}
		}
		return result;
	}
}
