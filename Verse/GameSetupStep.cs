namespace Verse;

public abstract class GameSetupStep
{
	public GameSetupStepDef def;

	public abstract int SeedPart { get; }

	public abstract void GenerateFresh();

	public virtual void GenerateWithoutWorldData()
	{
		GenerateFresh();
	}

	public virtual void GenerateFromScribe()
	{
	}
}
