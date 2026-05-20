using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class ThingOverlays
{
	public void ThingOverlaysOnGUI()
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
		List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.HasGUIOverlay);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (currentViewRect.Contains(thing.Position) && !Find.CurrentMap.fogGrid.IsFogged(thing.Position))
			{
				try
				{
					thing.DrawGUIOverlay();
				}
				catch (Exception ex)
				{
					Log.Error("Exception drawing ThingOverlay for " + thing?.ToString() + ": " + ex);
				}
			}
		}
	}
}
