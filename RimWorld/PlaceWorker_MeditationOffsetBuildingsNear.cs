using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_MeditationOffsetBuildingsNear : PlaceWorker
{
	public static readonly Color RingColor = new Color(0.5f, 0.8f, 0.5f);

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		if (!ModsConfig.RoyaltyActive)
		{
			return;
		}
		FocusStrengthOffset_BuildingDefs focusStrengthOffset_BuildingDefs = ((CompProperties_MeditationFocus)((def.IsFrame || def.IsBlueprint) ? ((ThingDef)def.entityDefToBuild).CompDefFor<CompMeditationFocus>() : def.CompDefFor<CompMeditationFocus>())).offsets.OfType<FocusStrengthOffset_BuildingDefs>().FirstOrDefault();
		if (focusStrengthOffset_BuildingDefs == null)
		{
			return;
		}
		if (focusStrengthOffset_BuildingDefs.drawRingRadius)
		{
			GenDraw.DrawRadiusRing(center, focusStrengthOffset_BuildingDefs.radius, RingColor);
		}
		List<Thing> forCell = Find.CurrentMap.listerBuldingOfDefInProximity.GetForCell(center, focusStrengthOffset_BuildingDefs.radius, focusStrengthOffset_BuildingDefs.defs);
		for (int i = 0; i < forCell.Count && i < focusStrengthOffset_BuildingDefs.maxBuildings; i++)
		{
			if (thing != forCell[i])
			{
				GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), forCell[i].TrueCenter(), SimpleColor.Green);
			}
		}
	}
}
