using Verse;

namespace RimWorld;

public class GenStep_MutatorPostTerrain : GenStep
{
	public override int SeedPart => 562343345;

	public override void Generate(Map map, GenStepParams parms)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			mutator.Worker?.GeneratePostTerrain(map);
		}
	}
}
