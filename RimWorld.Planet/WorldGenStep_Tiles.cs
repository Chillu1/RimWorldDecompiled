using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Tiles : WorldGenStep
{
	public override int SeedPart => 16315992;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		layer.Tiles.Clear();
		for (int i = 0; i < layer.TilesCount; i++)
		{
			Tile item = GenerateTileFor(new PlanetTile(i, layer), layer);
			layer.Tiles.Add(item);
		}
	}

	protected virtual Tile GenerateTileFor(PlanetTile tile, PlanetLayer layer)
	{
		return new Tile(tile)
		{
			PrimaryBiome = tile.LayerDef.DefaultBiome,
			elevation = tile.Layer.Radius
		};
	}
}
