using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public static class ReorderableWidget
{
	private struct ReorderableGroup
	{
		public Action<int, int> reorderedAction;

		public ReorderableDirection direction;

		public float drawLineExactlyBetween_space;

		public Action<int, Vector2> extraDraggedItemOnGUI;

		public Rect absRect;

		public bool playSoundOnStartReorder;

		public bool DrawLineExactlyBetween => drawLineExactlyBetween_space > 0f;
	}

	private struct ReorderableMultiGroup
	{
		public Action<int, int, int, int> reorderedAction;

		public List<int> includedGroups;
	}

	private struct ReorderableInstance
	{
		public int groupID;

		public Rect rect;

		public Rect absRect;
	}

	private static List<ReorderableGroup> groups = new List<ReorderableGroup>();

	private static List<ReorderableMultiGroup> multiGroups = new List<ReorderableMultiGroup>();

	private static List<ReorderableInstance> reorderables = new List<ReorderableInstance>();

	private static int draggingReorderable = -1;

	private static Vector2 dragStartPos;

	private static bool clicked;

	private static bool released;

	private static bool dragBegun;

	private static Vector2 clickedAt;

	private static int groupClicked;

	private static Rect clickedInRect;

	private static int lastInsertNear = -1;

	private static int hoveredGroup = -1;

	private static bool lastInsertNearLeft;

	private static int lastFrameReorderableCount = -1;

	private const float MinMouseMoveToHighlightReorderable = 5f;

	private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.6f);

	private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.3f);

	private const float LineWidth = 2f;

	public static bool Dragging => dragBegun;

	public static int GetDraggedIndex => GetIndexWithinGroup(draggingReorderable);

	public static int GetDraggedFromGroupID => reorderables[draggingReorderable].groupID;

	public static void ReorderableWidgetOnGUI_BeforeWindowStack()
	{
		if (dragBegun && draggingReorderable >= 0 && draggingReorderable < reorderables.Count)
		{
			int groupID = reorderables[draggingReorderable].groupID;
			if (groupID >= 0 && groupID < groups.Count && groups[groupID].extraDraggedItemOnGUI != null)
			{
				groups[groupID].extraDraggedItemOnGUI(GetIndexWithinGroup(draggingReorderable), dragStartPos);
			}
		}
	}

	public static void ReorderableWidgetOnGUI_AfterWindowStack()
	{
		if (Event.current.rawType == EventType.MouseUp)
		{
			released = true;
		}
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (clicked)
		{
			StopDragging();
			for (int i = 0; i < reorderables.Count; i++)
			{
				if (reorderables[i].groupID == groupClicked && reorderables[i].rect == clickedInRect)
				{
					draggingReorderable = i;
					dragStartPos = Event.current.mousePosition;
					break;
				}
			}
			clicked = false;
		}
		if (draggingReorderable >= reorderables.Count)
		{
			StopDragging();
		}
		if (reorderables.Count != lastFrameReorderableCount)
		{
			StopDragging();
		}
		lastInsertNear = CurrentInsertNear(out lastInsertNearLeft);
		hoveredGroup = -1;
		for (int j = 0; j < groups.Count; j++)
		{
			if (groups[j].absRect.Contains(Event.current.mousePosition))
			{
				hoveredGroup = j;
				if (lastInsertNear >= 0 && AreInMultiGroup(j, reorderables[lastInsertNear].groupID) && reorderables[lastInsertNear].groupID != j)
				{
					lastInsertNear = FindLastReorderableIndexWithinGroup(j);
					lastInsertNearLeft = lastInsertNear < 0;
				}
			}
		}
		if (released)
		{
			released = false;
			if (dragBegun && draggingReorderable >= 0)
			{
				int indexWithinGroup = GetIndexWithinGroup(draggingReorderable);
				int groupID = reorderables[draggingReorderable].groupID;
				int num = ((lastInsertNear == draggingReorderable) ? indexWithinGroup : ((!lastInsertNearLeft) ? (GetIndexWithinGroup(lastInsertNear) + 1) : GetIndexWithinGroup(lastInsertNear)));
				int num2 = -1;
				if (lastInsertNear >= 0)
				{
					num2 = reorderables[lastInsertNear].groupID;
				}
				if (AreInMultiGroup(groupID, hoveredGroup) && hoveredGroup >= 0 && hoveredGroup != num2)
				{
					num2 = hoveredGroup;
					num = GetIndexWithinGroup(FindLastReorderableIndexWithinGroup(num2)) + 1;
				}
				if (AreInMultiGroup(groupID, num2))
				{
					GetMultiGroupByGroupID(groupID).Value.reorderedAction(indexWithinGroup, groupID, num, num2);
					SoundDefOf.DropElement.PlayOneShotOnCamera();
				}
				else if (num >= 0 && num != indexWithinGroup && num != indexWithinGroup + 1)
				{
					SoundDefOf.DropElement.PlayOneShotOnCamera();
					try
					{
						groups[reorderables[draggingReorderable].groupID].reorderedAction(indexWithinGroup, num);
					}
					catch (Exception ex)
					{
						Log.Error("Could not reorder elements (from " + indexWithinGroup + " to " + num + "): " + ex);
					}
				}
			}
			StopDragging();
		}
		lastFrameReorderableCount = reorderables.Count;
		multiGroups.Clear();
		groups.Clear();
		reorderables.Clear();
	}

	public static int NewGroup(Action<int, int> reorderedAction, ReorderableDirection direction, Rect rect, float drawLineExactlyBetween_space = -1f, Action<int, Vector2> extraDraggedItemOnGUI = null, bool playSoundOnStartReorder = true)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return -1;
		}
		int count = groups.Count;
		ReorderableGroup item = new ReorderableGroup
		{
			reorderedAction = reorderedAction,
			direction = direction,
			absRect = new Rect(UI.GUIToScreenPoint(Vector2.zero), rect.size),
			drawLineExactlyBetween_space = drawLineExactlyBetween_space,
			extraDraggedItemOnGUI = extraDraggedItemOnGUI,
			playSoundOnStartReorder = playSoundOnStartReorder
		};
		groups.Add(item);
		if (draggingReorderable >= 0 && hoveredGroup == count && lastInsertNear == -1)
		{
			DrawLine(count, new Rect(0f, 0f, rect.width, rect.height));
		}
		return count;
	}

	public static int NewMultiGroup(List<int> includedGroups, Action<int, int, int, int> reorderedAction)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return -1;
		}
		ReorderableMultiGroup item = new ReorderableMultiGroup
		{
			includedGroups = includedGroups,
			reorderedAction = reorderedAction
		};
		multiGroups.Add(item);
		return multiGroups.Count - 1;
	}

	public static bool Reorderable(int groupID, Rect rect, bool useRightButton = false, bool highlightDragged = true)
	{
		if (Event.current.type == EventType.Repaint)
		{
			ReorderableInstance item = new ReorderableInstance
			{
				groupID = groupID,
				rect = rect,
				absRect = new Rect(UI.GUIToScreenPoint(rect.position), rect.size)
			};
			reorderables.Add(item);
			int num = reorderables.Count - 1;
			if (draggingReorderable != -1 && (dragBegun || (Vector2.Distance(clickedAt, Event.current.mousePosition) > 5f && groupClicked == groupID)))
			{
				if (!dragBegun)
				{
					if (groupID >= 0 && groupID < groups.Count && groups[groupID].playSoundOnStartReorder)
					{
						SoundDefOf.DragElement.PlayOneShotOnCamera();
					}
					dragBegun = true;
				}
				if (highlightDragged && draggingReorderable == num)
				{
					GUI.color = HighlightColor;
					Widgets.DrawHighlight(rect);
					GUI.color = Color.white;
				}
				if (lastInsertNear == num && groupID >= 0 && groupID < groups.Count)
				{
					Rect rect2 = reorderables[lastInsertNear].rect;
					DrawLine(groupID, rect2);
				}
			}
			if (draggingReorderable == num)
			{
				return dragBegun;
			}
			return false;
		}
		if (Event.current.rawType == EventType.MouseUp)
		{
			released = true;
		}
		if (Event.current.type == EventType.MouseDown && ((useRightButton && Event.current.button == 1) || (!useRightButton && Event.current.button == 0)) && Mouse.IsOver(rect))
		{
			clicked = true;
			clickedAt = Event.current.mousePosition;
			groupClicked = groupID;
			clickedInRect = rect;
		}
		return false;
	}

	private static int CurrentInsertNear(out bool toTheLeft)
	{
		toTheLeft = false;
		if (draggingReorderable < 0)
		{
			return -1;
		}
		int groupID = reorderables[draggingReorderable].groupID;
		if (groupID < 0 || groupID >= groups.Count)
		{
			Log.ErrorOnce("Reorderable used invalid group.", 1968375560);
			return -1;
		}
		int num = -1;
		for (int i = 0; i < reorderables.Count; i++)
		{
			ReorderableInstance reorderableInstance = reorderables[i];
			if ((reorderableInstance.groupID == groupID || AreInMultiGroup(reorderableInstance.groupID, groupID)) && (num == -1 || Event.current.mousePosition.DistanceToRect(reorderableInstance.absRect) < Event.current.mousePosition.DistanceToRect(reorderables[num].absRect)))
			{
				num = i;
			}
		}
		if (num >= 0)
		{
			ReorderableInstance reorderableInstance2 = reorderables[num];
			if (groups[reorderableInstance2.groupID].direction == ReorderableDirection.Horizontal)
			{
				toTheLeft = Event.current.mousePosition.x < reorderableInstance2.absRect.center.x;
			}
			else
			{
				toTheLeft = Event.current.mousePosition.y < reorderableInstance2.absRect.center.y;
			}
		}
		return num;
	}

	private static void DrawLine(int groupID, Rect r)
	{
		ReorderableGroup reorderableGroup = groups[groupID];
		if (reorderableGroup.DrawLineExactlyBetween)
		{
			if (reorderableGroup.direction == ReorderableDirection.Horizontal)
			{
				r.xMin -= reorderableGroup.drawLineExactlyBetween_space / 2f;
				r.xMax += reorderableGroup.drawLineExactlyBetween_space / 2f;
			}
			else
			{
				r.yMin -= reorderableGroup.drawLineExactlyBetween_space / 2f;
				r.yMax += reorderableGroup.drawLineExactlyBetween_space / 2f;
			}
		}
		GUI.color = LineColor;
		if (reorderableGroup.direction == ReorderableDirection.Horizontal)
		{
			if (lastInsertNearLeft)
			{
				Widgets.DrawLine(r.position, new Vector2(r.x, r.yMax), LineColor, 2f);
			}
			else
			{
				Widgets.DrawLine(new Vector2(r.xMax, r.y), new Vector2(r.xMax, r.yMax), LineColor, 2f);
			}
		}
		else if (lastInsertNearLeft)
		{
			Widgets.DrawLine(r.position, new Vector2(r.xMax, r.y), LineColor, 2f);
		}
		else
		{
			Widgets.DrawLine(new Vector2(r.x, r.yMax), new Vector2(r.xMax, r.yMax), LineColor, 2f);
		}
		GUI.color = Color.white;
	}

	private static int GetIndexWithinGroup(int index)
	{
		if (index < 0 || index >= reorderables.Count)
		{
			return -1;
		}
		int num = -1;
		for (int i = 0; i <= index; i++)
		{
			if (reorderables[i].groupID == reorderables[index].groupID)
			{
				num++;
			}
		}
		return num;
	}

	private static int FindLastReorderableIndexWithinGroup(int groupID)
	{
		if (groupID < 0 || groupID >= groups.Count)
		{
			return -1;
		}
		int result = -1;
		for (int i = 0; i < reorderables.Count; i++)
		{
			if (reorderables[i].groupID == groupID)
			{
				result = i;
			}
		}
		return result;
	}

	private static ReorderableMultiGroup? GetMultiGroupByGroupID(int groupID)
	{
		foreach (ReorderableMultiGroup multiGroup in multiGroups)
		{
			if (multiGroup.includedGroups.Contains(groupID))
			{
				return multiGroup;
			}
		}
		return null;
	}

	private static bool AreInMultiGroup(int groupA, int groupB)
	{
		ReorderableMultiGroup? multiGroupByGroupID = GetMultiGroupByGroupID(groupA);
		if (multiGroupByGroupID.HasValue && groupA != groupB)
		{
			return multiGroupByGroupID.Value.includedGroups.Contains(groupB);
		}
		return false;
	}

	private static void StopDragging()
	{
		draggingReorderable = -1;
		dragStartPos = default(Vector2);
		lastInsertNear = -1;
		dragBegun = false;
	}
}
