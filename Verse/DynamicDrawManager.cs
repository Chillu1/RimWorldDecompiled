using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class DynamicDrawManager
	{
		private Map map;

		private HashSet<Thing> drawThings = new HashSet<Thing>();

		private bool drawingNow;

		public DynamicDrawManager(Map map)
		{
			this.map = map;
		}

		public void RegisterDrawable(Thing t)
		{
			if (t.def.drawerType != 0)
			{
				if (drawingNow)
				{
					Log.Warning(string.Concat("Cannot register drawable ", t, " while drawing is in progress. Things shouldn't be spawned in Draw methods."));
				}
				drawThings.Add(t);
			}
		}

		public void DeRegisterDrawable(Thing t)
		{
			if (t.def.drawerType != 0)
			{
				if (drawingNow)
				{
					Log.Warning(string.Concat("Cannot deregister drawable ", t, " while drawing is in progress. Things shouldn't be despawned in Draw methods."));
				}
				drawThings.Remove(t);
			}
		}

		public void DrawDynamicThings()
		{
			if (!DebugViewSettings.drawThingsDynamic)
			{
				return;
			}
			drawingNow = true;
			try
			{
				bool[] fogGrid = map.fogGrid.fogGrid;
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				currentViewRect.ClipInsideMap(map);
				currentViewRect = currentViewRect.ExpandedBy(1);
				CellIndices cellIndices = map.cellIndices;
				foreach (Thing drawThing in drawThings)
				{
					IntVec3 position = drawThing.Position;
					if ((currentViewRect.Contains(position) || drawThing.def.drawOffscreen) && (!fogGrid[cellIndices.CellToIndex(position)] || drawThing.def.seeThroughFog) && (!(drawThing.def.hideAtSnowDepth < 1f) || !(map.snowGrid.GetDepth(position) > drawThing.def.hideAtSnowDepth)))
					{
						try
						{
							drawThing.Draw();
						}
						catch (Exception ex)
						{
							Log.Error(string.Concat("Exception drawing ", drawThing, ": ", ex.ToString()));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Exception drawing dynamic things: " + arg);
			}
			drawingNow = false;
		}

		public void LogDynamicDrawThings()
		{
			Log.Message(DebugLogsUtility.ThingListToUniqueCountString(drawThings));
		}
	}
}
