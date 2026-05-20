using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class VacuumUtility
{
	private const float SeverityPerSecond = 0.02f;

	public const float MinVacuumForDamage = 0.5f;

	public const float ApparelMinResistAvoidDamage = 0.1f;

	private static readonly List<BodyPartRecord> tmpParts = new List<BodyPartRecord>();

	private static Dictionary<Room, bool> tmpRoomOpenToOutside = new Dictionary<Room, bool>();

	public static void PawnVacuumTickInterval(Pawn pawn, int delta)
	{
		if (!ModsConfig.OdysseyActive || !pawn.IsHashIntervalTick(60, delta) || !pawn.Spawned || !pawn.Map.Biome.inVacuum || !pawn.HarmedByVacuum)
		{
			return;
		}
		float vacuum = pawn.Position.GetVacuum(pawn.Map);
		if (!(vacuum < 0.5f))
		{
			float num = 0.02f * vacuum * Mathf.Max(1f - pawn.GetStatValue(StatDefOf.VacuumResistance), 0f);
			if (num > 0f)
			{
				HealthUtility.AdjustSeverity(pawn, HediffDefOf.VacuumExposure, num);
			}
		}
	}

	public static bool CanBeVacuumBurnt(Pawn pawn)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (!pawn.def.race.canBeVacuumBurnt)
		{
			return false;
		}
		if (pawn.health.hediffSet.PreventVacuumBurns)
		{
			return false;
		}
		if (pawn.genes != null)
		{
			foreach (Gene item in pawn.genes.GenesListForReading)
			{
				if (item.Active && item.def.immuneToVacuumBurns)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool TryGetVacuumBurnablePart(Pawn pawn, out BodyPartRecord p)
	{
		if (!CanBeVacuumBurnt(pawn))
		{
			p = null;
			return false;
		}
		tmpParts.AddRange(pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside));
		for (int num = tmpParts.Count - 1; num >= 0; num--)
		{
			BodyPartRecord bodyPartRecord = tmpParts[num];
			bool flag = false;
			if (!bodyPartRecord.def.canBeVacuumBurnt || !bodyPartRecord.def.IsSkinCovered(bodyPartRecord, pawn.health.hediffSet))
			{
				tmpParts.RemoveAt(num);
			}
			else
			{
				if (pawn.apparel != null)
				{
					for (int i = 0; i < bodyPartRecord.groups.Count; i++)
					{
						if (pawn.apparel.BodyPartGroupIsCovered(bodyPartRecord.groups[i], (Apparel ap) => IsProtectiveApparel(ap.def)))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						flag = pawn.apparel.TryGetFirstApparelOnBodyPart(bodyPartRecord, out var _, (Apparel ap) => IsProtectiveApparel(ap.def));
					}
				}
				if (flag)
				{
					tmpParts.RemoveAt(num);
				}
			}
		}
		if (tmpParts.Empty())
		{
			p = null;
			return false;
		}
		tmpParts.TryRandomElementByWeight((BodyPartRecord w) => w.coverage, out p);
		tmpParts.Clear();
		return p != null;
	}

	public static bool IsProtectiveApparel(ThingDef thing)
	{
		return thing.equippedStatOffsets.GetStatOffsetFromList(StatDefOf.VacuumResistance) >= 0.1f;
	}

	private static bool EverInVacuum(IntVec3 cell, Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (map == null)
		{
			return false;
		}
		if (!cell.InBounds(map) || !map.Biome.inVacuum)
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice == null)
		{
			return true;
		}
		if (edifice.def.passability == Traversability.Impassable && edifice.def.Fillage == FillCategory.Full)
		{
			return false;
		}
		if (edifice.def.building.canExchangeVacuum || edifice.def.building.alwaysExchangeVacuum)
		{
			return true;
		}
		if (edifice.ExchangeVacuum)
		{
			return true;
		}
		return edifice.def.Fillage != FillCategory.Full;
	}

	public static bool VacuumConcernTo(this Thing thing, Pawn pawn)
	{
		if (!pawn.ConcernedByVacuum)
		{
			return false;
		}
		return thing.PositionHeld.GetVacuum(pawn.MapHeld) >= 0.5f;
	}

	public static bool VacuumConcernTo(this IntVec3 cell, Pawn pawn)
	{
		if (!pawn.ConcernedByVacuum)
		{
			return false;
		}
		return cell.GetVacuum(pawn.MapHeld) >= 0.5f;
	}

	public static float GetVacuum(this IntVec3 cell, Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return 0f;
		}
		if (!EverInVacuum(cell, map))
		{
			return 0f;
		}
		return cell.GetRoom(map)?.Vacuum ?? 1f;
	}

	public static bool IsRoomAirtight(Room room)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		tmpRoomOpenToOutside.Clear();
		if (IsRoomDirectlyOpenToOutside(room))
		{
			return false;
		}
		foreach (IntVec3 borderCell in room.BorderCells)
		{
			Building edifice = borderCell.GetEdifice(room.Map);
			if (edifice == null || (edifice != null && edifice.IsAirtight && !edifice.def.building.alwaysExchangeVacuum))
			{
				continue;
			}
			CellRect cellRect = edifice.OccupiedRect();
			foreach (IntVec3 item in cellRect)
			{
				for (int i = 0; i < 4; i++)
				{
					IntVec3 intVec = item + GenAdj.CardinalDirections[i];
					if (!cellRect.Contains(intVec))
					{
						Room room2 = intVec.GetRoom(room.Map);
						if (room2 != null && room2 != room && IsRoomDirectlyOpenToOutside(room2))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
		static bool IsRoomDirectlyOpenToOutside(Room room3)
		{
			if (tmpRoomOpenToOutside.TryGetValue(room3, out var value))
			{
				return value;
			}
			if (room3.TouchesMapEdge || room3.OpenRoofCount > 0)
			{
				tmpRoomOpenToOutside[room3] = true;
				return true;
			}
			foreach (IntVec3 cell in room3.Cells)
			{
				TerrainDef terrainDef = room3.Map.terrainGrid.FoundationAt(cell);
				if (terrainDef == null || !terrainDef.IsSubstructure)
				{
					tmpRoomOpenToOutside[room3] = true;
					return true;
				}
			}
			tmpRoomOpenToOutside[room3] = false;
			return false;
		}
	}
}
