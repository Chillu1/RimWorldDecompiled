using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_AncientSites : WorldGenStep
	{
		public FloatRange ancientSitesPer100kTiles;

		public override int SeedPart => 976238715;

		public override void GenerateFresh(string seed)
		{
			GenerateAncientSites();
		}

		private void GenerateAncientSites()
		{
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * ancientSitesPer100kTiles.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				Find.World.genData.ancientSites.Add(TileFinder.RandomSettlementTileFor(null));
			}
		}
	}
}
