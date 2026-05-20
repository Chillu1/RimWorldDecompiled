using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Factions : WorldGenStep
{
	public override int SeedPart => 777998381;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		FactionGenerator.GenerateFactionsIntoWorldLayer(layer, Current.CreatingWorld.info.factions);
	}

	public override void GenerateWithoutWorldData(string seed, PlanetLayer layer)
	{
	}
}
