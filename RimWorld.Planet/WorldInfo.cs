using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldInfo : IExposable
{
	public string name = "DefaultWorldName";

	public float planetCoverage;

	public string seedString = "SeedError";

	public int persistentRandomValue = Rand.Int;

	public OverallRainfall overallRainfall = OverallRainfall.Normal;

	public OverallTemperature overallTemperature = OverallTemperature.Normal;

	public OverallPopulation overallPopulation = OverallPopulation.Normal;

	public LandmarkDensity landmarkDensity = LandmarkDensity.Normal;

	public IntVec3 initialMapSize = new IntVec3(250, 1, 250);

	public List<FactionDef> factions;

	public float pollution;

	public string FileNameNoExtension => GenText.CapitalizedNoSpaces(name);

	public int Seed => GenText.StableStringHash(seedString);

	public void ExposeData()
	{
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref planetCoverage, "planetCoverage", 0f);
		Scribe_Values.Look(ref seedString, "seedString");
		Scribe_Values.Look(ref persistentRandomValue, "persistentRandomValue", 0);
		Scribe_Values.Look(ref overallRainfall, "overallRainfall", OverallRainfall.AlmostNone);
		Scribe_Values.Look(ref overallTemperature, "overallTemperature", OverallTemperature.VeryCold);
		Scribe_Values.Look(ref initialMapSize, "initialMapSize");
		Scribe_Values.Look(ref pollution, "pollution", 0f);
		Scribe_Collections.Look(ref factions, "factions", LookMode.Def);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			Dictionary<FactionDef, int> dict = null;
			Scribe_Collections.Look(ref dict, "factionCounts", LookMode.Def, LookMode.Value);
			if (dict != null)
			{
				factions = new List<FactionDef>();
				foreach (KeyValuePair<FactionDef, int> item in dict)
				{
					if (item.Value > 0)
					{
						for (int i = 0; i < item.Value; i++)
						{
							factions.Add(item.Key);
						}
					}
				}
			}
		}
		BackCompatibility.PostExposeData(this);
	}
}
