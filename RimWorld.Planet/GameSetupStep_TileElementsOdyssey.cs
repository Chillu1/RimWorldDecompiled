using Verse;

namespace RimWorld.Planet;

public class GameSetupStep_TileElementsOdyssey : GameSetupStep
{
	public override int SeedPart => 689393;

	public override void GenerateFresh()
	{
		Find.World.landmarks = new WorldLandmarks();
	}
}
