using Verse;

namespace RimWorld;

public class GenStep_ScaleElevation : GenStep
{
	public float factor = 1f;

	public override int SeedPart => 253986;

	public override void Generate(Map map, GenStepParams parms)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			elevation[allCell] *= factor;
		}
	}
}
