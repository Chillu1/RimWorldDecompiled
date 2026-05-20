using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_OrbitalMechhive : GenStep_LargeRuins
{
	private const int NumStabilizers = 3;

	private static readonly IntRange NumStandaloneRooms = new IntRange(10, 12);

	private static readonly IntRange StandaloneRoomSize = new IntRange(8, 25);

	private static readonly IntRange NumLaunchPads = new IntRange(8, 10);

	private static readonly IntRange NumProcessorArrays = new IntRange(8, 12);

	private static readonly IntRange NumGuardOutposts = new IntRange(10, 12);

	private static readonly IntRange GuardOutpostPointsRange = new IntRange(200, 350);

	private const int MapBorderPadding = 20;

	private const int PlatformRegionSize = 22;

	private static readonly IntRange PlatformPadBorderLumpLengthRange = new IntRange(8, 12);

	private static readonly IntRange PlatformPadBorderLumpOffsetRange = new IntRange(-2, 3);

	private static readonly FloatRange DamagedMaxPercentageRange = new FloatRange(0.5f, 0.5f);

	private int stabilizerCount;

	private static Chunks chunks;

	private static List<IntVec3> tmpHoleCells = new List<IntVec3>();

	protected override int RegionSize => 65;

	protected override FloatRange DefaultMapFillPercentRange { get; } = new FloatRange(0.5f, 0.6f);

	protected override FloatRange MergeRange { get; } = new FloatRange(1f, 1f);

	protected override IntRange RuinsMinMaxRange { get; } = new IntRange(3, 4);

	protected override int MoveRangeLimit => 6;

	protected override int ContractLimit => 6;

	protected override int MinRegionSize => 40;

	protected override bool UseUsedRects => true;

	protected override LayoutDef LayoutDef => LayoutDefOf.Mechhive;

	protected override Faction Faction => Faction.OfMechanoids;

	public override int SeedPart => 345879234;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckOdyssey("Orbital Mechhive"))
		{
			return;
		}
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		stabilizerCount = 0;
		chunks = new Chunks(map.BoundsRect(20), 22);
		using (ProfilerBlock.Scope("CreatePlatform"))
		{
			try
			{
				CreatePlatform(map);
			}
			catch (Exception ex)
			{
				Log.Error("Error in CreatePlatform " + ex);
			}
		}
		using (ProfilerBlock.Scope("GenerateCerebrexCore"))
		{
			try
			{
				GenerateCerebrexCore(map, orGenerateVar);
			}
			catch (Exception ex2)
			{
				Log.Error("Error in GenerateCerebrexCore " + ex2);
			}
		}
		using (ProfilerBlock.Scope("SetupPlayerStartSpot"))
		{
			try
			{
				SetupPlayerStartSpot(map, parms, orGenerateVar);
			}
			catch (Exception ex3)
			{
				Log.Error("Error in SetupPlayerStartSpot " + ex3);
			}
		}
		using (ProfilerBlock.Scope("GenerateGaussCannons"))
		{
			try
			{
				GenerateCannons(map, orGenerateVar);
			}
			catch (Exception ex4)
			{
				Log.Error("Error in GenerateGaussCannons " + ex4);
			}
		}
		using (ProfilerBlock.Scope("GenerateRuins"))
		{
			try
			{
				GenerateRuins(map, parms, DefaultMapFillPercentRange);
			}
			catch (Exception ex5)
			{
				Log.Error("Error in GenerateRuins " + ex5);
			}
		}
		using (ProfilerBlock.Scope("GenerateStandaloneRooms"))
		{
			try
			{
				GenerateStandaloneRooms(map, parms, orGenerateVar);
			}
			catch (Exception ex6)
			{
				Log.Error("Error in GenerateStandaloneRooms " + ex6);
			}
		}
		using (ProfilerBlock.Scope("CreateHoles"))
		{
			try
			{
				CreateHoles(map, orGenerateVar);
			}
			catch (Exception ex7)
			{
				Log.Error("Error in CreateHoles " + ex7);
			}
		}
		using (ProfilerBlock.Scope("GenerateMiniStructures"))
		{
			try
			{
				GenerateMiniStructures(map, orGenerateVar);
			}
			catch (Exception ex8)
			{
				Log.Error("Error in GenerateMiniStructures " + ex8);
			}
		}
		map.FogOfWarColor = ColorLibrary.SpaceMechFog;
		map.OrbitalDebris = OrbitalDebrisDefOf.Mechanoid;
	}

	protected override LayoutStructureSketch GenerateAndSpawn(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
	{
		GeneratePlatformBorder(rect, map);
		return base.GenerateAndSpawn(rect, map, parms, layoutDef);
	}

	private void GeneratePlatformBorder(CellRect rect, Map map)
	{
		foreach (IntVec3 edgeCell in rect.ExpandedBy(1).EdgeCells)
		{
			if (edgeCell.GetTerrain(map) == TerrainDefOf.Space)
			{
				map.terrainGrid.SetTerrain(edgeCell, TerrainDefOf.MechanoidPlatform);
			}
		}
	}

	private static void CreatePlatform(Map map)
	{
		using (map.pathing.DisableIncrementalScope())
		{
			SpaceGenUtility.ChunksSetSparsley(chunks, FloatRange.One);
			foreach (CellRect enumeratedRect in chunks.EnumeratedRects)
			{
				foreach (IntVec3 cell in enumeratedRect.Cells)
				{
					map.terrainGrid.SetTerrain(cell, TerrainDefOf.MechanoidPlatform);
				}
				CellRect rect = enumeratedRect;
				MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.MechanoidPlatform, ref rect, PlatformPadBorderLumpLengthRange, PlatformPadBorderLumpOffsetRange);
			}
		}
	}

	private void GenerateCerebrexCore(Map map, List<CellRect> usedRects)
	{
		RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			sketch = new Sketch()
		}, root: SketchResolverDefOf.CerebrexCore).Spawn(map, map.Center, Faction.OfMechanoids, Sketch.SpawnPosType.OccupiedCenter, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: true, clearEdificeWhereFloor: true);
		CellRect cellRect = CellRect.CenteredOn(map.Center, Mathf.FloorToInt(30.5f));
		foreach (IntVec3 cell in cellRect.Cells)
		{
			map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
		}
		cellRect = cellRect.ExpandedBy(4);
		usedRects.Add(cellRect);
	}

	private void SetupPlayerStartSpot(Map map, GenStepParams parms, List<CellRect> usedRects)
	{
		if (parms.gravship == null)
		{
			return;
		}
		HashSet<IntVec3> cellsAdjacentToSubstructure = GravshipPlacementUtility.GetCellsAdjacentToSubstructure(parms.gravship.OccupiedRects, 2);
		bool enabled = map.regionAndRoomUpdater.Enabled;
		map.regionAndRoomUpdater.Enabled = true;
		GenStep_ReserveGravshipArea.SetStartSpot(map, cellsAdjacentToSubstructure, usedRects);
		map.regionAndRoomUpdater.Enabled = enabled;
		IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
		foreach (CellRect occupiedRect in parms.gravship.OccupiedRects)
		{
			usedRects.Add(occupiedRect.MovedBy(playerStartSpot).ExpandedBy(2));
		}
		foreach (IntVec3 corner in parms.gravship.Bounds.MovedBy(playerStartSpot).ContractedBy(2).Corners)
		{
			GenSpawn.Spawn(ThingDefOf.AncientShipBeacon, corner, map);
		}
		MapGenerator.SetVar("DontGenerateClearedGravShipTerrain", var: true);
	}

	private void GenerateCannons(Map map, List<CellRect> usedRects)
	{
		CellRect boundary = chunks.boundary;
		List<IntVec3> list = boundary.Corners.ToList();
		for (int i = 0; i < 4; i++)
		{
			CellRect item = CellRect.CenteredOn(list[i], 13, 13);
			foreach (IntVec3 cell in item.Cells)
			{
				if (cell.InHorDistOf(list[i], 5.9f))
				{
					map.terrainGrid.SetTerrain(cell, TerrainDefOf.AncientTile);
				}
				else if (cell.InHorDistOf(list[i], 8f))
				{
					map.terrainGrid.SetTerrain(cell, TerrainDefOf.MechanoidPlatform);
				}
			}
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AnticraftBeam), list[i], map, new Rot4((i + 2) % 4));
			usedRects.Add(item);
		}
	}

	private void GenerateStandaloneRooms(Map map, GenStepParams parms, List<CellRect> usedRects)
	{
		int randomInRange = NumStandaloneRooms.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			CellRect rect = CellRect.Empty;
			if (TryGetFreeRect(map, StandaloneRoomSize.RandomInRange, StandaloneRoomSize.RandomInRange, usedRects, out rect))
			{
				GeneratePlatformBorder(rect, map);
				StructureGenParams parms2 = new StructureGenParams
				{
					size = rect.Size
				};
				LayoutWorker worker = LayoutDefOf.Mechhive_SingleRoom.Worker;
				LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms2);
				map.layoutStructureSketches.Add(layoutStructureSketch);
				structureSketches.Add(layoutStructureSketch);
				float? threatPoints = null;
				if (parms.sitePart != null)
				{
					threatPoints = parms.sitePart.parms.points;
				}
				worker.Spawn(layoutStructureSketch, map, rect.Min, threatPoints, null, roofs: true, canReuseSketch: false, Faction.OfMechanoids);
				usedRects.Add(rect.ExpandedBy(1));
			}
		}
	}

	private static void CreateHoles(Map map, List<CellRect> usedRects)
	{
		int cells = chunks.ApproximateArea;
		int maximum = Mathf.RoundToInt((float)cells * DamagedMaxPercentageRange.RandomInRange);
		List<CellRect> list = new List<CellRect>();
		Chunks obj = chunks;
		List<CellRect> spaces = list;
		SpaceGenUtility.DamageHoles(obj, map, maximum, ref cells, null, null, 1f, (CellRect r) => !usedRects.Any((CellRect ur) => ur.Overlaps(r)), spaces);
		foreach (CellRect item in list)
		{
			usedRects.Add(item);
		}
	}

	private void GenerateMiniStructures(Map map, List<CellRect> usedRects)
	{
		for (int i = 0; i < NumLaunchPads.RandomInRange; i++)
		{
			if (TryGetFreeRect(map, 5, 5, usedRects, out var rect))
			{
				GenerateSketch(SketchResolverDefOf.MechhiveLaunchPad, map, rect);
				usedRects.Add(rect);
			}
		}
		for (int j = 0; j < NumGuardOutposts.RandomInRange; j++)
		{
			if (!TryGetFreeRect(map, 5, 5, usedRects, out var rect2))
			{
				continue;
			}
			GenerateSketch(SketchResolverDefOf.MechhiveGuardOutpost, map, rect2);
			usedRects.Add(rect2);
			IEnumerable<Pawn> enumerable = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = Faction.OfMechanoids,
				points = GuardOutpostPointsRange.RandomInRange
			});
			if (!enumerable.Any())
			{
				continue;
			}
			Lord lord = LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendPoint(rect2.CenterCell), map);
			foreach (Pawn item in enumerable)
			{
				GenSpawn.Spawn(item, CellFinder.RandomClosewalkCellNear(rect2.CenterCell, map, 5), map);
				lord.AddPawn(item);
			}
		}
		for (int k = 0; k < NumProcessorArrays.RandomInRange; k++)
		{
			int width = Rand.RangeInclusive(2, 3) * (ThingDefOf.MechanoidProcessor.size.x + 1) + 1;
			int height = Rand.RangeInclusive(2, 3) * (ThingDefOf.MechanoidProcessor.size.z + 1) + 1;
			if (TryGetFreeRect(map, width, height, usedRects, out var rect3))
			{
				GenerateSketch(SketchResolverDefOf.MechhiveProcessorArray, map, rect3);
				usedRects.Add(rect3);
			}
		}
	}

	private void GenerateSketch(SketchResolverDef sketchDef, Map map, CellRect rect)
	{
		RimWorld.SketchGen.SketchGen.Generate(sketchDef, new SketchResolveParams
		{
			sketch = new Sketch(),
			rect = new CellRect(0, 0, rect.Width, rect.Height)
		}).Spawn(map, rect.CenterCell, Faction.OfMechanoids, Sketch.SpawnPosType.OccupiedCenter, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: true, clearEdificeWhereFloor: true);
	}

	private bool TryGetFreeRect(Map map, int width, int height, List<CellRect> usedRects, out CellRect rect)
	{
		rect = CellRect.Empty;
		if (!CellFinder.TryFindRandomCell(map, delegate(IntVec3 c)
		{
			CellRect tmpRect = CellRect.CenteredOn(c, width, height);
			return tmpRect.InBounds(map) && usedRects.All((CellRect ur) => !ur.Overlaps(tmpRect)) && tmpRect.Cells.All((IntVec3 cell) => cell.GetTerrain(map) == TerrainDefOf.MechanoidPlatform);
		}, out var result))
		{
			return false;
		}
		rect = CellRect.CenteredOn(result, width, height);
		return true;
	}

	protected override StructureGenParams GetStructureGenParams(CellRect rect, Map map, GenStepParams parms, LayoutDef layoutDef)
	{
		StructureGenParams structureGenParams = base.GetStructureGenParams(rect, map, parms, layoutDef);
		structureGenParams.spawnImportantRoom = stabilizerCount < 3;
		stabilizerCount++;
		return structureGenParams;
	}
}
