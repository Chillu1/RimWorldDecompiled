using RimWorld.Planet;

namespace Verse;

public abstract class WorldGenStep
{
	public WorldGenStepDef def;

	public abstract int SeedPart { get; }

	public abstract void GenerateFresh(string seed, PlanetLayer layer);

	public virtual void GenerateWithoutWorldData(string seed, PlanetLayer layer)
	{
		GenerateFresh(seed, layer);
	}

	public virtual void GenerateFromScribe(string seed, PlanetLayer layer)
	{
	}
}
