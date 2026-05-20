using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public class GameSetupStep_Grids : GameSetupStep
{
	public override int SeedPart => 83469557;

	public override void GenerateFresh()
	{
		LongEventHandler.ExecuteWhenFinished(WorldTerrainColliderManager.ClearCache);
		Find.World.grid = new WorldGrid();
		Find.World.pathGrid = new WorldPathGrid();
		NoiseDebugUI.ClearPlanetNoises();
		Find.WorldGrid.GenerateFreshWorld();
	}

	public override void GenerateFromScribe()
	{
		LongEventHandler.ExecuteWhenFinished(WorldTerrainColliderManager.ClearCache);
		Find.World.pathGrid = new WorldPathGrid();
		NoiseDebugUI.ClearPlanetNoises();
	}
}
