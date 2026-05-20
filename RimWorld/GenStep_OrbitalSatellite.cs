using System.Collections.Generic;
using System.Xml;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class GenStep_OrbitalSatellite : GenStep
{
	public class LayoutPrefabParms
	{
		public PrefabDef def;

		public float weight = 1f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "def", "weight");
		}
	}

	private LayoutDef layoutDef;

	private FactionDef factionDef;

	private List<LayoutPrefabParms> prefabs = new List<LayoutPrefabParms>();

	private float? temperature;

	private bool spawnSentryDrones;

	private int bridgeDistance = 34;

	private float ringWidth = 4.9f;

	private float ringExtraOffset = 0.5f;

	private float ringExtraChance;

	private const int ShipPadding = 5;

	private const int CenterPadding = 5;

	private static readonly IntRange CenterSizeRange = new IntRange(48, 65);

	private const int PlatformSize = 23;

	private const int RingSize = 8;

	private static readonly IntRange PlatformConnectorHeightRange = new IntRange(2, 2);

	private static readonly CellRect MinLandingArea = new CellRect(-10, -10, 20, 20);

	private static readonly SimpleCurve SentryCountFromPointsCurve = new SimpleCurve(new CurvePoint[4]
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(100f, 2f),
		new CurvePoint(1000f, 8f),
		new CurvePoint(5000f, 20f)
	});

	protected Faction Faction
	{
		get
		{
			if (factionDef != null)
			{
				return Find.FactionManager.FirstFactionOfDef(factionDef);
			}
			return null;
		}
	}

	protected virtual float SpawnTemp => temperature ?? (-75f);

	public override int SeedPart => 749387154;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckOdyssey("Orbital Satellite"))
		{
			CellRect cellRect = GeneratePlatform(map, parms);
			SpaceGenUtility.GenerateRing(map, cellRect, TerrainDefOf.OrbitalPlatform, bridgeDistance, 8, ringWidth, ringExtraOffset, ringExtraChance);
			GenerateSidePlatforms(cellRect, map, parms);
			map.FogOfWarColor = ColorLibrary.SpaceHumanFog;
		}
	}

	private CellRect GeneratePlatform(Map map, GenStepParams parms)
	{
		int randomInRange = CenterSizeRange.RandomInRange;
		IntVec2 size = new IntVec2(randomInRange, randomInRange);
		IntVec3 center = map.Center;
		Rot4 random = Rot4.Random;
		CellRect cellRect = center.RectAbout(size, random).ClipInsideMap(map);
		CellRect var = cellRect.ContractedBy(5);
		SpawnPlatformTerrain(cellRect, map, 2);
		StructureGenParams parms2 = new StructureGenParams
		{
			size = var.Size
		};
		LayoutWorker worker = layoutDef.Worker;
		LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms2);
		map.layoutStructureSketches.Add(layoutStructureSketch);
		IntVec3 min = var.Min;
		Faction faction = Faction;
		worker.Spawn(layoutStructureSketch, map, min, null, null, roofs: true, canReuseSketch: false, faction);
		MapGenerator.SetVar("SpawnRect", var);
		return cellRect;
	}

	public override void PostMapInitialized(Map map, GenStepParams parms)
	{
		MapGenUtility.SetMapRoomTemperature(map, layoutDef, SpawnTemp);
		if (spawnSentryDrones)
		{
			BaseGenUtility.ScatterSentryDronesInMap(SentryCountFromPointsCurve, map, Faction, parms);
		}
	}

	private void GenerateSidePlatforms(CellRect platformRect, Map map, GenStepParams parms)
	{
		int num = 0;
		int amountToPlace = Rand.Range(2, 4);
		int num2 = Rand.Range(0, 3);
		for (int i = 0; i < 4; i++)
		{
			Rot4 rot = new Rot4((i + num2) % 4);
			CellRect sideRect = new CellRect(0, 0, 23, 23);
			IntVec2 platformPosition = GetPlatformPosition(platformRect, sideRect, rot);
			sideRect = sideRect.MovedBy(platformPosition).ClipInsideMap(map);
			map.landingBlockers.Add(sideRect);
			SpawnPlatformTerrain(sideRect, map);
			if (Rand.DynamicChance(num, amountToPlace, 3 - i))
			{
				num++;
				SpawnWalkway(platformRect, sideRect, rot, map);
			}
			SpawnPrefab(map, sideRect);
		}
	}

	private void SpawnPrefab(Map map, CellRect rect)
	{
		if (prefabs.NullOrEmpty())
		{
			return;
		}
		LayoutPrefabParms layoutPrefabParms = prefabs.RandomElementByWeight((LayoutPrefabParms p) => p.weight);
		Rot4 random = Rot4.Random;
		IntVec3 centerCell = rect.CenterCell;
		if (rect.Width % 2 == 0)
		{
			centerCell.x--;
		}
		if (rect.Height % 2 == 0)
		{
			centerCell.z--;
		}
		for (int num = 0; num < 4; num++)
		{
			random.Rotate(RotationDirection.Clockwise);
			if (PrefabUtility.CanSpawnPrefab(layoutPrefabParms.def, map, centerCell, random, canWipeEdifices: false))
			{
				PrefabUtility.SpawnPrefab(layoutPrefabParms.def, map, centerCell, random, Faction, null, null, OnSpawned);
				break;
			}
		}
		static void OnSpawned(Thing thing)
		{
			if (thing.TryGetComp(out CompPowerBattery comp))
			{
				comp.SetStoredEnergyPct(1f);
			}
			if (thing.TryGetComp(out CompPowerTrader comp2))
			{
				comp2.PowerOn = true;
			}
		}
	}

	private static void SpawnWalkway(CellRect platform, CellRect side, Rot4 sideRot, Map map)
	{
		Rot4 opposite = sideRot.Opposite;
		int randomInRange = PlatformConnectorHeightRange.RandomInRange;
		IntVec3 centerCellOnEdge = side.GetCenterCellOnEdge(opposite, -randomInRange);
		IntVec3 centerCellOnEdge2 = platform.GetCenterCellOnEdge(sideRot, randomInRange);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge, centerCellOnEdge2).ClipInsideMap(map);
		CellRect cellRect2 = cellRect.ContractedBy(sideRot.IsVertical ? 1 : 0, sideRot.IsHorizontal ? 1 : 0).ExpandedBy(sideRot.IsHorizontal ? 2 : 0, sideRot.IsVertical ? 2 : 0);
		foreach (IntVec3 item in cellRect)
		{
			if (item.GetTerrain(map) == TerrainDefOf.Space)
			{
				map.terrainGrid.SetTerrain(item, TerrainDefOf.OrbitalPlatform);
			}
		}
		foreach (IntVec3 item2 in cellRect2)
		{
			map.terrainGrid.SetTerrain(item2, TerrainDefOf.AncientTile);
		}
	}

	private IntVec2 GetPlatformPosition(CellRect centerRect, CellRect sideRect, Rot4 rot)
	{
		IntVec3 centerCell = centerRect.CenterCell;
		if (rot == Rot4.North)
		{
			return new IntVec2(centerCell.x - sideRect.Height / 2, centerRect.maxZ + bridgeDistance);
		}
		if (rot == Rot4.East)
		{
			return new IntVec2(centerRect.maxX + bridgeDistance, centerCell.z - sideRect.Width / 2);
		}
		if (rot == Rot4.South)
		{
			return new IntVec2(centerCell.x - sideRect.Height / 2, centerRect.minZ - bridgeDistance - sideRect.Height);
		}
		return new IntVec2(centerRect.minX - bridgeDistance - sideRect.Width, centerCell.z - sideRect.Height / 2);
	}

	private static void SpawnPlatformTerrain(CellRect rect, Map map, int border = 1)
	{
		CellRect cellRect = rect.ContractedBy(border);
		foreach (IntVec3 item in rect)
		{
			map.terrainGrid.SetTerrain(item, TerrainDefOf.OrbitalPlatform);
			if (cellRect.Contains(item))
			{
				map.terrainGrid.SetTerrain(item, TerrainDefOf.AncientTile);
			}
		}
	}
}
