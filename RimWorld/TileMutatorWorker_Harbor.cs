using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_Harbor : TileMutatorWorker
{
	private const int OutpostSize = 16;

	private const int MaxDistFromCenter = 75;

	private static readonly IntRange MainDockSectionLengthRange = new IntRange(19, 27);

	private static readonly IntRange DockSectionLengthRange = new IntRange(8, 11);

	private static readonly IntRange BranchDistRange = new IntRange(9, 11);

	private Rot4 waterDir;

	public TileMutatorWorker_Harbor(TileMutatorDef def)
		: base(def)
	{
	}

	public override bool IsValidTile(PlanetTile tile, PlanetLayer layer)
	{
		if (!Find.FactionManager.GetFactions().Any())
		{
			return false;
		}
		return base.IsValidTile(tile, layer);
	}

	public override void GenerateCriticalStructures(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		if (!MapGenUtility.TryGetClosestClearRectTo(out var rect, new IntVec2(16, 16), map.Center, (CellRect r) => !r.Cells.Any((IntVec3 c) => c.GetTerrain(map).IsWater) && !usedRects.Any((CellRect ur) => ur.Overlaps(r) && ur.CenterCell.InHorDistOf(map.Center, 75f))))
		{
			if (!CellFinder.TryFindRandomCellNear(map.Center, map, 75, (IntVec3 c) => !c.GetTerrain(map).IsWater && c.GetEdifice(map) == null, out var result))
			{
				Log.Error("Failed to find location for harbor");
				return;
			}
			rect = CellRect.CenteredOn(result, 8);
		}
		Faction result2;
		if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
		{
			Find.FactionManager.GetFactions().TryRandomElement(out result2);
		}
		else
		{
			result2 = map.ParentFaction;
		}
		if (result2 == null)
		{
			return;
		}
		MapGenUtility.PostProcessSettlementParams postProcessSettlementParams = new MapGenUtility.PostProcessSettlementParams
		{
			clearBuildingFaction = true,
			faction = result2,
			damageBuildings = true,
			canDamageWalls = false,
			noFuel = true,
			ageCorpses = true
		};
		GenStep_Outpost genStep_Outpost = new GenStep_Outpost();
		genStep_Outpost.size = 16;
		genStep_Outpost.forcedRect = rect;
		genStep_Outpost.overrideFaction = result2;
		genStep_Outpost.postProcessSettlementParams = postProcessSettlementParams;
		genStep_Outpost.settlementDontGeneratePawns = true;
		genStep_Outpost.generateLoot = false;
		genStep_Outpost.Generate(map, default(GenStepParams));
		List<IntVec3> list = new List<IntVec3>();
		IntVec3 centerCell = rect.CenterCell;
		waterDir = Rot4.FromAngleFlat((map.Center - centerCell).AngleFlat);
		IntVec3 intVec;
		int num2;
		if (waterDir == Rot4.North || waterDir == Rot4.South)
		{
			int num = centerCell.x - map.Center.x;
			intVec = ((num > 0) ? Rot4.West.AsIntVec3 : Rot4.East.AsIntVec3);
			num2 = Mathf.Abs(num);
		}
		else
		{
			int num3 = centerCell.z - map.Center.z;
			intVec = ((num3 > 0) ? Rot4.South.AsIntVec3 : Rot4.North.AsIntVec3);
			num2 = Mathf.Abs(num3);
		}
		IntVec3 intVec2 = centerCell;
		for (int num4 = 0; num4 < num2; num4++)
		{
			list.Add(intVec2);
			intVec2 += intVec;
		}
		int num5 = 99;
		while (!intVec2.GetTerrain(map).IsOcean || num5-- < 0)
		{
			list.Add(intVec2);
			intVec2 += waterDir.AsIntVec3;
		}
		ExtendDock(intVec2, map, list, MainDockSectionLengthRange.RandomInRange, waterDir, 1f, mainBranch: true);
		list = FindDockStartPoint(list, rect);
		foreach (IntVec3 item in list)
		{
			SetDockAround(map, item);
		}
	}

	private List<IntVec3> FindDockStartPoint(List<IntVec3> path, CellRect outpostRect)
	{
		path.Reverse();
		for (int i = 0; i < path.Count; i++)
		{
			IntVec3 intVec = path[i];
			IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
			foreach (IntVec3 intVec2 in adjacentCellsAndInside)
			{
				if (outpostRect.Contains(intVec + intVec2))
				{
					path = path.GetRange(0, i);
					path.Reverse();
					return path;
				}
			}
		}
		path.Reverse();
		return path;
	}

	private bool CheckCollision(IntVec3 checkCell, Rot4 dir, Map map, List<IntVec3> path)
	{
		if (!ValidateCell(checkCell, dir))
		{
			return true;
		}
		if (!ValidateCell(checkCell, dir.Rotated(RotationDirection.Clockwise)) || !ValidateCell(checkCell, dir.Rotated(RotationDirection.Counterclockwise)))
		{
			return true;
		}
		return false;
		bool ValidateCell(IntVec3 cell, Rot4 rot)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = cell + rot.AsIntVec3 * i;
				if (!intVec.GetTerrain(map).IsOcean || path.Contains(intVec))
				{
					return false;
				}
			}
			return true;
		}
	}

	private void ExtendDock(IntVec3 startCell, Map map, List<IntVec3> path, int length, Rot4 direction, float branchChance, bool mainBranch)
	{
		int num = BranchDistRange.RandomInRange;
		for (int i = 0; i < length; i++)
		{
			IntVec3 intVec = startCell + direction.AsIntVec3 * i;
			if (!intVec.InBounds(map))
			{
				continue;
			}
			if (CheckCollision(intVec, direction, map, path))
			{
				if (mainBranch && i > 10)
				{
					break;
				}
				if (!mainBranch && i > 0 && CheckCollision(intVec, direction, map, path))
				{
					for (int num2 = i; num2 > 1; num2--)
					{
						path.Pop();
					}
					break;
				}
			}
			path.Add(intVec);
			num--;
			if (num != 0)
			{
				continue;
			}
			num = BranchDistRange.RandomInRange;
			if (Rand.Chance(branchChance))
			{
				Rot4 rot;
				do
				{
					rot = direction.Rotated(Rand.Bool ? RotationDirection.Clockwise : RotationDirection.Counterclockwise);
				}
				while (rot == waterDir.Opposite);
				ExtendDock(intVec, map, path, DockSectionLengthRange.RandomInRange, rot, branchChance - 0.7f, mainBranch: false);
				if (mainBranch && Rand.Chance(0.75f))
				{
					ExtendDock(intVec, map, path, DockSectionLengthRange.RandomInRange, rot.Opposite, branchChance - 0.7f, mainBranch: false);
				}
				else if (Rand.Bool)
				{
					path.Add(intVec + rot.Opposite.AsIntVec3);
					path.Add(intVec + rot.Opposite.AsIntVec3 * 2);
				}
			}
		}
	}

	private static void SetDockAround(Map map, IntVec3 cell)
	{
		IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
		foreach (IntVec3 intVec in adjacentCellsAndInside)
		{
			IntVec3 c = cell + intVec;
			if (!c.InBounds(map))
			{
				continue;
			}
			TerrainDef terrain = c.GetTerrain(map);
			if (terrain.IsFloor)
			{
				continue;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice != null)
			{
				if (edifice.def.building.isNaturalRock)
				{
					edifice.Destroy();
					map.roofGrid.SetRoof(c, null);
				}
				else if (edifice.def.fillPercent >= 0.99f)
				{
					continue;
				}
			}
			map.terrainGrid.SetTerrain(c, terrain.IsWater ? TerrainDefOf.Bridge : TerrainDefOf.PackedDirt);
		}
	}
}
