using Verse;

namespace RimWorld
{
	public class GenStep_Pollution : GenStep
	{
		private static readonly FloatRange LightPollutionCellRange = new FloatRange(0.1f, 0.3f);

		private static readonly FloatRange ModeratePollutionCellRange = new FloatRange(0.4f, 0.6f);

		private const int PollutionGlobSize = 1000;

		public override int SeedPart => 7593785;

		public override void Generate(Map map, GenStepParams parms)
		{
			PollutionUtility.PolluteMapToPercent(map, Find.WorldGrid[map.Tile].pollution);
		}
	}
}
