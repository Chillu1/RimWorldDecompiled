using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_ElectricInhibitor : PlaceWorker
{
	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		tmpCells.Clear();
		tmpCells.AddRange(ContainmentUtility.GetInhibitorAffectedCells(def, center, rot, Find.CurrentMap));
		GenDraw.DrawFieldEdges(tmpCells, Color.white);
		tmpCells.Clear();
	}

	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (checkingDef is ThingDef def && ContainmentUtility.IsLinearBuildingBlocked(def, loc, rot, map, thingToIgnore))
		{
			return new AcceptanceReport("InhibitorSpaceOccupied".Translate());
		}
		return AcceptanceReport.WasAccepted;
	}
}
