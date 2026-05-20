using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ThingSelectionUtility
{
	private static HashSet<Thing> yieldedThings = new HashSet<Thing>();

	private static readonly HashSet<Plan> yieldedPlans = new HashSet<Plan>();

	private static readonly HashSet<Zone> yieldedZones = new HashSet<Zone>();

	private static readonly List<Pawn> tmpColonists = new List<Pawn>();

	public static bool SelectableByMapClick(Thing t)
	{
		if (!t.def.selectable)
		{
			return false;
		}
		if (t is Pawn pawn && pawn.IsHiddenFromPlayer())
		{
			return false;
		}
		Thing spawnedParentOrMe = t.SpawnedParentOrMe;
		if (spawnedParentOrMe == null)
		{
			return false;
		}
		if (spawnedParentOrMe.def.size.x == 1 && spawnedParentOrMe.def.size.z == 1)
		{
			return !spawnedParentOrMe.Position.Fogged(spawnedParentOrMe.Map);
		}
		foreach (IntVec3 item in spawnedParentOrMe.OccupiedRect())
		{
			if (!item.Fogged(spawnedParentOrMe.Map))
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
				for (int k = 0; k < cellThings.Count; k++)
				{
					Thing t = cellThings[k];
					if (SelectableByMapClick(t) && !t.def.neverMultiSelect && !yieldedThings.Contains(t))
					{
						yield return t;
						yieldedThings.Add(t);
					}
				}
			}
			Rect rectInWorldSpace = GetRectInWorldSpace(rect);
			foreach (IntVec3 edgeCell in mapRect.ExpandedBy(1).EdgeCells)
			{
				if (!edgeCell.InBounds(Find.CurrentMap) || edgeCell.GetItemCount(Find.CurrentMap) <= 1)
				{
					continue;
				}
				foreach (Thing t in Find.CurrentMap.thingGrid.ThingsAt(edgeCell))
				{
					if (t.def.category == ThingCategory.Item && SelectableByMapClick(t) && !t.def.neverMultiSelect && !yieldedThings.Contains(t))
					{
						Vector3 vector = t.TrueCenter();
						if (new Rect(vector.x - 0.5f, vector.z - 0.5f, 1f, 1f).Overlaps(rectInWorldSpace))
						{
							yield return t;
							yieldedThings.Add(t);
						}
					}
				}
			}
		}
		finally
		{
			yieldedThings.Clear();
		}
	}

	private static Rect GetRectInWorldSpace(Rect rect)
	{
		Vector2 screenLoc = new Vector2(rect.x, (float)UI.screenHeight - rect.y);
		Vector2 screenLoc2 = new Vector2(rect.x + rect.width, (float)UI.screenHeight - (rect.y + rect.height));
		Vector3 vector = UI.UIToMapPosition(screenLoc);
		Vector3 vector2 = UI.UIToMapPosition(screenLoc2);
		return new Rect(vector.x, vector2.z, vector2.x - vector.x, vector.z - vector2.z);
	}

	public static IEnumerable<Plan> MultiSelectablePlansInScreenRectDistinct(Rect rect, ColorDef match = null)
	{
		CellRect mapRect = GetMapRect(rect);
		yieldedPlans.Clear();
		try
		{
			foreach (IntVec3 item in mapRect)
			{
				if (item.InBounds(Find.CurrentMap))
				{
					Plan plan = item.GetPlan(Find.CurrentMap);
					if (plan != null && (match == null || plan.Color == match) && !yieldedPlans.Contains(plan))
					{
						yield return plan;
						yieldedPlans.Add(plan);
					}
				}
			}
		}
		finally
		{
			yieldedPlans.Clear();
		}
	}

	public static IEnumerable<Zone> MultiSelectableZonesInScreenRectDistinct(Rect rect, Zone matchType = null)
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
					if (zone != null && zone.IsMultiselectable && (matchType == null || !(zone.GetType() != matchType.GetType())) && !yieldedZones.Contains(zone))
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
		return new CellRect
		{
			minX = Mathf.FloorToInt(vector.x),
			minZ = Mathf.FloorToInt(vector2.z),
			maxX = Mathf.FloorToInt(vector2.x),
			maxZ = Mathf.FloorToInt(vector.z)
		};
	}

	public static void SelectNextColonist()
	{
		tmpColonists.Clear();
		tmpColonists.AddRange(Find.ColonistBar.GetColonistsInOrder().Where(SelectableByHotkey));
		if (tmpColonists.Count == 0)
		{
			return;
		}
		bool worldSelected = WorldRendererUtility.WorldSelected;
		int num = -1;
		for (int num2 = tmpColonists.Count - 1; num2 >= 0; num2--)
		{
			if ((!worldSelected && Find.Selector.IsSelected(tmpColonists[num2])) || (worldSelected && tmpColonists[num2].IsCaravanMember() && Find.WorldSelector.IsSelected(tmpColonists[num2].GetCaravan())))
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
		bool worldSelected = WorldRendererUtility.WorldSelected;
		int num = -1;
		for (int i = 0; i < tmpColonists.Count; i++)
		{
			if ((!worldSelected && Find.Selector.IsSelected(tmpColonists[i])) || (worldSelected && tmpColonists[i].IsCaravanMember() && Find.WorldSelector.IsSelected(tmpColonists[i].GetCaravan())))
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
