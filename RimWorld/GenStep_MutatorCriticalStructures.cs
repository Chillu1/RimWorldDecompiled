using Verse;

namespace RimWorld;

public class GenStep_MutatorCriticalStructures : GenStep
{
	public override int SeedPart => 651234136;

	public override void Generate(Map map, GenStepParams parms)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			mutator.Worker?.GenerateCriticalStructures(map);
		}
	}
}
