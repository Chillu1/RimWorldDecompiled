using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDynamicDrawManager
	{
		private HashSet<WorldObject> drawObjects = new HashSet<WorldObject>();

		private bool drawingNow;

		public void RegisterDrawable(WorldObject o)
		{
			if (o.def.useDynamicDrawer)
			{
				if (drawingNow)
				{
					Log.Warning(string.Concat("Cannot register drawable ", o, " while drawing is in progress. WorldObjects shouldn't be spawned in Draw methods."));
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
					Log.Warning(string.Concat("Cannot deregister drawable ", o, " while drawing is in progress. WorldObjects shouldn't be despawned in Draw methods."));
				}
				drawObjects.Remove(o);
			}
		}

		public void DrawDynamicWorldObjects()
		{
			drawingNow = true;
			try
			{
				foreach (WorldObject drawObject in drawObjects)
				{
					try
					{
						if (!drawObject.def.expandingIcon || !(ExpandableWorldObjectsUtility.TransitionPct >= 1f))
						{
							drawObject.Draw();
						}
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat("Exception drawing ", drawObject, ": ", ex));
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Exception drawing dynamic world objects: " + arg);
			}
			drawingNow = false;
		}
	}
}
