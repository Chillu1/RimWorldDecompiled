using Verse;

namespace RimWorld.Planet
{
	public class WorldInfo : IExposable
	{
		public string name = "DefaultWorldName";

		public float planetCoverage;

		public string seedString = "SeedError";

		public int persistentRandomValue = Rand.Int;

		public OverallRainfall overallRainfall = OverallRainfall.Normal;

		public OverallTemperature overallTemperature = OverallTemperature.Normal;

		public OverallPopulation overallPopulation = OverallPopulation.Normal;

		public IntVec3 initialMapSize = new IntVec3(250, 1, 250);

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
			BackCompatibility.PostExposeData(this);
		}
	}
}
