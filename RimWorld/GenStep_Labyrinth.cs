using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_Labyrinth : GenStep
{
	private LayoutStructureSketch structureSketch;

	private const int Border = 2;

	public override int SeedPart => 8767466;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckAnomaly("Labyrinth"))
		{
			return;
		}
		TerrainGrid terrainGrid = map.terrainGrid;
		foreach (IntVec3 allCell in map.AllCells)
		{
			terrainGrid.SetTerrain(allCell, TerrainDefOf.GraySurface);
		}
		CellRect rect = map.BoundsRect();
		FillEdges(rect, map);
		StructureGenParams parms2 = new StructureGenParams
		{
			size = rect.ContractedBy(2).Size
		};
		LayoutWorker worker = LayoutDefOf.Labyrinth.Worker;
		int num = 10;
		do
		{
			structureSketch = worker.GenerateStructureSketch(parms2);
		}
		while (!structureSketch.structureLayout.HasRoomWithDef(LayoutRoomDefOf.LabyrinthObelisk) && num-- > 0);
		if (num == 0)
		{
			Log.ErrorOnce("Failed to generate labyrinth, guard exceeded. Check layout worker for errors placing minimum rooms", 9868797);
			return;
		}
		worker.Spawn(structureSketch, map, new IntVec3(2, 0, 2), null, null, roofs: false);
		map.layoutStructureSketches.Add(structureSketch);
		LabyrinthMapComponent component = map.GetComponent<LabyrinthMapComponent>();
		LayoutRoom firstRoomOfDef = structureSketch.structureLayout.GetFirstRoomOfDef(LayoutRoomDefOf.LabyrinthObelisk);
		List<LayoutRoom> spawnableRooms = GetSpawnableRooms(firstRoomOfDef);
		component.SetSpawnRooms(spawnableRooms);
		MapGenerator.PlayerStartSpot = IntVec3.Zero;
		map.fogGrid.SetAllFogged();
	}

	private List<LayoutRoom> GetSpawnableRooms(LayoutRoom obelisk)
	{
		List<LayoutRoom> list = new List<LayoutRoom>();
		list.AddRange(structureSketch.structureLayout.Rooms);
		list.Remove(obelisk);
		foreach (var logicalRoomConnection in structureSketch.structureLayout.GetLogicalRoomConnections(obelisk))
		{
			LayoutRoom item = logicalRoomConnection.Item1;
			if (!list.Contains(item))
			{
				continue;
			}
			list.Remove(item);
			foreach (var logicalRoomConnection2 in structureSketch.structureLayout.GetLogicalRoomConnections(item))
			{
				LayoutRoom item2 = logicalRoomConnection2.Item1;
				if (list.Contains(item2))
				{
					list.Remove(item2);
				}
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			foreach (LayoutRoomDef def in list[num].defs)
			{
				if (!def.isValidPlayerSpawnRoom)
				{
					list.RemoveAt(num);
					break;
				}
			}
		}
		if (list.Empty())
		{
			list.Clear();
			list.AddRange(structureSketch.structureLayout.Rooms);
			list.Remove(obelisk);
		}
		return list;
	}

	private static void FillEdges(CellRect rect, Map map)
	{
		for (int i = 0; i < rect.Width; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				SpawnWall(new IntVec3(i, 0, j), map);
				SpawnWall(new IntVec3(i, 0, rect.Height - j - 1), map);
			}
		}
		for (int k = 2; k < rect.Height - 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				SpawnWall(new IntVec3(l, 0, k), map);
				SpawnWall(new IntVec3(rect.Width - l - 1, 0, k), map);
			}
		}
	}

	private static void SpawnWall(IntVec3 pos, Map map)
	{
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GrayWall, ThingDefOf.LabyrinthMatter), pos, map);
	}
}
