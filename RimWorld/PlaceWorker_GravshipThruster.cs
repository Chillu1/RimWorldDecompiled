using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_GravshipThruster : PlaceWorker
{
	private static List<IntVec3> exclusionCells = new List<IntVec3>();

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		CompProperties_GravshipThruster compProperties = def.GetCompProperties<CompProperties_GravshipThruster>();
		if (compProperties != null)
		{
			compProperties.GetExclusionZone(center, rot, ref exclusionCells);
			GenDraw.DrawFieldEdges(exclusionCells);
		}
	}

	public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
	{
		if (CompGravshipThruster.IsBlocked(def as ThingDef, map, loc, rot, out var blockedBy, out var blockedBySubstructure))
		{
			if (blockedBy != null)
			{
				Messages.Message("WarningThrusterBlocked".Translate(def.LabelCap, blockedBy.Label), MessageTypeDefOf.CautionInput, historical: false);
			}
			else if (blockedBySubstructure)
			{
				Messages.Message("WarningThrusterBlocked".Translate(def.LabelCap, TerrainDefOf.Substructure.label), MessageTypeDefOf.CautionInput, historical: false);
			}
		}
		else if (!CompGravshipThruster.IsOutdoors(def as ThingDef, map, loc, rot))
		{
			Messages.Message("WarningThrusterInside".Translate(def.LabelCap), MessageTypeDefOf.CautionInput, historical: false);
		}
	}
}
