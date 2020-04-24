using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldGenerator
	{
		private static List<WorldGenStepDef> tmpGenSteps = new List<WorldGenStepDef>();

		public const float DefaultPlanetCoverage = 0.3f;

		public const OverallRainfall DefaultOverallRainfall = OverallRainfall.Normal;

		public const OverallPopulation DefaultOverallPopulation = OverallPopulation.Normal;

		public const OverallTemperature DefaultOverallTemperature = OverallTemperature.Normal;

		public static IEnumerable<WorldGenStepDef> GenStepsInOrder => from x in DefDatabase<WorldGenStepDef>.AllDefs
			orderby x.order, x.index
			select x;

		public static World GenerateWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature, OverallPopulation population)
		{
			DeepProfiler.Start("GenerateWorld");
			Rand.PushState();
			int seed = Rand.Seed = GetSeedFromSeedString(seedString);
			try
			{
				Current.CreatingWorld = new World();
				Current.CreatingWorld.info.seedString = seedString;
				Current.CreatingWorld.info.planetCoverage = planetCoverage;
				Current.CreatingWorld.info.overallRainfall = overallRainfall;
				Current.CreatingWorld.info.overallTemperature = overallTemperature;
				Current.CreatingWorld.info.overallPopulation = population;
				Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld);
				tmpGenSteps.Clear();
				tmpGenSteps.AddRange(GenStepsInOrder);
				for (int i = 0; i < tmpGenSteps.Count; i++)
				{
					DeepProfiler.Start("WorldGenStep - " + tmpGenSteps[i]);
					try
					{
						Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(tmpGenSteps, i));
						tmpGenSteps[i].worldGenStep.GenerateFresh(seedString);
					}
					catch (Exception arg)
					{
						Log.Error("Error in WorldGenStep: " + arg);
					}
					finally
					{
						DeepProfiler.End();
					}
				}
				Rand.Seed = seed;
				Current.CreatingWorld.grid.StandardizeTileData();
				Current.CreatingWorld.FinalizeInit();
				Find.Scenario.PostWorldGenerate();
				return Current.CreatingWorld;
			}
			finally
			{
				Rand.PopState();
				DeepProfiler.End();
				Current.CreatingWorld = null;
			}
		}

		public static void GenerateWithoutWorldData(string seedString)
		{
			int seedFromSeedString = GetSeedFromSeedString(seedString);
			tmpGenSteps.Clear();
			tmpGenSteps.AddRange(GenStepsInOrder);
			Rand.PushState();
			for (int i = 0; i < tmpGenSteps.Count; i++)
			{
				try
				{
					Rand.Seed = Gen.HashCombineInt(seedFromSeedString, GetSeedPart(tmpGenSteps, i));
					tmpGenSteps[i].worldGenStep.GenerateWithoutWorldData(seedString);
				}
				catch (Exception arg)
				{
					Log.Error("Error in WorldGenStep: " + arg);
				}
			}
			Rand.PopState();
		}

		public static void GenerateFromScribe(string seedString)
		{
			int seedFromSeedString = GetSeedFromSeedString(seedString);
			tmpGenSteps.Clear();
			tmpGenSteps.AddRange(GenStepsInOrder);
			Rand.PushState();
			for (int i = 0; i < tmpGenSteps.Count; i++)
			{
				try
				{
					Rand.Seed = Gen.HashCombineInt(seedFromSeedString, GetSeedPart(tmpGenSteps, i));
					tmpGenSteps[i].worldGenStep.GenerateFromScribe(seedString);
				}
				catch (Exception arg)
				{
					Log.Error("Error in WorldGenStep: " + arg);
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
				if (tmpGenSteps[i].worldGenStep.SeedPart == seedPart)
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
