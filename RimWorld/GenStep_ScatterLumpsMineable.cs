using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterLumpsMineable : GenStep_Scatterer
{
	public ThingDef forcedDefToScatter;

	public int forcedLumpSize;

	public float maxValue = float.MaxValue;

	public bool useNomadicMineables;

	[Unsaved(false)]
	protected readonly List<IntVec3> recentLumpCells = new List<IntVec3>();

	public override int SeedPart => 920906419;

	public override void Generate(Map map, GenStepParams parms)
	{
		minSpacing = 5f;
		warnOnFail = false;
		int num = CalculateFinalCount(map);
		for (int i = 0; i < num; i++)
		{
			if (!TryFindScatterCell(map, out var result))
			{
				return;
			}
			ScatterAt(result, map, parms);
			usedSpots.Add(result);
		}
		usedSpots.Clear();
	}

	protected override int CalculateFinalCount(Map map)
	{
		if (count >= 0)
		{
			return Mathf.RoundToInt((float)count * GetPlacementFactor(map));
		}
		float num = countPer10kCellsRange.RandomInRange;
		if (!map.IsStartingMap && useNomadicMineables)
		{
			num *= Current.Game.storyteller.difficulty.nomadicMineableResourcesFactor;
		}
		return Mathf.RoundToInt((float)GenStep_Scatterer.CountFromPer10kCells(num, map) * GetPlacementFactor(map));
	}

	protected virtual ThingDef ChooseThingDef()
	{
		if (forcedDefToScatter != null)
		{
			return forcedDefToScatter;
		}
		return DefDatabase<ThingDef>.AllDefs.RandomElementByWeightWithFallback(delegate(ThingDef d)
		{
			if (d.building == null)
			{
				return 0f;
			}
			return (d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > maxValue) ? 0f : d.building.mineableScatterCommonality;
		});
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (NearUsedSpot(c, CalculateFinalMinSpacing(map)))
		{
			return false;
		}
		Building edifice = c.GetEdifice(map);
		if (edifice == null || !edifice.def.building.isNaturalRock)
		{
			return false;
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		if (Current.ProgramState == ProgramState.MapInitializing && Find.Storyteller.def.tutorialMode)
		{
			return;
		}
		ThingDef thingDef = ChooseThingDef();
		if (thingDef == null)
		{
			return;
		}
		int numCells = ((forcedLumpSize > 0) ? forcedLumpSize : thingDef.building.mineableScatterLumpSizeRange.RandomInRange);
		recentLumpCells.Clear();
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(c, map, numCells, Validator))
		{
			GenSpawn.Spawn(thingDef, item, map);
			recentLumpCells.Add(item);
		}
		bool Validator(IntVec3 cell)
		{
			if (!usedRects.Any((CellRect x) => x.Contains(cell)))
			{
				if (Current.ProgramState == ProgramState.MapInitializing)
				{
					return MapGenerator.Caves[cell] == 0f;
				}
				return true;
			}
			return false;
		}
	}
}
