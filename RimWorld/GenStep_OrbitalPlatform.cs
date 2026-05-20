using System.Collections.Generic;
using System.Xml;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_OrbitalPlatform : GenStep
{
	private class PrefabRange
	{
		public PrefabDef prefab;

		public IntRange countRange;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "prefab", "countRange");
		}
	}

	private FactionDef factionDef;

	private LayoutDef layoutDef;

	private bool useSiteFaction;

	private float? temperature;

	private bool spawnSentryDrones;

	private ThingDef cannonDef;

	private TerrainDef platformTerrain;

	private OrbitalDebrisDef orbitalDebrisDef;

	private ColorInt fogOfWarColor = new ColorInt(43, 46, 47);

	private List<PrefabRange> exteriorPrefabs = new List<PrefabRange>();

	private static readonly IntRange LargeDockRange = new IntRange(1, 2);

	private static readonly IntRange SmallPlatformRange = new IntRange(4, 6);

	private static readonly IntRange SmallPlatformSizeRange = new IntRange(16, 20);

	private static readonly IntRange SmallPlatformDistanceRange = new IntRange(10, 18);

	private static readonly IntRange SizeRange = new IntRange(70, 80);

	private static readonly IntRange LargeLandingAreaWidthRange = new IntRange(30, 40);

	private static readonly IntRange LargeLandingAreaHeightRange = new IntRange(50, 60);

	public static readonly IntRange LandingPadBorderLumpLengthRange = new IntRange(6, 10);

	public static readonly IntRange LandingPadBorderLumpOffsetRange = new IntRange(-1, 1);

	private static readonly SimpleCurve SentryCountFromPointsCurve = new SimpleCurve(new CurvePoint[4]
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(100f, 2f),
		new CurvePoint(1000f, 8f),
		new CurvePoint(5000f, 16f)
	});

	protected virtual LayoutDef LayoutDef => layoutDef;

	protected virtual float SpawnTemp => temperature ?? (-75f);

	public override int SeedPart => 8256151;

	protected virtual Faction GetFaction(Map map)
	{
		if (useSiteFaction && map.Parent.Faction != null)
		{
			return map.Parent.Faction;
		}
		if (factionDef != null)
		{
			return Find.FactionManager.FirstFactionOfDef(factionDef);
		}
		return null;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckOdyssey("Orbital Platform"))
		{
			float? threatPoints = null;
			if (parms.sitePart != null)
			{
				threatPoints = parms.sitePart.parms.points;
			}
			if (!threatPoints.HasValue && map.Parent is Site site)
			{
				threatPoints = site.ActualThreatPoints;
			}
			Faction faction = GetFaction(map);
			CellRect rect = GeneratePlatform(map, faction, threatPoints);
			if (Rand.Chance(0.33f))
			{
				DoRing(map, rect);
			}
			else if (Rand.Chance(0.5f))
			{
				DoLargePlatforms(map, rect);
			}
			else
			{
				DoSmallPlatforms(map, rect);
			}
			SpawnCannons(map, rect.ExpandedBy(6));
			map.FogOfWarColor = fogOfWarColor.ToColor;
			map.OrbitalDebris = orbitalDebrisDef;
			SpawnExteriorPrefabs(map, rect.ExpandedBy(6), faction);
		}
	}

	private void DoLargePlatforms(Map map, CellRect rect)
	{
		int randomInRange = LargeDockRange.RandomInRange;
		List<Rot4> list = new List<Rot4>
		{
			Rot4.North,
			Rot4.East,
			Rot4.South,
			Rot4.West
		};
		for (int i = 0; i < randomInRange; i++)
		{
			if (!list.Any())
			{
				break;
			}
			Rot4 rot = list.RandomElement();
			list.Remove(rot);
			SpaceGenUtility.GenerateConnectedPlatform(map, platformTerrain, rect, LargeLandingAreaWidthRange, LargeLandingAreaHeightRange, rot);
		}
	}

	private void DoSmallPlatforms(Map map, CellRect rect)
	{
		(CellRect, CellRect, CellRect, CellRect) tuple = rect.Subdivide(1);
		int randomInRange = SmallPlatformRange.RandomInRange;
		List<(CellRect, Rot4)> list = new List<(CellRect, Rot4)>
		{
			(tuple.Item1, Rot4.South),
			(tuple.Item1, Rot4.West),
			(tuple.Item3, Rot4.South),
			(tuple.Item3, Rot4.East),
			(tuple.Item2, Rot4.North),
			(tuple.Item2, Rot4.West),
			(tuple.Item4, Rot4.North),
			(tuple.Item4, Rot4.East)
		};
		for (int i = 0; i < randomInRange; i++)
		{
			if (!list.Any())
			{
				break;
			}
			(CellRect, Rot4) tuple2 = list.RandomElement();
			list.Remove(tuple2);
			var (platformRect, dir) = tuple2;
			SpaceGenUtility.GenerateConnectedPlatform(map, platformTerrain, platformRect, SmallPlatformSizeRange, SmallPlatformSizeRange, dir, SmallPlatformDistanceRange.RandomInRange);
		}
	}

	private void DoRing(Map map, CellRect rect)
	{
		float num = Mathf.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height) - (float)rect.Width - 12f;
		SpaceGenUtility.GenerateRing(map, rect, platformTerrain, Mathf.RoundToInt(num / 2f), 0, 13.9f);
	}

	private void SpawnCannons(Map map, CellRect rect)
	{
		if (cannonDef == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			Rot4 rot = new Rot4(i);
			IntVec3 corner = rect.GetCorner(rot);
			int num = Mathf.Max(cannonDef.size.x, cannonDef.size.z) + 4;
			CellRect item = corner.RectAbout(num, num);
			MapGenUtility.Line_NewTemp(platformTerrain, map, corner, rect.CenterCell, 6f, canExit: true, TerrainDefOf.Space);
			foreach (IntVec3 cell in item.Cells)
			{
				if (cell.InHorDistOf(corner, (float)num / 2f - 0.6f))
				{
					map.terrainGrid.SetTerrain(cell, TerrainDefOf.AncientTile);
				}
				else if (cell.InHorDistOf(corner, (float)num / 2f + 0.5f))
				{
					map.terrainGrid.SetTerrain(cell, platformTerrain);
				}
			}
			GenSpawn.Spawn(ThingMaker.MakeThing(cannonDef), corner, map, new Rot4(i % 4));
			MapGenerator.UsedRects.Add(item);
		}
	}

	private void SpawnExteriorPrefabs(Map map, CellRect rect, Faction faction)
	{
		foreach (PrefabRange exteriorPrefab in exteriorPrefabs)
		{
			PrefabRange set = exteriorPrefab;
			int randomInRange = set.countRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				if (rect.TryFindRandomCell(out var cell, Validator))
				{
					Rot4 opposite = rect.GetClosestEdge(cell).Opposite;
					PrefabUtility.SpawnPrefab(set.prefab, map, cell, opposite, faction);
				}
			}
			bool Validator(IntVec3 x)
			{
				Rot4 opposite2 = rect.GetClosestEdge(x).Opposite;
				if (x.GetRoof(map) == null && x.GetAffordances(map).Contains(TerrainAffordanceDefOf.Medium) && rect.DistanceToEdge(x) <= 5f)
				{
					return PrefabUtility.CanSpawnPrefab(set.prefab, map, x, opposite2, canWipeEdifices: false);
				}
				return false;
			}
		}
	}

	private CellRect GeneratePlatform(Map map, Faction faction, float? threatPoints)
	{
		IntVec2 size = new IntVec2(SizeRange.RandomInRange, SizeRange.RandomInRange);
		Rot4 random = Rot4.Random;
		CellRect cellRect = map.Center.RectAbout(size, random).ClipInsideMap(map);
		StructureGenParams parms = new StructureGenParams
		{
			size = cellRect.Size
		};
		LayoutWorker worker = LayoutDef.Worker;
		LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms);
		map.layoutStructureSketches.Add(layoutStructureSketch);
		worker.Spawn(layoutStructureSketch, map, cellRect.Min, threatPoints, null, roofs: true, canReuseSketch: false, faction);
		MapGenerator.SetVar("SpawnRect", cellRect);
		MapGenerator.UsedRects.Add(cellRect);
		return cellRect;
	}

	public override void PostMapInitialized(Map map, GenStepParams parms)
	{
		MapGenUtility.SetMapRoomTemperature(map, layoutDef, SpawnTemp);
		if (spawnSentryDrones)
		{
			BaseGenUtility.ScatterSentryDronesInMap(SentryCountFromPointsCurve, map, GetFaction(map), parms);
		}
	}
}
