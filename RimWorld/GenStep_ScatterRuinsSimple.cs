using System.Collections.Generic;
using RimWorld.SketchGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterRuinsSimple : GenStep_Scatterer
{
	private int randomSize;

	private SimpleCurve ruinSizeChanceCurve = new SimpleCurve
	{
		new CurvePoint(6f, 0f),
		new CurvePoint(6.001f, 4f),
		new CurvePoint(10f, 1f),
		new CurvePoint(30f, 0f)
	};

	private bool clearSurroundingArea;

	private float destroyChanceExp = 1.32f;

	private bool mustBeStandable;

	private bool canBeOnEdge;

	private bool ignoreUsedRects;

	private int usedRectsPadding = 2;

	public override int SeedPart => 1348417666;

	protected virtual CellRect EffectiveRectAt(IntVec3 c)
	{
		return CellRect.CenteredOn(c, randomSize, randomSize);
	}

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		randomSize = Mathf.RoundToInt(Rand.ByCurve(ruinSizeChanceCurve));
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
		if (mustBeStandable && !c.Standable(map))
		{
			return false;
		}
		if (!CanPlaceAncientBuildingInRange(EffectiveRectAt(c).ClipInsideMap(map), map))
		{
			return false;
		}
		if (!ignoreUsedRects && MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var))
		{
			CellRect cellRect = EffectiveRectAt(c);
			foreach (CellRect item in var)
			{
				if (cellRect.Overlaps(item.ExpandedBy(usedRectsPadding)))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected bool CanPlaceAncientBuildingInRange(CellRect rect, Map map)
	{
		if (MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Any((CellRect x) => x.Overlaps(rect)))
		{
			return false;
		}
		foreach (IntVec3 cell in rect.Cells)
		{
			if (!canBeOnEdge && !cell.InBounds(map))
			{
				return false;
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
			if (terrainDef.HasTag("River") || terrainDef.HasTag("Road"))
			{
				return false;
			}
			if (!GenConstruct.CanBuildOnTerrain(ThingDefOf.Wall, cell, map, Rot4.North))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			if (edifice != null && edifice.def.IsBuildingArtificial)
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		CellRect cellRect = EffectiveRectAt(c).ClipInsideMap(map);
		if (!CanPlaceAncientBuildingInRange(cellRect, map))
		{
			return;
		}
		RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			sketch = new Sketch(),
			monumentSize = new IntVec2(cellRect.Width, cellRect.Height),
			destroyChanceExp = destroyChanceExp
		}, root: SketchResolverDefOf.MonumentRuin).Spawn(map, cellRect.CenterCell, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: false, forceTerrainAffordance: false, clearEdificeWhereFloor: false, null, dormant: false, buildRoofsInstantly: false, delegate(SketchEntity entity, IntVec3 cell)
		{
			if (clearSurroundingArea)
			{
				IntVec3[] cardinalDirectionsAndInside = GenAdj.CardinalDirectionsAndInside;
				foreach (IntVec3 intVec in cardinalDirectionsAndInside)
				{
					if ((cell + intVec).InBounds(map))
					{
						Building edifice = (cell + intVec).GetEdifice(map);
						if (edifice != null && !edifice.Position.CloseToEdge(map, 3) && edifice.def.building.isNaturalRock)
						{
							edifice.Destroy();
						}
					}
				}
			}
			bool result = false;
			foreach (IntVec3 adjacentCell in entity.OccupiedRect.AdjacentCells)
			{
				IntVec3 c2 = cell + adjacentCell;
				if (c2.InBounds(map))
				{
					Building edifice2 = c2.GetEdifice(map);
					if (edifice2 == null || !edifice2.def.building.isNaturalRock)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		});
		if (!ignoreUsedRects)
		{
			orGenerateVar.Add(cellRect);
		}
	}
}
