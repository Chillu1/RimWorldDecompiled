using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_IceCaves : TileMutatorWorker_Caves
{
	public TileMutatorWorker_IceCaves(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostElevationFertility(Map map)
	{
	}

	public override void GeneratePostTerrain(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		GenerateCaves(map);
		MapGenFloatGrid caves = MapGenerator.Caves;
		List<IntVec3> list = new List<IntVec3>();
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!(caves[allCell] <= 0f))
			{
				Thing edifice = allCell.GetEdifice(map);
				if (edifice != null)
				{
					edifice.Destroy();
					list.Add(allCell);
				}
			}
		}
		RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
		base.GeneratePostTerrain(map);
	}

	protected override bool ShouldCarve(IntVec3 c, MapGenFloatGrid elevation, Map map)
	{
		if (c.InBounds(map))
		{
			return c.GetEdifice(map)?.def == ThingDefOf.SolidIce;
		}
		return false;
	}
}
