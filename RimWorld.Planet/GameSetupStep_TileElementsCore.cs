using Verse;

namespace RimWorld.Planet;

public class GameSetupStep_TileElementsCore : GameSetupStep
{
	public override int SeedPart => 711240483;

	public override void GenerateFresh()
	{
		Find.World.features = new WorldFeatures();
	}
}
