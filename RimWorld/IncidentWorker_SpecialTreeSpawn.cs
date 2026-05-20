using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_SpecialTreeSpawn : IncidentWorker
{
	private GenStep_SpecialTrees genStep;

	protected GenStep_SpecialTrees GenStep
	{
		get
		{
			if (genStep == null)
			{
				genStep = (GenStep_SpecialTrees)def.treeGenStepDef.genStep;
			}
			return genStep;
		}
	}

	protected virtual int MaxTreesPerIncident => 1;

	protected virtual bool SendLetter => true;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (map == null)
		{
			return false;
		}
		if (!parms.bypassStorytellerSettings && map.Biome.isExtremeBiome)
		{
			return false;
		}
		int num = GenStep.DesiredTreeCountForMap(map);
		if (map.listerThings.ThingsOfDef(def.treeDef).Count >= num)
		{
			return false;
		}
		IntVec3 cell;
		return TryFindRootCell(map, out cell);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		int num = Math.Min(MaxTreesPerIncident, GenStep.DesiredTreeCountForMap(map) - map.listerThings.ThingsOfDef(def.treeDef).Count);
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < num; i++)
		{
			if (!TryFindRootCell(map, out var cell))
			{
				break;
			}
			if (!GenStep.TrySpawnAt(cell, map, def.treeGrowth, out var plant))
			{
				break;
			}
			list.Add(plant);
		}
		if (list.NullOrEmpty())
		{
			return false;
		}
		if (SendLetter)
		{
			SendStandardLetter(def.letterLabel, (list.Count > 1) ? def.letterTextPlural : def.letterText, def.letterDef, parms, list);
		}
		return true;
	}

	protected virtual bool TryFindRootCell(Map map, out IntVec3 cell)
	{
		if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep.CanSpawnAt(x, map), map, out cell))
		{
			return true;
		}
		return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep.CanSpawnAt(x, map, 10, 0, 0f, 18, 20), map, out cell);
	}
}
