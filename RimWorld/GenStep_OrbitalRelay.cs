using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_OrbitalRelay : GenStep
{
	private OrbitalDebrisDef orbitalDebrisDef;

	private List<PrefabParms> centerPrefabs = new List<PrefabParms>();

	private List<PrefabParms> satellitePrefabs = new List<PrefabParms>();

	private List<PrefabParms> scatterPrefabs = new List<PrefabParms>();

	private IntRange satelliteRange = new IntRange(2, 4);

	private IntRange scatterRange = new IntRange(2, 3);

	private const int SatelliteSize = 20;

	private static readonly Dictionary<PrefabDef, int> used = new Dictionary<PrefabDef, int>();

	public override int SeedPart => 982398321;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckOdyssey("Orbital Relay"))
		{
			return;
		}
		map.regionAndRoomUpdater.Enabled = true;
		CellRect area;
		foreach (var item3 in GetSatelliteRects(area = GeneratePlatform(map)).TakeRandomDistinct(satelliteRange.RandomInRange))
		{
			CellRect item = item3.Item1;
			Rot4 item2 = item3.Item2;
			area = area.Encapsulate(item);
			SpawnSatellite(map, item, item2);
		}
		ReserveRelayAreas(map);
		if (TryGetRect(area, map, new IntVec2(12, 12), out var rect) || TryGetRect(area, map, new IntVec2(10, 10), out rect))
		{
			MapGenerator.PlayerStartSpot = rect.CenterCell;
		}
		int randomInRange = scatterRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			TryScatterPrefab(area, map);
		}
		map.OrbitalDebris = orbitalDebrisDef;
		used.Clear();
	}

	private static void ReserveRelayAreas(Map map)
	{
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.MechRelay))
		{
			MapGenerator.UsedRects.Add(item.OccupiedRect().ExpandedBy(2));
		}
	}

	private void TryScatterPrefab(CellRect area, Map map)
	{
		scatterPrefabs.Where(RequiredForMinimum).TryRandomElementByWeight((PrefabParms w) => w.weight, out var result);
		if (result == null)
		{
			scatterPrefabs.Where(CanSpawn).TryRandomElementByWeight((PrefabParms w) => w.weight, out result);
		}
		if (result != null && TryGetPrefabSpawn(area, map, result.def, out (IntVec3, Rot4) result2))
		{
			PrefabUtility.SpawnPrefab(result.def, map, result2.Item1, result2.Item2);
			if (!used.TryAdd(result.def, 1))
			{
				used[result.def]++;
			}
			CellRect cellRect = result2.Item1.RectAbout(result.def.size, result2.Item2);
			MapGenerator.UsedRects.Add(cellRect.ExpandedBy(1));
		}
	}

	private void SpawnSatellite(Map map, CellRect rect, Rot4 rot)
	{
		satellitePrefabs.Where(RequiredForMinimum).TryRandomElementByWeight((PrefabParms w) => w.weight, out var result);
		if (result == null)
		{
			satellitePrefabs.Where(CanSpawn).TryRandomElementByWeight((PrefabParms w) => w.weight, out result);
		}
		if (result != null)
		{
			PrefabUtility.SpawnPrefab(result.def, map, rect.CenterCell, rot);
			if (!used.TryAdd(result.def, 1))
			{
				used[result.def]++;
			}
		}
	}

	private static bool RequiredForMinimum(PrefabParms p)
	{
		if (p.minMaxRange.IsValid && p.minMaxRange.min > 0)
		{
			if (used.TryGetValue(p.def, out var value))
			{
				return value < p.minMaxRange.min;
			}
			return true;
		}
		return false;
	}

	private static bool CanSpawn(PrefabParms p)
	{
		if (!p.minMaxRange.IsInvalid && used.TryGetValue(p.def, out var value))
		{
			return value < p.minMaxRange.max;
		}
		return true;
	}

	private static bool TryGetPrefabSpawn(CellRect area, Map map, PrefabDef prefab, out (IntVec3 cell, Rot4 rot) result)
	{
		List<CellRect> usedRects = MapGenerator.UsedRects;
		int num = Rand.Range(0, 3);
		foreach (IntVec3 item in area.Cells.InRandomOrder())
		{
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(num + i);
				CellRect cellRect = item.RectAbout(prefab.size, rot);
				bool flag = true;
				foreach (IntVec3 inner in cellRect)
				{
					if (usedRects.Any((CellRect r) => r.Contains(inner)))
					{
						flag = false;
						break;
					}
				}
				if (flag && PrefabUtility.CanSpawnPrefab(prefab, map, item, rot, canWipeEdifices: false))
				{
					result = (cell: item, rot: rot);
					return true;
				}
			}
		}
		result = default((IntVec3, Rot4));
		return false;
	}

	private static bool TryGetRect(CellRect area, Map map, IntVec2 size, out CellRect rect)
	{
		List<CellRect> usedRects = MapGenerator.UsedRects;
		bool num = area.TryFindRandomInnerRect(size, out rect, Validator);
		if (num)
		{
			usedRects.Add(rect);
		}
		return num;
		bool Validator(CellRect r)
		{
			if (!usedRects.Any((CellRect ur) => ur.Overlaps(r)))
			{
				return r.Cells.All((IntVec3 c) => c.GetTerrain(map) == TerrainDefOf.MechanoidPlatform && c.GetEdifice(map) == null);
			}
			return false;
		}
	}

	private static List<(CellRect, Rot4)> GetSatelliteRects(CellRect center)
	{
		List<(CellRect, Rot4)> list = new List<(CellRect, Rot4)>(8);
		(int, int, int, int, Rot4, bool)[] array = new(int, int, int, int, Rot4, bool)[4]
		{
			(center.minX, center.maxZ, center.maxX, center.maxZ + 20, Rot4.North, true),
			(center.minX, center.minZ - 20, center.maxX, center.minZ - 1, Rot4.South, true),
			(center.maxX, center.minZ, center.maxX + 20, center.maxZ, Rot4.East, false),
			(center.minX - 20, center.minZ, center.minX - 1, center.maxZ, Rot4.West, false)
		};
		for (int i = 0; i < array.Length; i++)
		{
			(int, int, int, int, Rot4, bool) tuple = array[i];
			int item = tuple.Item1;
			int item2 = tuple.Item2;
			int item3 = tuple.Item3;
			int item4 = tuple.Item4;
			Rot4 item5 = tuple.Item5;
			bool item6 = tuple.Item6;
			CellRect cellRect = CellRect.FromLimits(item, item2, item3, item4);
			CellRect item7;
			CellRect item8;
			if (!item6)
			{
				(item7, item8) = cellRect.SplitVertical();
			}
			else
			{
				(item7, item8) = cellRect.SplitHorizontal();
			}
			list.Add((item7, item5));
			list.Add((item8, item5));
		}
		return list;
	}

	private CellRect GeneratePlatform(Map map)
	{
		PrefabParms prefabParms = centerPrefabs.RandomElementByWeight((PrefabParms w) => w.weight);
		CellRect result = map.Center.RectAbout(prefabParms.def.size);
		PrefabUtility.SpawnPrefab(prefabParms.def, map, map.Center, Rot4.North);
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.MechRelay))
		{
			item.TryGetComp<CompMechRelay>().inert = true;
		}
		return result;
	}
}
