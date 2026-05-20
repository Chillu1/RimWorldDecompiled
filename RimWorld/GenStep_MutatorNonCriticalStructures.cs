using Verse;

namespace RimWorld;

public class GenStep_MutatorNonCriticalStructures : GenStep
{
	public override int SeedPart => 125123443;

	public override void Generate(Map map, GenStepParams parms)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			mutator.Worker?.GenerateNonCriticalStructures(map);
		}
	}
}
