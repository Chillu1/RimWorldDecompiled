using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualPosition_Copilot : RitualPosition_Cells
{
	public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
	{
		thing.TryGetComp(out CompPilotConsole comp);
		IntVec3 interactionCell = thing.InteractionCell;
		IntVec3[] adjacentCells = GenAdj.AdjacentCells;
		foreach (IntVec3 intVec in adjacentCells)
		{
			IntVec3 intVec2 = interactionCell + intVec;
			if (ReachabilityImmediate.CanReachImmediate(intVec2, thing, thing.Map, PathEndMode.Touch, null) && comp.engine.ValidSubstructureAt(intVec2))
			{
				cells.Add(intVec2);
			}
		}
	}

	protected override IntVec3 GetFallbackSpot(CellRect rect, IntVec3 spot, Pawn p, LordJob_Ritual ritual, Func<IntVec3, bool> Validator)
	{
		CompPilotConsole pilotConsole = null;
		Thing thing = ritual.selectedTarget.Thing;
		if (thing == null || !thing.TryGetComp(out pilotConsole))
		{
			return IntVec3.Invalid;
		}
		if (!CellFinder.TryFindRandomCellNear(spot, p.Map, 20, (IntVec3 c) => Validator(c) && pilotConsole.engine.ValidSubstructureAt(c), out var result))
		{
			return IntVec3.Invalid;
		}
		return result;
	}
}
