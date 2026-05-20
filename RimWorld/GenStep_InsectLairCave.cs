using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;

namespace RimWorld;

public class GenStep_InsectLairCave : GenStep
{
	private enum ThreatPOIType
	{
		InsectHives,
		InsectCocoons,
		Ambush
	}

	private enum MiscPOIType
	{
		EggSacs,
		BoomShrooms,
		Mushrooms,
		Corpses,
		HermeticCrate
	}

	private const int MinDistBetweenPOIs = 15;

	private const int MinDistFromEdge = 10;

	private const int MinDistFromEntrance = 20;

	private const float SludgeThreshold = 0.2f;

	public const float SlimeChance = 0.2f;

	public const float SacChance = 0.02f;

	private const int BurrowWallSize = 154;

	private static readonly IntRange NumThreatPOIsRange = new IntRange(2, 3);

	private static readonly IntRange NumMiscPOIsRange = new IntRange(3, 5);

	private static readonly IntRange GravlitePanelCount = new IntRange(100, 150);

	private static readonly SimpleCurve HivesFromThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 1f),
		new CurvePoint(800f, 2f),
		new CurvePoint(5000f, 4f),
		new CurvePoint(10000f, 8f)
	};

	private static readonly SimpleCurve CocoonThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 100f),
		new CurvePoint(1000f, 500f),
		new CurvePoint(10000f, 2500f)
	};

	private static readonly SimpleCurve HiveThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 1f),
		new CurvePoint(800f, 2f),
		new CurvePoint(5000f, 7f),
		new CurvePoint(10000f, 10f)
	};

	private static readonly SimpleCurve BossHivesThreatPointsCurve = new SimpleCurve
	{
		new CurvePoint(100f, 1f),
		new CurvePoint(800f, 2f),
		new CurvePoint(5000f, 4f),
		new CurvePoint(10000f, 8f)
	};

	private List<IntVec3> poiCells = new List<IntVec3>();

	private PocketMapExit exit;

	public InsectLairEntrance Entrance => exit.entrance as InsectLairEntrance;

	public override int SeedPart => 98236175;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		poiCells.Clear();
		ModuleBase moduleBase = new Perlin(0.1, 0.9, 1.5, 4, Rand.Int, QualityMode.Medium);
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.GetEdifice(map) == null && GenConstruct.CanBuildOnTerrain(TerrainDefOf.InsectSludge, allCell, map, Rot4.North) && moduleBase.GetValue(allCell) > 0.2f)
			{
				TryPlaceSludge(allCell, map);
			}
		}
		for (int i = 0; i < NumThreatPOIsRange.max + NumMiscPOIsRange.max + 1; i++)
		{
			IntVec3 pOILocation = GetPOILocation(map);
			if (pOILocation.IsValid)
			{
				poiCells.Add(pOILocation);
			}
		}
		GenerateRandomBurrowWalls(map);
		foreach (IntVec3 allCell2 in map.AllCells)
		{
			if (Rand.Chance(0.02f) && allCell2.GetTerrain(map) == TerrainDefOf.InsectSludge)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.EggSac);
				thing.SetFaction(Faction.OfInsects);
				GenPlace.TryPlaceThing(thing, allCell2, map, ThingPlaceMode.Near);
			}
		}
		exit = map.listerThings.ThingsOfDef(ThingDefOf.CaveExit).FirstOrDefault() as PocketMapExit;
		if (exit == null)
		{
			Log.Error("No cave entrance found for Insect Lair POI generation.");
		}
		GenerateBossPOI(map);
		GenerateThreatPOIs(map);
		GenerateMiscPOIs(map);
		Map target = (map.Parent as PocketMapParent)?.sourceMap ?? map;
		GenerateRandomHives(map, Mathf.RoundToInt(HiveThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target))));
	}

	private static void GenerateRandomBurrowWalls(Map map)
	{
		Perlin perlin = new Perlin(0.08, 2.0, 2.0, 2, Rand.Int, QualityMode.Medium);
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = perlin.GetValue(allCell);
			if (allCell.GetFirstBuilding(map) == null && value > 0.7f && GenSpawn.CanSpawnAt(ThingDefOf.BurrowWall, allCell, map))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.BurrowWall);
				if (GenPlace.TryPlaceThing(thing, allCell, map, ThingPlaceMode.Direct))
				{
					thing.SetFaction(Faction.OfInsects);
				}
			}
		}
	}

	private void GenerateBossPOI(Map map)
	{
		IntVec3 intVec = poiCells.OrderBy((IntVec3 c) => -c.DistanceToSquared(exit?.Position ?? map.Center)).First();
		poiCells.Remove(intVec);
		ClearArea(intVec, map);
		GeneratePOIWalls(intVec, map, out var shape);
		GenerateBossRoom(intVec, map);
		string signalTag = "queenApproached-" + Find.UniqueIDsManager.GetNextSignalTagID();
		CellRect rect = CellRect.FromCellList(shape).ExpandedBy(2).ClipInsideMap(map);
		RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
		obj.signalTag = signalTag;
		obj.Rect = rect;
		obj.destroyIfUnfogged = true;
		GenSpawn.Spawn(obj, rect.CenterCell, map);
		SignalAction_Letter obj2 = (SignalAction_Letter)ThingMaker.MakeThing(ThingDefOf.SignalAction_Letter);
		obj2.signalTag = signalTag;
		obj2.letterDef = LetterDefOf.ThreatBig;
		obj2.letterLabelKey = "LetterLabelInsectQueenWarning";
		obj2.letterMessageKey = "LetterInsectQueenWarning";
		GenSpawn.Spawn(obj2, rect.CenterCell, map);
	}

	private void GenerateThreatPOIs(Map map)
	{
		int randomInRange = NumThreatPOIsRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			ThreatPOIType threatPOIType = Rand.EnumValue<ThreatPOIType>();
			if (!poiCells.Empty())
			{
				IntVec3 intVec = poiCells.RandomElement();
				poiCells.Remove(intVec);
				ClearArea(intVec, map);
				GeneratePOIWalls(intVec, map, out var _);
				switch (threatPOIType)
				{
				case ThreatPOIType.InsectHives:
					GenerateInsectHivePOI(intVec, map);
					break;
				case ThreatPOIType.InsectCocoons:
					GenerateInsectCocoonPOI(intVec, map);
					break;
				case ThreatPOIType.Ambush:
					GenerateAmbushPOI(intVec, map);
					break;
				}
			}
		}
	}

	private void GenerateMiscPOIs(Map map)
	{
		int randomInRange = NumMiscPOIsRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			MiscPOIType miscPOIType = Rand.EnumValue<MiscPOIType>();
			if (!poiCells.Empty())
			{
				IntVec3 intVec = poiCells.RandomElement();
				poiCells.Remove(intVec);
				ClearArea(intVec, map);
				GeneratePOIWalls(intVec, map, out var _);
				switch (miscPOIType)
				{
				case MiscPOIType.EggSacs:
					GenerateEggSacs(intVec, map, Rand.Range(5, 10));
					break;
				case MiscPOIType.BoomShrooms:
					GeneratePlants(ThingDefOf.Boomshroom, intVec, map, Rand.Range(5, 10));
					break;
				case MiscPOIType.Mushrooms:
					GeneratePlants(ThingDefOf.Plant_Psilocap, intVec, map, Rand.Range(5, 10));
					GeneratePlants(ThingDefOf.Agarilux, intVec, map, Rand.Range(5, 10));
					break;
				case MiscPOIType.Corpses:
					GenStep_UndercaveInterest.GenerateCorpsePile(map, intVec);
					break;
				case MiscPOIType.HermeticCrate:
					GenerateHermeticCratePOI(intVec, map);
					break;
				}
			}
		}
	}

	private void GenerateRandomHives(Map map, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (CellFinder.TryFindRandomCell(map, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map) && !c.InHorDistOf(exit.Position, 20f), out var result))
			{
				HiveUtility.SpawnHive(result, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: true, canSpawnHives: false);
			}
		}
	}

	private IntVec3 GetPOILocation(Map map)
	{
		Thing entrance = map.listerThings.ThingsOfDef(ThingDefOf.CaveExit).FirstOrDefault();
		if (entrance == null)
		{
			Log.Error("No cave entrance found for Insect Lair POI generation.");
			return IntVec3.Invalid;
		}
		if (!CellFinder.TryFindRandomCell(map, delegate(IntVec3 c)
		{
			if (!c.Standable(map))
			{
				return false;
			}
			if (c.DistanceToEdge(map) < 10)
			{
				return false;
			}
			if (entrance.Position.InHorDistOf(c, 20f))
			{
				return false;
			}
			foreach (IntVec3 poiCell in poiCells)
			{
				if (c.InHorDistOf(poiCell, 15f))
				{
					return false;
				}
			}
			return map.reachability.CanReach(c, entrance.Position, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors) ? true : false;
		}, out var result))
		{
			return IntVec3.Invalid;
		}
		return result;
	}

	private void ClearArea(IntVec3 loc, Map map)
	{
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, 50, (IntVec3 c) => true))
		{
			item.GetEdifice(map)?.Destroy();
		}
	}

	private void GeneratePOIWalls(IntVec3 loc, Map map, out List<IntVec3> shape)
	{
		shape = GridShapeMaker.IrregularLump(loc, map, 154, (IntVec3 c) => true);
		foreach (IntVec3 item in shape)
		{
			TryPlaceSludge(item, map);
		}
		List<IntVec3> list = new List<IntVec3>();
		foreach (IntVec3 item2 in shape)
		{
			bool flag = false;
			foreach (IntVec3 item3 in GenRadial.RadialCellsAround(item2, 2f, useCenter: false))
			{
				if (!shape.Contains(item3))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(item2);
			}
		}
		foreach (IntVec3 item4 in list)
		{
			shape.Remove(item4);
		}
		foreach (IntVec3 item5 in shape)
		{
			if (item5.DistanceToEdge(map) > 1)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.BurrowWall);
				if (GenPlace.TryPlaceThing(thing, item5, map, ThingPlaceMode.Direct))
				{
					thing.SetFaction(Faction.OfInsects);
				}
			}
		}
	}

	private void GenerateEggSacs(IntVec3 loc, Map map, int num)
	{
		for (int i = 0; i < num; i++)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.EggSac);
			thing.SetFaction(Faction.OfInsects);
			GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Radius, null, null, null, 5);
		}
	}

	private void GenerateHermeticCratePOI(IntVec3 loc, Map map)
	{
		Rot4 rot = Rot4.Random;
		if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.AncientHermeticCrate, c, map, rot)))
		{
			RoomGenUtility.SpawnCrate(ThingDefOf.AncientHermeticCrate, result, map, rot, ThingSetMakerDefOf.MapGen_HighValueCrate);
			int num = Rand.Range(3, 4);
			GenerateEggSacs(loc, map, num);
		}
	}

	private void GenerateBossRoom(IntVec3 loc, Map map)
	{
		if (!CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result))
		{
			return;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.HiveQueen, Faction.OfInsects);
		pawn.TryGetComp<CompCanBeDormant>().ToSleep();
		GenSpawn.Spawn(pawn, result, map);
		LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_HiveQueen(Faction.OfInsects, result, 12f, sendWokenUpMessage: false), map, Gen.YieldSingle(pawn));
		int num = Rand.Range(3, 4);
		GenerateEggSacs(loc, map, num);
		int num2 = (int)BossHivesThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
		for (int i = 0; i < num2; i++)
		{
			if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result2, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map)))
			{
				HiveUtility.SpawnHive(result2, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: false, canSpawnInsects: true, dormant: true);
			}
		}
		if (Entrance != null && Entrance.spawnGravcore)
		{
			GenSpawn.Spawn(ThingDefOf.Gravcore, loc, map).SetForbidden(value: true);
			int num3 = Rand.Range(2, 4);
			for (int num4 = 0; num4 < num3; num4++)
			{
				GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.ShipChunk_Mech), loc, map, ThingPlaceMode.Radius, null, null, null, 5);
			}
			int num5 = GravlitePanelCount.RandomInRange;
			while (num5 > 0)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
				thing.stackCount = Mathf.Min(num5, thing.def.stackLimit);
				num5 -= thing.stackCount;
				thing.SetForbidden(value: true);
				GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near);
			}
		}
		else
		{
			Rot4 rot = Rot4.Random;
			if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result3, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.AncientHermeticCrate, c, map, rot)))
			{
				RoomGenUtility.SpawnCrate(ThingDefOf.AncientHermeticCrate, result3, map, rot, ThingSetMakerDefOf.MapGen_HighValueCrate);
			}
		}
	}

	private void GenerateInsectHivePOI(IntVec3 loc, Map map)
	{
		int num = (int)HivesFromThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
		for (int i = 0; i < num; i++)
		{
			if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map)))
			{
				HiveUtility.SpawnHive(result, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: false, canSpawnInsects: true, dormant: true);
			}
		}
		int num2 = Rand.Range(2, 4);
		GenerateEggSacs(loc, map, num2);
	}

	private void GenerateInsectCocoonPOI(IntVec3 loc, Map map)
	{
		IEnumerable<ThingDef> cocoonsToSpawn = CocoonInfestationUtility.GetCocoonsToSpawn(CocoonThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap)));
		int nextCocoonGroupID = Find.UniqueIDsManager.GetNextCocoonGroupID();
		foreach (ThingDef item in cocoonsToSpawn)
		{
			Thing thing = ThingMaker.MakeThing(item);
			thing.SetFaction(Faction.OfInsects);
			thing.TryGetComp<CompWakeUpDormant>().groupID = nextCocoonGroupID;
			GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Radius, null, null, null, 5);
		}
	}

	private void GenerateAmbushPOI(IntVec3 loc, Map map)
	{
		GenSpawn.Spawn(ThingDefOf.InsectAmbush, loc, map);
		int num = Rand.Range(5, 10);
		GenerateEggSacs(loc, map, num);
	}

	private void GeneratePlants(ThingDef plantDef, IntVec3 loc, Map map, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Thing thing = ThingMaker.MakeThing(plantDef);
			if (GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Radius, out var _, null, (IntVec3 c) => c.GetPlant(map) == null, null, 5))
			{
				(thing as Plant).Growth = Mathf.Clamp01(WildPlantSpawner.InitialGrowthRandomRange.RandomInRange);
			}
		}
	}

	private void TryPlaceSludge(IntVec3 cell, Map map)
	{
		if (GenConstruct.CanBuildOnTerrain(TerrainDefOf.InsectSludge, cell, map, Rot4.North))
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.InsectSludge);
			if (Rand.Chance(0.2f))
			{
				GenSpawn.Spawn(ThingDefOf.Filth_Slime, cell, map);
			}
		}
	}
}
