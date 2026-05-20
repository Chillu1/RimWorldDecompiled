using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public static class DragAndDropWidget
{
	private struct DraggableGroup
	{
		public Action<object, Vector2> extraDraggedItemOnGUI;
	}

	private struct DropAreaInstance
	{
		public int groupID;

		public Rect rect;

		public Rect absRect;

		public Action<object> onDrop;

		public object context;
	}

	private struct DraggableInstance
	{
		public int groupID;

		public Rect rect;

		public Rect absRect;

		public object context;

		public Action clickHandler;

		public Action onStartDragging;
	}

	private static List<DraggableGroup> groups = new List<DraggableGroup>();

	private static List<DropAreaInstance> dropAreas = new List<DropAreaInstance>();

	private static List<DraggableInstance> draggables = new List<DraggableInstance>();

	private static int draggingDraggable = -1;

	private static Vector2 dragStartPos;

	private static bool mouseIsDown;

	private static bool clicked;

	private static bool released;

	private static bool dragBegun;

	private static Vector2 clickedAt;

	private static Rect clickedInRect;

	private static int lastFrameDraggableCount = -1;

	private const float MinMouseMoveToHighlightDraggable = 5f;

	private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.6f);

	private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.3f);

	private const float LineWidth = 2f;

	public static bool Dragging => dragBegun;

	public static void DragAndDropWidgetOnGUI_BeforeWindowStack()
	{
		if (dragBegun && draggingDraggable >= 0 && draggingDraggable < draggables.Count)
		{
			int groupID = draggables[draggingDraggable].groupID;
			if (groupID >= 0 && groupID < groups.Count && groups[groupID].extraDraggedItemOnGUI != null)
			{
				groups[groupID].extraDraggedItemOnGUI(draggables[draggingDraggable].context, dragStartPos);
			}
		}
	}

	public static void DragAndDropWidgetOnGUI_AfterWindowStack()
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
			for (int i = 0; i < draggables.Count; i++)
			{
				if (draggables[i].rect == clickedInRect)
				{
					draggingDraggable = i;
					draggables[i].onStartDragging?.Invoke();
					dragStartPos = Event.current.mousePosition;
					break;
				}
			}
			mouseIsDown = true;
			clicked = false;
		}
		if (draggingDraggable >= draggables.Count)
		{
			StopDragging();
		}
		if (draggables.Count != lastFrameDraggableCount)
		{
			StopDragging();
		}
		if (released)
		{
			released = false;
			if (!dragBegun && mouseIsDown)
			{
				foreach (DraggableInstance draggable in draggables)
				{
					Rect absRect = draggable.absRect;
					if (absRect.Contains(Event.current.mousePosition) && draggable.clickHandler != null)
					{
						draggable.clickHandler();
					}
				}
			}
			mouseIsDown = false;
			if (dragBegun && draggingDraggable >= 0)
			{
				DraggableInstance draggableInstance = draggables[draggingDraggable];
				DropAreaInstance? dropAreaInstance = null;
				for (int num = dropAreas.Count - 1; num >= 0; num--)
				{
					DropAreaInstance value = dropAreas[num];
					if (draggableInstance.groupID == value.groupID && value.absRect.Contains(Event.current.mousePosition))
					{
						dropAreaInstance = value;
					}
				}
				if (dropAreaInstance.HasValue)
				{
					dropAreaInstance.Value.onDrop?.Invoke(draggableInstance.context);
				}
				else
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
			}
			StopDragging();
		}
		lastFrameDraggableCount = draggables.Count;
		groups.Clear();
		draggables.Clear();
		dropAreas.Clear();
	}

	public static int NewGroup(Action<object, Vector2> extraDraggedItemOnGUI = null)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return -1;
		}
		DraggableGroup item = new DraggableGroup
		{
			extraDraggedItemOnGUI = extraDraggedItemOnGUI
		};
		groups.Add(item);
		return groups.Count - 1;
	}

	public static bool Draggable(int groupID, Rect rect, object context, Action clickHandler = null, Action onStartDragging = null)
	{
		if (Event.current.type == EventType.Repaint)
		{
			DraggableInstance item = new DraggableInstance
			{
				groupID = groupID,
				rect = rect,
				context = context,
				clickHandler = clickHandler,
				onStartDragging = onStartDragging,
				absRect = new Rect(UI.GUIToScreenPoint(rect.position), rect.size)
			};
			draggables.Add(item);
			int num = draggables.Count - 1;
			if (draggingDraggable != -1 && (dragBegun || Vector2.Distance(clickedAt, Event.current.mousePosition) > 5f))
			{
				if (!dragBegun)
				{
					SoundDefOf.DragElement.PlayOneShotOnCamera();
					dragBegun = true;
				}
				if (draggingDraggable == num)
				{
					GUI.color = HighlightColor;
					Widgets.DrawHighlight(rect);
					GUI.color = Color.white;
				}
			}
			if (draggingDraggable == num)
			{
				return dragBegun;
			}
			return false;
		}
		if (Event.current.rawType == EventType.MouseUp)
		{
			released = true;
		}
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect))
		{
			clicked = true;
			clickedAt = Event.current.mousePosition;
			clickedInRect = rect;
		}
		return false;
	}

	public static void DropArea(int groupID, Rect rect, Action<object> onDrop, object context)
	{
		if (Event.current.type == EventType.Repaint)
		{
			DropAreaInstance item = new DropAreaInstance
			{
				groupID = groupID,
				rect = rect,
				onDrop = onDrop,
				absRect = new Rect(UI.GUIToScreenPoint(rect.position), rect.size),
				context = context
			};
			dropAreas.Add(item);
		}
	}

	public static object CurrentlyDraggedDraggable()
	{
		if (!dragBegun || draggingDraggable < 0)
		{
			return null;
		}
		return draggables[draggingDraggable].context;
	}

	public static object HoveringDropArea(int groupID)
	{
		DropAreaInstance? dropAreaInstance = null;
		for (int num = dropAreas.Count - 1; num >= 0; num--)
		{
			DropAreaInstance value = dropAreas[num];
			if (groupID == value.groupID && value.rect.Contains(Event.current.mousePosition))
			{
				dropAreaInstance = value;
			}
		}
		if (!dropAreaInstance.HasValue)
		{
			return null;
		}
		return dropAreaInstance.Value.context;
	}

	public static Rect? HoveringDropAreaRect(int groupID, Vector3? mousePos = null)
	{
		Vector3 point = mousePos ?? ((Vector3)Event.current.mousePosition);
		DropAreaInstance? dropAreaInstance = null;
		for (int num = dropAreas.Count - 1; num >= 0; num--)
		{
			DropAreaInstance value = dropAreas[num];
			if (groupID == value.groupID && value.rect.Contains(point))
			{
				dropAreaInstance = value;
			}
		}
		return dropAreaInstance?.rect;
	}

	public static object DraggableAt(int groupID, Vector3 mousePos)
	{
		DraggableInstance? draggableInstance = null;
		for (int num = draggables.Count - 1; num >= 0; num--)
		{
			DraggableInstance value = draggables[num];
			if (groupID == value.groupID && value.rect.Contains(mousePos))
			{
				draggableInstance = value;
			}
		}
		if (!draggableInstance.HasValue)
		{
			return null;
		}
		return draggableInstance.Value.context;
	}

	private static object GetDraggable(int groupID, Vector3 mousePosAbs, int direction)
	{
		float num = float.PositiveInfinity;
		DraggableInstance? draggableInstance = null;
		for (int num2 = draggables.Count - 1; num2 >= 0; num2--)
		{
			DraggableInstance value = draggables[num2];
			if (groupID == value.groupID)
			{
				Rect absRect = value.absRect;
				if (mousePosAbs.y >= absRect.yMin && mousePosAbs.y <= absRect.yMax)
				{
					float num3 = (mousePosAbs.x - absRect.xMax) * (float)direction;
					if (!(num3 < 0f) && num3 < num)
					{
						num = num3;
						draggableInstance = value;
					}
				}
			}
		}
		if (!draggableInstance.HasValue)
		{
			return null;
		}
		return draggableInstance.Value.context;
	}

	public static object GetDraggableBefore(int groupID, Vector3 mousePosAbs)
	{
		return GetDraggable(groupID, mousePosAbs, 1);
	}

	public static object GetDraggableAfter(int groupID, Vector3 mousePosAbs)
	{
		return GetDraggable(groupID, mousePosAbs, -1);
	}

	private static void StopDragging()
	{
		draggingDraggable = -1;
		dragStartPos = default(Vector2);
		dragBegun = false;
	}
}
