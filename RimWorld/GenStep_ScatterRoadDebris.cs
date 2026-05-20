using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_ScatterRoadDebris : GenStep_Scatterer
{
	private const int EngineBlockMinDistance = 10;

	private const float EngineBlockChance = 0.5f;

	private const float OilSmearChance = 0.15f;

	private static readonly IntRange VehicleRangeNonRoadMap = new IntRange(1, 2);

	private static readonly IntRange VehicleRangeRoadMap = new IntRange(2, 3);

	private ThingDef thingToPlace;

	private Rot4 rotation;

	private bool mapHasRoads;

	public override int SeedPart => 765346456;

	public IEnumerable<ThingDef> Debris
	{
		get
		{
			yield return ThingDefOf.AncientRustedCarFrame;
			yield return ThingDefOf.AncientRustedJeep;
			yield return ThingDefOf.AncientRustedCar;
			yield return ThingDefOf.AncientTank;
			yield return ThingDefOf.AncientPodCar;
			yield return ThingDefOf.AncientHydrant;
		}
	}

	protected override float GetPlacementFactor(Map map)
	{
		float num = 1f;
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			num *= mutator.junkDensityFactor;
		}
		return num;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter road debris"))
		{
			allowInWaterBiome = false;
			mapHasRoads = HasAncientRoads(map);
			count = (mapHasRoads ? VehicleRangeRoadMap : VehicleRangeNonRoadMap).RandomInRange;
			thingToPlace = Debris.RandomElement();
			base.Generate(map, parms);
		}
	}

	private bool HasAncientRoads(Map map)
	{
		if (map.TileInfo.Isnt<SurfaceTile>(out var casted) || casted.Roads == null)
		{
			return false;
		}
		for (int i = 0; i < casted.Roads.Count; i++)
		{
			if (casted.Roads[i].road == RoadDefOf.AncientAsphaltHighway || casted.Roads[i].road == RoadDefOf.AncientAsphaltRoad)
			{
				return true;
			}
		}
		return false;
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (mapHasRoads && !c.GetTerrain(map).IsRoad)
		{
			return false;
		}
		if (thingToPlace.rotatable)
		{
			int num = Rand.RangeInclusive(1, 4);
			for (int i = 0; i < 4; i++)
			{
				rotation = new Rot4((i + num) % 4);
				if (CanPlaceThingAt(c, rotation, map, thingToPlace))
				{
					return true;
				}
			}
			return false;
		}
		rotation = thingToPlace.defaultPlacingRot;
		return CanPlaceThingAt(c, rotation, map, thingToPlace);
	}

	private bool CanPlaceThingAt(IntVec3 c, Rot4 rot, Map map, ThingDef thingDef)
	{
		return ScatterDebrisUtility.CanPlaceThingAt(c, rot, map, thingDef);
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(thingToPlace), loc, map, rotation);
		ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_MachineBits, 0.75f);
		ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_OilSmear, 0.25f, 0);
		ThingDef engineBlockDef = (Rand.Bool ? ThingDefOf.AncientRustedEngineBlock : ThingDefOf.AncientLargeRustedEngineBlock);
		if (Rand.Chance(0.5f) && RCellFinder.TryFindRandomCellNearWith(loc, (IntVec3 c) => CanPlaceThingAt(c, Rot4.North, map, engineBlockDef), map, out var result, 10))
		{
			ScatterDebrisUtility.ScatterFilthAroundThing(GenSpawn.Spawn(ThingMaker.MakeThing(engineBlockDef), result, map, Rot4.North), map, ThingDefOf.Filth_MachineBits);
			ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_OilSmear, 0.15f, 0);
		}
		thingToPlace = Debris.RandomElement();
	}
}
