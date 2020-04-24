using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatheringsUtility
	{
		private const float GatherAreaRadiusIfNotWholeRoom = 10f;

		private const int MaxRoomCellsCountToUseWholeRoom = 324;

		public static bool ShouldGuestKeepAttendingGathering(Pawn p)
		{
			if (p.Downed)
			{
				return false;
			}
			if (p.needs != null && p.needs.food.Starving)
			{
				return false;
			}
			if (p.health.hediffSet.BleedRateTotal > 0f)
			{
				return false;
			}
			if (p.needs.rest != null && (int)p.needs.rest.CurCategory >= 3)
			{
				return false;
			}
			if (p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff())
			{
				return false;
			}
			if (!p.Awake())
			{
				return false;
			}
			if (p.InAggroMentalState)
			{
				return false;
			}
			if (p.IsPrisoner)
			{
				return false;
			}
			return true;
		}

		public static bool PawnCanStartOrContinueGathering(Pawn pawn)
		{
			if (pawn.Drafted)
			{
				return false;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0.3f)
			{
				return false;
			}
			if (pawn.IsPrisoner)
			{
				return false;
			}
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.2f)
			{
				return false;
			}
			if (pawn.IsWildMan())
			{
				return false;
			}
			if (pawn.Spawned && !pawn.Downed)
			{
				return !pawn.InMentalState;
			}
			return false;
		}

		public static bool AnyLordJobPreventsNewGatherings(Map map)
		{
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (!lords[i].LordJob.AllowStartNewGatherings)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AcceptableGameConditionsToStartGathering(Map map, GatheringDef gatheringDef)
		{
			if (!AcceptableGameConditionsToContinueGathering(map))
			{
				return false;
			}
			if (GenLocalDate.HourInteger(map) < 4 || GenLocalDate.HourInteger(map) > 21)
			{
				return false;
			}
			if (AnyLordJobPreventsNewGatherings(map))
			{
				return false;
			}
			if (map.dangerWatcher.DangerRating != 0)
			{
				return false;
			}
			int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
			if (freeColonistsSpawnedCount < 4)
			{
				return false;
			}
			int num = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.health.hediffSet.BleedRateTotal > 0f)
				{
					return false;
				}
				if (item.Drafted)
				{
					num++;
				}
			}
			if ((float)num / (float)freeColonistsSpawnedCount >= 0.5f)
			{
				return false;
			}
			if (!EnoughPotentialGuestsToStartGathering(map, gatheringDef))
			{
				return false;
			}
			return true;
		}

		public static bool AcceptableGameConditionsToContinueGathering(Map map)
		{
			if (map.dangerWatcher.DangerRating == StoryDanger.High)
			{
				return false;
			}
			return true;
		}

		public static bool ValidateGatheringSpot(IntVec3 cell, GatheringDef gatheringDef, Pawn organizer, bool enjoyableOutside)
		{
			Map map = organizer.Map;
			if (!cell.Standable(map))
			{
				return false;
			}
			if (cell.GetDangerFor(organizer, map) != Danger.None)
			{
				return false;
			}
			if (!enjoyableOutside && !cell.Roofed(map))
			{
				return false;
			}
			if (cell.IsForbidden(organizer))
			{
				return false;
			}
			if (!organizer.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
			{
				return false;
			}
			bool flag = cell.GetRoom(map)?.isPrisonCell ?? false;
			if (organizer.IsPrisoner != flag)
			{
				return false;
			}
			if (!EnoughPotentialGuestsToStartGathering(map, gatheringDef, cell))
			{
				return false;
			}
			return true;
		}

		public static bool EnoughPotentialGuestsToStartGathering(Map map, GatheringDef gatheringDef, IntVec3? gatherSpot = null)
		{
			int value = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
			value = Mathf.Clamp(value, 2, 10);
			int num = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (ShouldPawnKeepGathering(item, gatheringDef) && (!gatherSpot.HasValue || !gatherSpot.Value.IsForbidden(item)) && (!gatherSpot.HasValue || item.CanReach(gatherSpot.Value, PathEndMode.Touch, Danger.Some)))
				{
					num++;
				}
			}
			return num >= value;
		}

		public static Pawn FindRandomGatheringOrganizer(Faction faction, Map map, GatheringDef gatheringDef)
		{
			Predicate<Pawn> v = (Pawn x) => x.RaceProps.Humanlike && !x.InBed() && !x.InMentalState && x.GetLord() == null && ShouldPawnKeepGathering(x, gatheringDef) && !x.Drafted && (gatheringDef.requiredTitleAny == null || gatheringDef.requiredTitleAny.Count == 0 || (x.royalty != null && x.royalty.AllTitlesInEffectForReading.Any((RoyalTitle t) => gatheringDef.requiredTitleAny.Contains(t.def))));
			if ((from x in map.mapPawns.SpawnedPawnsInFaction(faction)
				where v(x)
				select x).TryRandomElement(out Pawn result))
			{
				return result;
			}
			return null;
		}

		public static bool InGatheringArea(IntVec3 cell, IntVec3 partySpot, Map map)
		{
			if (UseWholeRoomAsGatheringArea(partySpot, map) && cell.GetRoom(map) == partySpot.GetRoom(map))
			{
				return true;
			}
			if (cell.InHorDistOf(partySpot, 10f))
			{
				Building edifice = cell.GetEdifice(map);
				TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None);
				if (edifice != null)
				{
					return map.reachability.CanReach(partySpot, edifice, PathEndMode.ClosestTouch, traverseParams);
				}
				return map.reachability.CanReach(partySpot, cell, PathEndMode.ClosestTouch, traverseParams);
			}
			return false;
		}

		public static bool TryFindRandomCellInGatheringArea(Pawn pawn, out IntVec3 result)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None);
			if (UseWholeRoomAsGatheringArea(cell, pawn.Map))
			{
				return cell.GetRoom(pawn.Map).Cells.Where((IntVec3 x) => validator(x)).TryRandomElement(out result);
			}
			return CellFinder.TryFindRandomReachableCellNear(cell, pawn.Map, 10f, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 x) => validator(x), null, out result, 10);
		}

		public static bool UseWholeRoomAsGatheringArea(IntVec3 partySpot, Map map)
		{
			Room room = partySpot.GetRoom(map);
			if (room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount <= 324)
			{
				return true;
			}
			return false;
		}

		public static bool ShouldPawnKeepGathering(Pawn p, GatheringDef gatheringDef)
		{
			if (gatheringDef.respectTimetable && p.timetable != null && !p.timetable.CurrentAssignment.allowJoy)
			{
				return false;
			}
			if (!ShouldGuestKeepAttendingGathering(p))
			{
				return false;
			}
			return true;
		}
	}
}
