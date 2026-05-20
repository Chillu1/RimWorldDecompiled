using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_Fleshmass : GenStep
{
	private float fleshFrequency = 0.3f;

	private float fleshThreshold = 0.2f;

	private float fleshTerrainThreshold = 0.1f;

	private bool fleshmassCanReplaceBuildings;

	private float fleshmassFalloffRadius = -1f;

	private int noiseOctaves = 6;

	private float bloodFrequency = 0.3f;

	private float bloodThreshold = 0.2f;

	public override int SeedPart => 9872136;

	public override void Generate(Map map, GenStepParams parms)
	{
		Perlin perlin = new Perlin(fleshFrequency, 2.0, 0.5, noiseOctaves, Rand.Int, QualityMode.Medium);
		Perlin perlin2 = new Perlin(bloodFrequency, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
		MapGenFloatGrid caves = MapGenerator.Caves;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (map.generatorDef.isUnderground && caves[allCell] <= 0f)
			{
				continue;
			}
			Building edifice = allCell.GetEdifice(map);
			if ((edifice == null || (fleshmassCanReplaceBuildings && !edifice.def.building.isNaturalRock)) && allCell.GetAffordances(map).Contains(ThingDefOf.Fleshmass.terrainAffordanceNeeded))
			{
				float num = (float)perlin.GetValue(allCell.x, 0.0, allCell.z);
				if (fleshmassFalloffRadius > 0f)
				{
					float num2 = allCell.DistanceTo(map.Center);
					float num3 = 1f - Mathf.Clamp01((num2 - fleshmassFalloffRadius) / ((float)map.Size.x / 2f - fleshmassFalloffRadius));
					num *= num3;
				}
				if (num > fleshThreshold)
				{
					GenSpawn.Spawn(ThingDefOf.Fleshmass, allCell, map).SetFaction(Faction.OfEntities);
				}
				if (num > fleshTerrainThreshold)
				{
					map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Flesh);
				}
				float num4 = (float)perlin2.GetValue(allCell.x, 0.0, allCell.z);
				ThingDef filthDef = (Rand.Bool ? ThingDefOf.Filth_TwistedFlesh : ThingDefOf.Filth_Blood);
				if (num4 > bloodThreshold)
				{
					FilthMaker.TryMakeFilth(allCell, map, filthDef);
				}
			}
		}
	}
}
