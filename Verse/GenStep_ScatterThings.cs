using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class GenStep_ScatterThings : GenStep_Scatterer
{
	public ThingDef thingDef;

	public ThingDef stuff;

	public int clearSpaceSize;

	public int clusterSize = 1;

	public QualityCategory? quality;

	public bool minify;

	public bool avoidUsedRects;

	public bool createUsedRect;

	public bool skipRoofed;

	public ThingDef filthDef;

	public int filthExpandBy;

	public float filthChance = 0.5f;

	public float terrainValidationRadius;

	[NoTranslate]
	private List<string> terrainValidationDisallowed;

	[NoTranslate]
	private List<string> terrainValidationAllowed;

	[Unsaved(false)]
	private IntVec3 clusterCenter;

	[Unsaved(false)]
	private int leftInCluster;

	private const int ClusterRadius = 4;

	private List<Rot4> possibleRotationsInt;

	private static List<Rot4> tmpRotations = new List<Rot4>();

	public override int SeedPart => 1158116095;

	private List<Rot4> PossibleRotations
	{
		get
		{
			if (possibleRotationsInt == null)
			{
				possibleRotationsInt = new List<Rot4>();
				if (thingDef.rotatable)
				{
					possibleRotationsInt.Add(Rot4.North);
					possibleRotationsInt.Add(Rot4.East);
					possibleRotationsInt.Add(Rot4.South);
					possibleRotationsInt.Add(Rot4.West);
				}
				else
				{
					possibleRotationsInt.Add(Rot4.North);
				}
			}
			return possibleRotationsInt;
		}
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ShouldSkipMap(map))
		{
			return;
		}
		int num = CalculateFinalCount(map);
		IntRange stackSizeRange = ((thingDef.ingestible != null && thingDef.ingestible.IsMeal && thingDef.stackLimit <= 10) ? IntRange.One : ((thingDef.stackLimit <= 5) ? new IntRange(thingDef.stackLimit, thingDef.stackLimit) : new IntRange(Mathf.RoundToInt((float)thingDef.stackLimit * 0.5f), thingDef.stackLimit)));
		List<int> list = CountDividedIntoStacks(num, stackSizeRange);
		for (int i = 0; i < list.Count; i++)
		{
			if (!TryFindScatterCell(map, out var result))
			{
				return;
			}
			ScatterAt(result, map, parms, list[i]);
			usedSpots.Add(result);
		}
		usedSpots.Clear();
		clusterCenter = IntVec3.Invalid;
		leftInCluster = 0;
	}

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		if (clusterSize > 1)
		{
			if (leftInCluster <= 0)
			{
				if (!base.TryFindScatterCell(map, out clusterCenter))
				{
					Log.Error("Could not find cluster center to scatter " + thingDef);
				}
				leftInCluster = clusterSize;
			}
			leftInCluster--;
			result = CellFinder.RandomClosewalkCellNear(clusterCenter, map, 4, (IntVec3 x) => TryGetRandomValidRotation(x, map, out var _));
			return result.IsValid;
		}
		return base.TryFindScatterCell(map, out result);
	}

	protected virtual Thing GenerateThing()
	{
		Thing thing = ThingMaker.MakeThing(thingDef, stuff);
		if (thing.TryGetComp(out CompQuality comp))
		{
			comp.SetQuality(quality ?? QualityCategory.Normal, ArtGenerationContext.Outsider);
		}
		if (thingDef.IsIngestible && thingDef.ingestible.IsMeal)
		{
			FoodUtility.GenerateGoodIngredients(thing, Faction.OfPlayer.ideos.PrimaryIdeo);
		}
		if (minify && thingDef.Minifiable)
		{
			thing = thing.MakeMinified();
		}
		return thing;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
	{
		if (!TryGetRandomValidRotation(loc, map, out var rot))
		{
			Log.Warning("Could not find any valid rotation for " + thingDef);
			return;
		}
		if (clearSpaceSize > 0)
		{
			foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, clearSpaceSize))
			{
				item.GetEdifice(map)?.Destroy();
			}
		}
		Thing thing = GenerateThing();
		if (thing.def.category == ThingCategory.Item)
		{
			thing.stackCount = stackCount;
			thing.SetForbidden(value: true, warnOnFail: false);
			GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near, out var lastResultingThing);
			if (nearPlayerStart && lastResultingThing != null && lastResultingThing.def.category == ThingCategory.Item && TutorSystem.TutorialMode)
			{
				Find.TutorialState.AddStartingItem(lastResultingThing);
			}
		}
		else
		{
			GenSpawn.Spawn(thing, loc, map, rot);
		}
		if (filthDef != null)
		{
			foreach (IntVec3 item2 in thing.OccupiedRect().ExpandedBy(filthExpandBy))
			{
				if (Rand.Chance(filthChance) && item2.InBounds(thing.Map))
				{
					FilthMaker.TryMakeFilth(item2, thing.Map, filthDef);
				}
			}
		}
		if (createUsedRect)
		{
			MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(thing.OccupiedRect());
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		if (!GenSpawn.CanSpawnAt(thingDef, loc, map))
		{
			return false;
		}
		if (!TryGetRandomValidRotation(loc, map, out var _))
		{
			return false;
		}
		if (terrainValidationRadius > 0f)
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(loc, terrainValidationRadius, useCenter: true))
			{
				if (!item.InBounds(map))
				{
					continue;
				}
				TerrainDef terrain = item.GetTerrain(map);
				if (terrainValidationDisallowed != null)
				{
					foreach (string item2 in terrainValidationDisallowed)
					{
						if (terrain.HasTag(item2))
						{
							return false;
						}
					}
				}
				if (terrainValidationAllowed == null)
				{
					continue;
				}
				foreach (string item3 in terrainValidationAllowed)
				{
					if (!terrain.HasTag(item3))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool TryGetRandomValidRotation(IntVec3 loc, Map map, out Rot4 rot)
	{
		List<Rot4> possibleRotations = PossibleRotations;
		for (int i = 0; i < possibleRotations.Count; i++)
		{
			if (IsRotationValid(loc, possibleRotations[i], map))
			{
				tmpRotations.Add(possibleRotations[i]);
			}
		}
		if (tmpRotations.TryRandomElement(out rot))
		{
			tmpRotations.Clear();
			return true;
		}
		rot = Rot4.Invalid;
		return false;
	}

	private bool IsRotationValid(IntVec3 loc, Rot4 rot, Map map)
	{
		CellRect cellRect = GenAdj.OccupiedRect(loc, rot, thingDef.size);
		if (!cellRect.InBounds(map))
		{
			return false;
		}
		if (avoidUsedRects && MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Any((CellRect ur) => ur.Overlaps(cellRect)))
		{
			return false;
		}
		if (thingDef.hasInteractionCell)
		{
			IntVec3 c = loc + thingDef.interactionCellOffset.RotatedBy(rot);
			if (!c.InBounds(map) || !c.Walkable(map))
			{
				return false;
			}
		}
		if (GenSpawn.WouldWipeAnythingWith(loc, rot, thingDef, map, (Thing x) => x.def == thingDef || (x.def.category != ThingCategory.Plant && x.def.category != ThingCategory.Filth)))
		{
			return false;
		}
		if (skipRoofed && cellRect.Any((IntVec3 c2) => c2.Roofed(map)))
		{
			return false;
		}
		if (!thingDef.CanSpawnAt(loc, rot, map))
		{
			return false;
		}
		return true;
	}

	public static List<int> CountDividedIntoStacks(int count, IntRange stackSizeRange)
	{
		List<int> list = new List<int>();
		while (count > 0)
		{
			int num = Mathf.Min(count, stackSizeRange.RandomInRange);
			count -= num;
			list.Add(num);
		}
		if (stackSizeRange.max > 2)
		{
			for (int i = 0; i < list.Count * 4; i++)
			{
				int num2 = Rand.RangeInclusive(0, list.Count - 1);
				int num3 = Rand.RangeInclusive(0, list.Count - 1);
				if (num2 != num3 && list[num2] > list[num3])
				{
					int num4 = (int)((float)(list[num2] - list[num3]) * Rand.Value);
					list[num2] -= num4;
					list[num3] += num4;
				}
			}
		}
		return list;
	}
}
