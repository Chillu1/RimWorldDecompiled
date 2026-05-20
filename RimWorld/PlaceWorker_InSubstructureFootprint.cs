using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_InSubstructureFootprint : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		SubstructureGrid.DrawSubstructureFootprint();
	}

	public override void DrawOnGUIExtra(BuildableDef def)
	{
		SubstructureGrid.DrawSubstructureCountOnGUI();
	}

	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		RoofDef roofDef = map.roofGrid.RoofAt(loc);
		if (roofDef != null && roofDef.isNatural)
		{
			return "MessageCannotPlaceUnderRockRoof".Translate();
		}
		if (!GravshipUtility.InsideFootprint(loc, map))
		{
			return "MessageMustBePlacedInRangeOfGravEngineOrExtender".Translate();
		}
		return AcceptanceReport.WasAccepted;
	}

	public override void DrawPlaceMouseAttachments(float curX, ref float curY, BuildableDef bdef, IntVec3 center, Rot4 rot)
	{
		if (center.InBounds(Find.CurrentMap) && !center.Fogged(Find.CurrentMap))
		{
			GUI.color = ColorLibrary.RedReadable;
			RoofDef roofDef = Find.CurrentMap.roofGrid.RoofAt(center);
			if (roofDef != null && roofDef.isNatural)
			{
				PlaceWorker.DrawTextLine(curX, ref curY, "MessageCannotPlaceUnderRockRoof".Translate());
			}
			if (!GravshipUtility.InsideFootprint(center, Find.CurrentMap))
			{
				PlaceWorker.DrawTextLine(curX, ref curY, "MessageMustBePlacedInRangeOfGravEngineOrExtender".Translate());
			}
			GUI.color = Color.white;
		}
	}

	public override bool ForceAllowPlaceOver(BuildableDef otherDef)
	{
		if (otherDef is ThingDef { Fillage: FillCategory.Full })
		{
			return true;
		}
		return base.ForceAllowPlaceOver(otherDef);
	}
}
