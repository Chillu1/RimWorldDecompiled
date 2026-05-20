using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_RockChunks : GenStep
{
	private ModuleBase freqFactorNoise;

	private const float ThreshLooseRock = 0.55f;

	private const float PlaceProbabilityPerCell = 0.006f;

	private const float RubbleProbability = 0.5f;

	public override int SeedPart => 1898758716;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (map.TileInfo.WaterCovered)
		{
			return;
		}
		freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
		freqFactorNoise = new ScaleBias(1.0, 1.0, freqFactorNoise);
		NoiseDebugUI.StoreNoiseRender(freqFactorNoise, "rock_chunks_freq_factor");
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		float num = 0.006f;
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			num *= mutator.chunkDensityFactor;
		}
		foreach (IntVec3 allCell in map.AllCells)
		{
			float num2 = num * freqFactorNoise.GetValue(allCell);
			if ((elevation[allCell] < 0.55f || map.generatorDef.isUnderground) && Rand.Value < num2)
			{
				GrowLowRockFormationFrom(allCell, map);
			}
		}
		freqFactorNoise = null;
	}

	private void GrowLowRockFormationFrom(IntVec3 root, Map map)
	{
		ThingDef filth_RubbleRock = ThingDefOf.Filth_RubbleRock;
		List<ThingDef> forceRockTypes = map.Biome.forceRockTypes;
		ThingDef thingDef = ((forceRockTypes == null) ? Find.World.NaturalRockTypesIn(map.Tile).RandomElement() : forceRockTypes.RandomElement());
		ThingDef mineableThing = thingDef.building.mineableThing;
		if (mineableThing == null)
		{
			return;
		}
		Rot4 random = Rot4.Random;
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		IntVec3 intVec = root;
		while (true)
		{
			Rot4 random2 = Rot4.Random;
			if (random2 == random)
			{
				continue;
			}
			intVec += random2.FacingCell;
			if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null || (!map.generatorDef.isUnderground && elevation[intVec] > 0.55f) || !intVec.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy) || intVec.GetDoor(map) != null)
			{
				break;
			}
			GenSpawn.Spawn(mineableThing, intVec, map);
			IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
			foreach (IntVec3 intVec2 in adjacentCellsAndInside)
			{
				if (!(Rand.Value < 0.5f))
				{
					continue;
				}
				IntVec3 c = intVec + intVec2;
				if (!c.InBounds(map))
				{
					continue;
				}
				bool flag = false;
				List<Thing> thingList = c.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					FilthMaker.TryMakeFilth(c, map, filth_RubbleRock);
				}
			}
		}
	}
}
