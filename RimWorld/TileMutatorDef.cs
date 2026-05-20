using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TileMutatorDef : Def
{
	private Type workerClass;

	public int genOrder;

	[NoTranslate]
	public List<string> categories = new List<string>();

	[NoTranslate]
	public List<string> overrideCategories = new List<string>();

	public int priority;

	public int displayPriority;

	public float chanceOnNonLandmarkTile;

	public List<GenStepDef> extraGenSteps = new List<GenStepDef>();

	public List<GenStepDef> preventGenSteps = new List<GenStepDef>();

	public TerrainDef overrideCoastalBeachTerrain;

	public TerrainDef overrideLakeBeachTerrain;

	public TerrainDef overrideRiverbankTerrain;

	public TerrainDef overrideMudTerrain;

	public bool preventsPondGeneration;

	public bool preventsLandmarks;

	public bool preventNaturalElevation;

	public bool preventPatches;

	public float animalDensityFactor = 1f;

	public float plantDensityFactor = 1f;

	public float junkDensityFactor = 1f;

	public float geyserCountFactor = 1f;

	public float chunkDensityFactor = 1f;

	public float fishPopulationFactor = 1f;

	public List<BiomePlantRecord> additionalWildPlants = new List<BiomePlantRecord>();

	public SimpleCurve overrideDensityForFertilityCurve;

	public bool allowRoofedEdgeWalkIn;

	public List<RaidStrategyDef> blacklistedRaidStrategies = new List<RaidStrategyDef>();

	public List<GameConditionDef> additionalGameConditions = new List<GameConditionDef>();

	public Hilliness hillinessForElevationGen;

	public Hilliness hillinessForOreGeneration;

	public Hilliness hillinessLabel;

	public List<BiomeDef> biomeWhitelist;

	public List<BiomeDef> biomeBlacklist;

	public FloatRange animalDensityRange = new FloatRange(0f, float.MaxValue);

	public FloatRange plantDensityRange = new FloatRange(0f, float.MaxValue);

	public Hilliness minHilliness;

	public Hilliness maxHilliness;

	public IntRange coastSidesRange = IntRange.Invalid;

	public bool canSpawnOnRiver = true;

	public bool canSpawnOnRoad = true;

	public bool canSpawnOnLandmark = true;

	public FactionDef requiresFactionOfDef;

	public FloatRange pollutionRange = new FloatRange(0f, float.MaxValue);

	public FloatRange averageTemperatureRange = new FloatRange(float.MinValue, float.MaxValue);

	public float weatherFrequencyFactor = 1f;

	public float weatherFrequencyOffset;

	public List<WeatherDef> weathersToAffect = new List<WeatherDef>();

	public List<TerrainPatchMaker> terrainPatchMakers = new List<TerrainPatchMaker>();

	public List<ThingDef> plantKinds = new List<ThingDef>();

	public AncientStructureGenParms structureGenParms;

	public List<ThingDef> resourceBlacklist = new List<ThingDef>();

	private HashSet<BiomeDef> biomeWhitelistSet;

	private HashSet<BiomeDef> biomeBlacklistSet;

	private TileMutatorWorker mutatorWorker;

	private static readonly List<PlanetTile> tmpTileNeighbors = new List<PlanetTile>();

	public TileMutatorWorker Worker
	{
		get
		{
			if (mutatorWorker != null)
			{
				return mutatorWorker;
			}
			if (workerClass != null)
			{
				return mutatorWorker = (TileMutatorWorker)Activator.CreateInstance(workerClass, this);
			}
			return null;
		}
	}

	public bool IsCave => categories.Contains("Caves");

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (biomeWhitelist != null)
		{
			biomeWhitelistSet = new HashSet<BiomeDef>(biomeWhitelist);
		}
		if (biomeBlacklist != null)
		{
			biomeBlacklistSet = new HashSet<BiomeDef>(biomeBlacklist);
		}
	}

	public string Label(PlanetTile tileID)
	{
		return Worker?.GetLabel(tileID) ?? label;
	}

	public string Description(PlanetTile tileID)
	{
		return Worker?.GetDescription(tileID) ?? description;
	}

	public bool EverValid()
	{
		if (requiresFactionOfDef != null && Find.FactionManager.FirstFactionOfDef(requiresFactionOfDef) == null)
		{
			return false;
		}
		return true;
	}

	public bool IsValidTile(PlanetTile tile, PlanetLayer layer)
	{
		if (!EverValid())
		{
			return false;
		}
		Tile tile2 = layer[tile];
		BiomeDef primaryBiome = tile2.PrimaryBiome;
		if (tile2.Mutators.Contains(this))
		{
			return false;
		}
		if (Worker != null && !Worker.IsValidTile(tile, layer))
		{
			return false;
		}
		foreach (string category in categories)
		{
			foreach (TileMutatorDef mutator in tile2.Mutators)
			{
				if (mutator.categories.Contains(category) && mutator.priority >= priority)
				{
					return false;
				}
				if (mutator.overrideCategories.Contains(category))
				{
					return false;
				}
			}
		}
		if (!animalDensityRange.Includes(primaryBiome.animalDensity))
		{
			return false;
		}
		if (!plantDensityRange.Includes(primaryBiome.plantDensity))
		{
			return false;
		}
		if (!pollutionRange.Includes(tile2.pollution))
		{
			return false;
		}
		if (!averageTemperatureRange.Includes(tile2.temperature))
		{
			return false;
		}
		if (minHilliness != Hilliness.Undefined && (int)tile2.hilliness < (int)minHilliness)
		{
			return false;
		}
		if (maxHilliness != Hilliness.Undefined && (int)tile2.hilliness > (int)maxHilliness)
		{
			return false;
		}
		if (biomeWhitelistSet != null && !biomeWhitelistSet.Contains(primaryBiome))
		{
			return false;
		}
		if (biomeBlacklistSet != null && biomeBlacklistSet.Contains(primaryBiome))
		{
			return false;
		}
		if (coastSidesRange != IntRange.Invalid)
		{
			tmpTileNeighbors.Clear();
			layer.GetTileNeighbors(tile, tmpTileNeighbors);
			bool flag = coastSidesRange.max == 0;
			int num = 0;
			for (int i = 0; i < tmpTileNeighbors.Count; i++)
			{
				BiomeDef primaryBiome2 = layer[tmpTileNeighbors[i]].PrimaryBiome;
				if (primaryBiome2 == BiomeDefOf.Ocean || primaryBiome2 == BiomeDefOf.Lake)
				{
					if (flag)
					{
						return false;
					}
					num++;
				}
			}
			if (!coastSidesRange.Includes(num))
			{
				return false;
			}
		}
		if (tile2 is SurfaceTile surfaceTile)
		{
			if (!surfaceTile.Rivers.NullOrEmpty() && !canSpawnOnRiver)
			{
				return false;
			}
			if (!surfaceTile.Roads.NullOrEmpty() && !canSpawnOnRoad)
			{
				return false;
			}
		}
		if (tile2.Landmark != null && !canSpawnOnLandmark)
		{
			return false;
		}
		return true;
	}
}
