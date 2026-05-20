using Verse;

namespace RimWorld;

public class GenStep_MutatorPostElevationFertility : GenStep
{
	public override int SeedPart => 1239847543;

	public override void Generate(Map map, GenStepParams parms)
	{
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			mutator.Worker?.GeneratePostElevationFertility(map);
		}
	}
}
