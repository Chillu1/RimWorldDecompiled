using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_FogSpace : GenStep
{
	public override int SeedPart => 492371731;

	public override void Generate(Map map, GenStepParams parms)
	{
		map.fogGrid.SetAllFogged();
		foreach (IntVec3 corner in map.BoundsRect().Corners)
		{
			if (Validator(corner))
			{
				FloodFillerFog.FloodUnfog(corner, map);
			}
		}
		List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
		for (int i = 0; i < rootsToUnfog.Count; i++)
		{
			FloodFillerFog.FloodUnfog(rootsToUnfog[i], map);
		}
		bool Validator(IntVec3 c)
		{
			if (c.GetEdifice(map) != null)
			{
				return false;
			}
			if (c.Roofed(map))
			{
				return false;
			}
			return true;
		}
	}
}
