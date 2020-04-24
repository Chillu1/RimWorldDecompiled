using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterRuinsSimple : GenStep_Scatterer
	{
		private static readonly SimpleCurve RuinSizeChanceCurve = new SimpleCurve
		{
			new CurvePoint(6f, 0f),
			new CurvePoint(6.001f, 4f),
			new CurvePoint(10f, 1f),
			new CurvePoint(30f, 0f)
		};

		private int randomSize;

		public override int SeedPart => 1348417666;

		protected override bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			randomSize = Mathf.RoundToInt(Rand.ByCurve(RuinSizeChanceCurve));
			return base.TryFindScatterCell(map, out result);
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			CellRect rect = new CellRect(c.x, c.z, randomSize, randomSize).ClipInsideMap(map);
			if (!CanPlaceAncientBuildingInRange(rect, map))
			{
				return false;
			}
			return true;
		}

		protected bool CanPlaceAncientBuildingInRange(CellRect rect, Map map)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				if (cell.InBounds(map))
				{
					TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
					if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
					{
						return false;
					}
					if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, cell, map, Rot4.North))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
		{
			CellRect rect = new CellRect(c.x, c.z, randomSize, randomSize).ClipInsideMap(map);
			if (CanPlaceAncientBuildingInRange(rect, map))
			{
				ResolveParams parms2 = default(ResolveParams);
				parms2.sketch = new Sketch();
				parms2.monumentSize = new IntVec2(rect.Width, rect.Height);
				RimWorld.SketchGen.SketchGen.Generate(SketchResolverDefOf.MonumentRuin, parms2).Spawn(map, rect.CenterCell, null);
			}
		}
	}
}
