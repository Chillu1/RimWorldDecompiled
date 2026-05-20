using Verse;

namespace RimWorld.Planet;

public class GameSetupStep_Components : GameSetupStep
{
	public override int SeedPart => 508565678;

	public override void GenerateFresh()
	{
		Find.World.ConstructComponents();
	}

	public override void GenerateWithoutWorldData()
	{
		GenerateFromScribe();
	}

	public override void GenerateFromScribe()
	{
		Find.World.ConstructComponents();
		Find.World.ExposeComponents();
	}
}
