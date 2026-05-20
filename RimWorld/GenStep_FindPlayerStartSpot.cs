using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_FindPlayerStartSpot : GenStep
{
	private const int MinRoomCellCount = 10;

	public override int SeedPart => 1187186631;

	public override void Generate(Map map, GenStepParams parms)
	{
		HashSet<IntVec3> largestOpenArea;
		List<CellRect> usedRects;
		if (!map.wasSpawnedViaGravShipLanding)
		{
			DeepProfiler.Start("RebuildAllRegions");
			map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			if (!MapGenerator.PlayerStartSpotValid)
			{
				largestOpenArea = FindLargestContiguousOpenArea(map);
				usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
				MapGenerator.PlayerStartSpot = CellFinderLoose.TryFindCentralCell(map, 7, 10, Validator);
			}
		}
		bool Validator(IntVec3 cell)
		{
			if (!largestOpenArea.Contains(cell))
			{
				return false;
			}
			foreach (LayoutStructureSketch layoutStructureSketch in map.layoutStructureSketches)
			{
				if (layoutStructureSketch.structureLayout != null && layoutStructureSketch.structureLayout.container.Contains(cell))
				{
					return false;
				}
			}
			foreach (CellRect item in usedRects)
			{
				if (item.Contains(cell))
				{
					return false;
				}
			}
			if (!cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			return !cell.Roofed(map);
		}
	}

	private HashSet<IntVec3> FindLargestContiguousOpenArea(Map map)
	{
		HashSet<IntVec3> checkedCells = new HashSet<IntVec3>();
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		HashSet<IntVec3> current = new HashSet<IntVec3>();
		_ = MapGenerator.Elevation;
		foreach (IntVec3 cell in map.AllCells)
		{
			if (cell.GetEdifice(map) == null && !checkedCells.Contains(cell))
			{
				current.Clear();
				map.floodFiller.FloodFill(cell, (IntVec3 x) => cell.GetEdifice(map) == null, delegate(IntVec3 x)
				{
					current.Add(x);
					checkedCells.Add(x);
				});
				if (current.Count > hashSet.Count)
				{
					hashSet.Clear();
					hashSet.AddRange(current);
				}
			}
		}
		return hashSet;
	}
}
