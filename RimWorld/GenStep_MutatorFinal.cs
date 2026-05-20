using Verse;

namespace RimWorld;

public class GenStep_MutatorFinal : GenStep
{
	public override int SeedPart => 756452345;

	public override void Generate(Map map, GenStepParams parms)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			mutator.Worker?.GeneratePostFog(map);
		}
	}
}
