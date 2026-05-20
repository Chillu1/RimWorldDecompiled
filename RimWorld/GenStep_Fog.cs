using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_Fog : GenStep
{
	public override int SeedPart => 1568957891;

	public override void Generate(Map map, GenStepParams parms)
	{
		DeepProfiler.Start("GenerateInitialFogGrid");
		map.fogGrid.SetAllFogged();
		if (MapGenerator.PlayerStartSpot.IsValid)
		{
			IntVec3 intVec = MapGenerator.PlayerStartSpot;
			Building edifice = intVec.GetEdifice(map);
			if (edifice != null && edifice.def.MakeFog)
			{
				intVec = CellFinder.StandableCellNear(intVec, map, 5f, (IntVec3 c) => !c.Roofed(map));
			}
			FloodFillerFog.FloodUnfog(intVec, map);
		}
		else
		{
			UnfogMapFromEdge(map);
		}
		List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
		for (int num = 0; num < rootsToUnfog.Count; num++)
		{
			FloodFillerFog.FloodUnfog(rootsToUnfog[num], map);
			map.fogGrid.Unfog(rootsToUnfog[num]);
		}
		DeepProfiler.End();
	}

	private static void UnfogMapFromEdge(Map map)
	{
		Predicate<IntVec3> validator = delegate(IntVec3 c)
		{
			if (!c.Standable(map))
			{
				return false;
			}
			if (c.Roofed(map))
			{
				return false;
			}
			return map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater)) ? true : false;
		};
		if (CellFinder.TryFindRandomCellNear(map.Center, map, 30, validator, out var result) || CellFinder.TryFindRandomEdgeCellWith(validator, map, 0f, out result) || CellFinder.TryFindRandomCell(map, validator, out result))
		{
			FloodFillerFog.FloodUnfog(result, map);
		}
	}
}
