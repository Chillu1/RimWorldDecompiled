using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class FocusStrengthOffset_BuildingDefsLit : FocusStrengthOffset_BuildingDefs
{
	public override bool CanApply(Thing parent, Pawn user = null)
	{
		if (BuildingLit(parent))
		{
			return base.CanApply(parent, user);
		}
		return false;
	}

	protected override float OffsetForBuilding(Thing b)
	{
		if (BuildingLit(b))
		{
			return OffsetFor(b.def);
		}
		return 0f;
	}

	private bool BuildingLit(Thing b)
	{
		return b.TryGetComp<CompGlower>()?.Glows ?? false;
	}

	protected override int BuildingCount(Thing parent)
	{
		if (parent == null || !parent.Spawned)
		{
			return 0;
		}
		int num = 0;
		List<Thing> forCell = parent.Map.listerBuldingOfDefInProximity.GetForCell(parent.Position, radius, defs, parent);
		for (int i = 0; i < forCell.Count; i++)
		{
			Thing b = forCell[i];
			if (BuildingLit(b))
			{
				num++;
			}
		}
		return Math.Min(num, maxBuildings);
	}
}
