using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class SiegeBlueprintPlacer
{
	private static IntVec3 center;

	private static Faction faction;

	private static List<IntVec3> placedCoverLocs = new List<IntVec3>();

	private const int MaxArtyCount = 2;

	public const float ArtyCost = 60f;

	private const int MinCoverDistSquared = 36;

	private static readonly IntRange NumCoverRange = new IntRange(2, 4);

	private static readonly IntRange CoverLengthRange = new IntRange(2, 7);

	public static IEnumerable<Blueprint_Build> PlaceBlueprints(IntVec3 placeCenter, Map map, Faction placeFaction, float points)
	{
		center = placeCenter;
		faction = placeFaction;
		foreach (Blueprint_Build item in PlaceCoverBlueprints(map))
		{
			yield return item;
		}
		foreach (Blueprint_Build item2 in PlaceArtilleryBlueprints(points, map))
		{
			yield return item2;
		}
	}

	private static bool CanPlaceBlueprintAt(IntVec3 root, Rot4 rot, ThingDef buildingDef, Map map, ThingDef stuffDef)
	{
		return GenConstruct.CanPlaceBlueprintAt(buildingDef, root, rot, map, godMode: false, null, null, stuffDef, ignoreEdgeArea: true, ignoreInteractionSpots: true, ignoreClearableFreeBuildings: true).Accepted;
	}

	private static IEnumerable<Blueprint_Build> PlaceCoverBlueprints(Map map)
	{
		placedCoverLocs.Clear();
		ThingDef coverThing;
		ThingDef coverStuff;
		if (Rand.Chance(0.5f))
		{
			coverThing = ThingDefOf.Sandbags;
			coverStuff = ThingDefOf.Cloth;
		}
		else
		{
			coverThing = ThingDefOf.Barricade;
			coverStuff = (Rand.Chance(0.5f) ? ThingDefOf.Steel : ThingDefOf.WoodLog);
		}
		int numCover = NumCoverRange.RandomInRange;
		for (int i = 0; i < numCover; i++)
		{
			IntVec3 bagRoot = FindCoverRoot(map, coverThing, coverStuff);
			if (!bagRoot.IsValid)
			{
				break;
			}
			Rot4 growDir = ((bagRoot.x <= center.x) ? Rot4.East : Rot4.West);
			Rot4 growDirB = ((bagRoot.z <= center.z) ? Rot4.North : Rot4.South);
			foreach (Blueprint_Build item in MakeCoverLine(bagRoot, map, growDir, CoverLengthRange.RandomInRange, coverThing, coverStuff))
			{
				yield return item;
			}
			bagRoot += growDirB.FacingCell;
			foreach (Blueprint_Build item2 in MakeCoverLine(bagRoot, map, growDirB, CoverLengthRange.RandomInRange, coverThing, coverStuff))
			{
				yield return item2;
			}
		}
	}

	private static IEnumerable<Blueprint_Build> MakeCoverLine(IntVec3 root, Map map, Rot4 growDir, int maxLength, ThingDef coverThing, ThingDef coverStuff)
	{
		IntVec3 cur = root;
		for (int j = 0; j < maxLength; j++)
		{
			if (!CanPlaceBlueprintAt(cur, Rot4.North, coverThing, map, coverStuff))
			{
				break;
			}
			yield return GenConstruct.PlaceBlueprintForBuild(coverThing, cur, map, Rot4.North, faction, coverStuff);
			placedCoverLocs.Add(cur);
			cur += growDir.FacingCell;
		}
	}

	private static IEnumerable<Blueprint_Build> PlaceArtilleryBlueprints(float points, Map map)
	{
		IEnumerable<ThingDef> artyDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.building != null && def.building.buildingTags.Contains("Artillery_BaseDestroyer"));
		int numArtillery = Mathf.RoundToInt(points / 60f);
		numArtillery = Mathf.Clamp(numArtillery, 1, 2);
		for (int i = 0; i < numArtillery; i++)
		{
			Rot4 random = Rot4.Random;
			ThingDef thingDef = artyDefs.RandomElement();
			IntVec3 intVec = FindArtySpot(thingDef, random, map);
			if (!intVec.IsValid)
			{
				break;
			}
			yield return GenConstruct.PlaceBlueprintForBuild(thingDef, intVec, map, random, faction, ThingDefOf.Steel);
			points -= 60f;
		}
	}

	private static IntVec3 FindCoverRoot(Map map, ThingDef coverThing, ThingDef coverStuff)
	{
		CellRect cellRect = CellRect.CenteredOn(center, 13);
		cellRect.ClipInsideMap(map);
		CellRect cellRect2 = CellRect.CenteredOn(center, 8);
		cellRect2.ClipInsideMap(map);
		int num = 0;
		IntVec3 randomCell;
		while (true)
		{
			num++;
			if (num > 200)
			{
				return IntVec3.Invalid;
			}
			randomCell = cellRect.RandomCell;
			if (cellRect2.Contains(randomCell) || !map.reachability.CanReach(randomCell, center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) || !CanPlaceBlueprintAt(randomCell, Rot4.North, coverThing, map, coverStuff))
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < placedCoverLocs.Count; i++)
			{
				if ((float)(placedCoverLocs[i] - randomCell).LengthHorizontalSquared < 36f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return randomCell;
	}

	private static IntVec3 FindArtySpot(ThingDef artyDef, Rot4 rot, Map map)
	{
		CellRect cellRect = CellRect.CenteredOn(center, 8);
		cellRect.ClipInsideMap(map);
		int num = 0;
		IntVec3 randomCell;
		do
		{
			num++;
			if (num > 200)
			{
				return IntVec3.Invalid;
			}
			randomCell = cellRect.RandomCell;
		}
		while (!map.reachability.CanReach(randomCell, center, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) || randomCell.Roofed(map) || !CanPlaceBlueprintAt(randomCell, rot, artyDef, map, ThingDefOf.Steel));
		return randomCell;
	}
}
