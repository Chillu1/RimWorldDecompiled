using System.Collections.Generic;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterAncientUtilityBuilding : GenStep_Scatterer
{
	private static readonly SimpleCurve SizeChanceCurve = new SimpleCurve
	{
		new CurvePoint(8f, 0f),
		new CurvePoint(12f, 4f),
		new CurvePoint(18f, 0f)
	};

	private int randomSize;

	public override int SeedPart => 1872954345;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter ancient outdoor building"))
		{
			count = 1;
			allowInWaterBiome = false;
			randomSize = Mathf.RoundToInt(Rand.ByCurve(SizeChanceCurve));
			base.Generate(map, parms);
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		if (!loc.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
		{
			return false;
		}
		CellRect rect = new CellRect(loc.x, loc.z, randomSize, randomSize);
		if (!CanPlaceAt(rect, map))
		{
			return false;
		}
		return true;
	}

	private bool CanPlaceAt(CellRect rect, Map map)
	{
		if (MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Any((CellRect ur) => ur.Overlaps(rect)))
		{
			return false;
		}
		foreach (IntVec3 cell in rect.Cells)
		{
			if (!cell.InBounds(map))
			{
				return false;
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
			if (terrainDef.IsWater || terrainDef.IsRoad)
			{
				return false;
			}
			if (cell.GetEdifice(map) != null)
			{
				return false;
			}
			List<Thing> thingList = cell.GetThingList(map);
			for (int num = 0; num < thingList.Count; num++)
			{
				if (!thingList[num].def.destroyable)
				{
					return false;
				}
			}
			if (!cell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			utilityBuildingSize = new IntVec2(randomSize, randomSize),
			sketch = new Sketch()
		}, root: SketchResolverDefOf.AncientUtilityBuilding).Spawn(map, loc, null);
	}
}
