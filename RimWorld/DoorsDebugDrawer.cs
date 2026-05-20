using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class DoorsDebugDrawer
{
	public static void DrawDebug()
	{
		if (!DebugViewSettings.drawDoorsDebug)
		{
			return;
		}
		CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
		List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
		for (int i = 0; i < list.Count; i++)
		{
			if (!currentViewRect.Contains(list[i].Position) || !(list[i] is Building_Door building_Door))
			{
				continue;
			}
			Color col = ((!building_Door.FreePassage) ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f));
			foreach (IntVec3 item in building_Door.OccupiedRect())
			{
				CellRenderer.RenderCell(item, SolidColorMaterials.SimpleSolidColorMaterial(col));
			}
		}
	}
}
