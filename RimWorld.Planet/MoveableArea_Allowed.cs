using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class MoveableArea_Allowed : MoveableArea
{
	public List<Pawn> assignedPawns = new List<Pawn>();

	public MoveableArea_Allowed()
	{
	}

	public MoveableArea_Allowed(Gravship gravship, Area area)
		: base(gravship, area.Label, area.RenamableLabel, area.Color, area.ID)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref assignedPawns, "assignedPawns", LookMode.Reference);
	}

	public bool TryCreateArea(AreaManager areaManager, IntVec3 newOrigin)
	{
		if (relativeCells.Count == 0)
		{
			return false;
		}
		Area_Allowed area = areaManager.GetLabeled(label) as Area_Allowed;
		if (area == null && !areaManager.TryMakeNewAllowed(out area))
		{
			return area != null;
		}
		foreach (IntVec3 relativeCell in base.RelativeCells)
		{
			area[newOrigin + relativeCell] = true;
		}
		area.SetColor(color);
		area.SetLabel(label);
		area.RenamableLabel = renamableLabel;
		foreach (Pawn assignedPawn in assignedPawns)
		{
			assignedPawn.playerSettings.AreaRestrictionInPawnCurrentMap = area;
		}
		return true;
	}
}
