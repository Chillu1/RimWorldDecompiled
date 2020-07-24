using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSelectionUtility
	{
		private static HashSet<Thing> yieldedThings = new HashSet<Thing>();

		private static HashSet<Zone> yieldedZones = new HashSet<Zone>();

		private static List<Pawn> tmpColonists = new List<Pawn>();

		public static bool SelectableByMapClick(Thing t)
		{
			if (!t.def.selectable)
			{
				return false;
			}
			if (!t.Spawned)
			{
				return false;
			}
			if (t.def.size.x == 1 && t.def.size.z == 1)
			{
				return !t.Position.Fogged(t.Map);
			}
			foreach (IntVec3 item in t.OccupiedRect())
			{
				if (!item.Fogged(t.Map))
				{
					return true;
				}
			}
			return false;
		}

		public static bool SelectableByHotkey(Thing t)
		{
			if (t.def.selectable)
			{
				return t.Spawned;
			}
			return false;
		}

		public static IEnumerable<Thing> MultiSelectableThingsInScreenRectDistinct(Rect rect)
		{
			CellRect mapRect = GetMapRect(rect);
			yieldedThings.Clear();
			try
			{
				foreach (IntVec3 item in mapRect)
				{
					if (!item.InBounds(Find.CurrentMap))
					{
						continue;
					}
					List<Thing> cellThings = Find.CurrentMap.thingGrid.ThingsListAt(item);
					if (cellThings == null)
					{
						continue;
					}
					for (int i = 0; i < cellThings.Count; i++)
					{
						Thing t = cellThings[i];
						if (SelectableByMapClick(t) && !t.def.neverMultiSelect && !yieldedThings.Contains(t))
						{
							yield return t;
							yieldedThings.Add(t);
						}
					}
				}
			}
			finally
			{
				yieldedThings.Clear();
			}
		}

		public static IEnumerable<Zone> MultiSelectableZonesInScreenRectDistinct(Rect rect)
		{
			CellRect mapRect = GetMapRect(rect);
			yieldedZones.Clear();
			try
			{
				foreach (IntVec3 item in mapRect)
				{
					if (item.InBounds(Find.CurrentMap))
					{
						Zone zone = item.GetZone(Find.CurrentMap);
						if (zone != null && zone.IsMultiselectable && !yieldedZones.Contains(zone))
						{
							yield return zone;
							yieldedZones.Add(zone);
						}
					}
				}
			}
			finally
			{
				yieldedZones.Clear();
			}
		}

		private static CellRect GetMapRect(Rect rect)
		{
			Vector2 screenLoc = new Vector2(rect.x, (float)UI.screenHeight - rect.y);
			Vector2 screenLoc2 = new Vector2(rect.x + rect.width, (float)UI.screenHeight - (rect.y + rect.height));
			Vector3 vector = UI.UIToMapPosition(screenLoc);
			Vector3 vector2 = UI.UIToMapPosition(screenLoc2);
			CellRect result = default(CellRect);
			result.minX = Mathf.FloorToInt(vector.x);
			result.minZ = Mathf.FloorToInt(vector2.z);
			result.maxX = Mathf.FloorToInt(vector2.x);
			result.maxZ = Mathf.FloorToInt(vector.z);
			return result;
		}

		public static void SelectNextColonist()
		{
			tmpColonists.Clear();
			tmpColonists.AddRange(Find.ColonistBar.GetColonistsInOrder().Where(SelectableByHotkey));
			if (tmpColonists.Count == 0)
			{
				return;
			}
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			int num = -1;
			for (int num2 = tmpColonists.Count - 1; num2 >= 0; num2--)
			{
				if ((!worldRenderedNow && Find.Selector.IsSelected(tmpColonists[num2])) || (worldRenderedNow && tmpColonists[num2].IsCaravanMember() && Find.WorldSelector.IsSelected(tmpColonists[num2].GetCaravan())))
				{
					num = num2;
					break;
				}
			}
			if (num == -1)
			{
				CameraJumper.TryJumpAndSelect(tmpColonists[0]);
			}
			else
			{
				CameraJumper.TryJumpAndSelect(tmpColonists[(num + 1) % tmpColonists.Count]);
			}
			tmpColonists.Clear();
		}

		public static void SelectPreviousColonist()
		{
			tmpColonists.Clear();
			tmpColonists.AddRange(Find.ColonistBar.GetColonistsInOrder().Where(SelectableByHotkey));
			if (tmpColonists.Count == 0)
			{
				return;
			}
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			int num = -1;
			for (int i = 0; i < tmpColonists.Count; i++)
			{
				if ((!worldRenderedNow && Find.Selector.IsSelected(tmpColonists[i])) || (worldRenderedNow && tmpColonists[i].IsCaravanMember() && Find.WorldSelector.IsSelected(tmpColonists[i].GetCaravan())))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				CameraJumper.TryJumpAndSelect(tmpColonists[tmpColonists.Count - 1]);
			}
			else
			{
				CameraJumper.TryJumpAndSelect(tmpColonists[GenMath.PositiveMod(num - 1, tmpColonists.Count)]);
			}
			tmpColonists.Clear();
		}
	}
}
