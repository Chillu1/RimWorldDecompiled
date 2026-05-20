using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldDynamicDrawManager
{
	private readonly HashSet<WorldObject> drawObjects = new HashSet<WorldObject>();

	private bool drawingNow;

	public void RegisterDrawable(WorldObject o)
	{
		if (o.def.useDynamicDrawer)
		{
			if (drawingNow)
			{
				Log.Warning("Cannot register drawable " + o?.ToString() + " while drawing is in progress. WorldObjects shouldn't be spawned in Draw methods.");
			}
			drawObjects.Add(o);
		}
	}

	public void DeRegisterDrawable(WorldObject o)
	{
		if (o.def.useDynamicDrawer)
		{
			if (drawingNow)
			{
				Log.Warning("Cannot deregister drawable " + o?.ToString() + " while drawing is in progress. WorldObjects shouldn't be despawned in Draw methods.");
			}
			drawObjects.Remove(o);
		}
	}

	public void DrawDynamicWorldObjects()
	{
		if (WorldRendererUtility.WorldBackgroundNow || !DebugViewSettings.drawWorldObjects)
		{
			return;
		}
		drawingNow = true;
		try
		{
			foreach (WorldObject drawObject in drawObjects)
			{
				try
				{
					if ((!drawObject.def.expandingIcon || !(ExpandableWorldObjectsUtility.TransitionPct(drawObject) >= 1f)) && (!WorldRendererUtility.WorldBackgroundNow || drawObject.VisibleInBackground))
					{
						drawObject.Draw();
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception drawing " + drawObject?.ToString() + ": " + ex);
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Error("Exception drawing dynamic world objects: " + ex2);
		}
		drawingNow = false;
	}
}
