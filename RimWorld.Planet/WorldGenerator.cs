using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldGenerator
	{
		public const float DefaultPlanetCoverage = 0.3f;

		public const float DefaultPlanetCoverageOdyssey = 0.5f;

		public const OverallRainfall DefaultOverallRainfall = OverallRainfall.Normal;

		public const OverallPopulation DefaultOverallPopulation = OverallPopulation.Normal;

		public const OverallTemperature DefaultOverallTemperature = OverallTemperature.Normal;

		public const LandmarkDensity DefaultLandmarkDensity = LandmarkDensity.Normal;

		public const float DefaultPollutionCoverage = 0.05f;

		private static List<GameSetupStepDef> cachedGenSteps;

		private static List<GameSetupStepDef> GameSetupStepsInOrder => cachedGenSteps ?? (cachedGenSteps = (from x in DefDatabase<GameSetupStepDef>.AllDefs
			orderby x.order, x.index
			select x).ToList());

		public static World GenerateWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature, OverallPopulation population, LandmarkDensity landmarkDensity, List<FactionDef> factions = null, float pollution = 0f)
		{
			DeepProfiler.Start("GenerateWorld");
			Rand.PushState();
			int seed = (Rand.Seed = GetSeedFromSeedString(seedString));
			try
			{
				Current.CreatingWorld = new World
				{
					info = 
					{
						seedString = seedString,
						planetCoverage = planetCoverage,
						overallRainfall = overallRainfall,
						overallTemperature = overallTemperature,
						overallPopulation = population,
						landmarkDensity = landmarkDensity,
						name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld),
						factions = factions,
						pollution = pollution
					}
				};
				foreach (GameSetupStepDef item in GameSetupStepsInOrder)
				{
					Rand.Seed = Gen.HashCombineInt(seed, item.setupStep.SeedPart);
					item.setupStep.GenerateFresh();
				}
				foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers.OrderBy((KeyValuePair<int, PlanetLayer> x) => x.Key))
				{
					GeneratePlanetLayer(planetLayer2, seedString, seed);
					if (planetLayer2.Tiles.Count == 0)
					{
						Log.Warning($"No tiles on layer {planetLayer2}, layer should have a world gen step worker which initializes tiles such as WorldGenStep_Tiles");
					}
				}
				Rand.Seed = seed;
				Current.CreatingWorld.grid.StandardizeTileData();
				Current.CreatingWorld.FinalizeInit(fromLoad: false);
				Find.Scenario.PostWorldGenerate();
				if (!ModsConfig.IdeologyActive)
				{
					Find.Scenario.PostIdeoChosen();
				}
				return Current.CreatingWorld;
			}
			finally
			{
				Rand.PopState();
				DeepProfiler.End();
				Current.CreatingWorld = null;
			}
		}

		public static void GeneratePlanetLayer(PlanetLayer layer, string seedString, int seed)
		{
			List<WorldGenStepDef> genStepsInOrder = layer.Def.GenStepsInOrder;
			DeepProfiler.Start($"WorldGen - {layer}");
			for (int i = 0; i < genStepsInOrder.Count; i++)
			{
				DeepProfiler.Start($"WorldGenStep - {genStepsInOrder[i]}");
				try
				{
					Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(genStepsInOrder, i));
					genStepsInOrder[i].worldGenStep.GenerateFresh(seedString, layer);
				}
				catch (Exception arg)
				{
					Log.Error($"Error in WorldGenStep: {arg}");
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void GenerateWithoutWorldData(string seedString)
		{
			int seedFromSeedString = GetSeedFromSeedString(seedString);
			Rand.PushState();
			foreach (GameSetupStepDef item in GameSetupStepsInOrder)
			{
				Rand.Seed = Gen.HashCombineInt(seedFromSeedString, item.setupStep.SeedPart);
				item.setupStep.GenerateWithoutWorldData();
			}
			foreach (KeyValuePair<int, PlanetLayer> item2 in Find.WorldGrid.PlanetLayers.OrderBy((KeyValuePair<int, PlanetLayer> x) => x.Key))
			{
				item2.Deconstruct(out var _, out var value);
				PlanetLayer planetLayer = value;
				List<WorldGenStepDef> genStepsInOrder = planetLayer.Def.GenStepsInOrder;
				for (int num = 0; num < genStepsInOrder.Count; num++)
				{
					try
					{
						Rand.Seed = Gen.HashCombineInt(seedFromSeedString, GetSeedPart(genStepsInOrder, num));
						genStepsInOrder[num].worldGenStep.GenerateWithoutWorldData(seedString, planetLayer);
					}
					catch (Exception arg)
					{
						Log.Error($"Error in WorldGenStep: {arg}");
					}
				}
			}
			Rand.PopState();
		}

		public static void GenerateFromScribe(string seedString)
		{
			int seedFromSeedString = GetSeedFromSeedString(seedString);
			Rand.PushState();
			foreach (GameSetupStepDef item in GameSetupStepsInOrder)
			{
				Rand.Seed = Gen.HashCombineInt(seedFromSeedString, item.setupStep.SeedPart);
				item.setupStep.GenerateFromScribe();
			}
			foreach (KeyValuePair<int, PlanetLayer> item2 in Find.WorldGrid.PlanetLayers.OrderBy((KeyValuePair<int, PlanetLayer> x) => x.Key))
			{
				item2.Deconstruct(out var _, out var value);
				PlanetLayer planetLayer = value;
				List<WorldGenStepDef> genStepsInOrder = planetLayer.Def.GenStepsInOrder;
				for (int num = 0; num < genStepsInOrder.Count; num++)
				{
					try
					{
						Rand.Seed = Gen.HashCombineInt(seedFromSeedString, GetSeedPart(genStepsInOrder, num));
						genStepsInOrder[num].worldGenStep.GenerateFromScribe(seedString, planetLayer);
					}
					catch (Exception arg)
					{
						Log.Error($"Error in WorldGenStep: {arg}");
					}
				}
			}
			Rand.PopState();
		}

		private static int GetSeedPart(List<WorldGenStepDef> genSteps, int index)
		{
			int seedPart = genSteps[index].worldGenStep.SeedPart;
			int num = 0;
			for (int i = 0; i < index; i++)
			{
				if (genSteps[i].worldGenStep.SeedPart == seedPart)
				{
					num++;
				}
			}
			return seedPart + num;
		}

		private static int GetSeedFromSeedString(string seedString)
		{
			return GenText.StableStringHash(seedString);
		}
	}
}
