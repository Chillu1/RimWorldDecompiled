using UnityEngine;

namespace Verse
{
	public static class DragSliderManager
	{
		private static bool dragging = false;

		private static float rootX;

		private static float lastRateFactor = 1f;

		private static DragSliderCallback draggingUpdateMethod;

		private static DragSliderCallback completedMethod;

		public static void ForceStop()
		{
			dragging = false;
		}

		public static bool DragSlider(Rect rect, float rateFactor, DragSliderCallback newStartMethod, DragSliderCallback newDraggingUpdateMethod, DragSliderCallback newCompletedMethod)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect))
			{
				lastRateFactor = rateFactor;
				newStartMethod(0f, rateFactor);
				StartDragSliding(newDraggingUpdateMethod, newCompletedMethod);
				return true;
			}
			return false;
		}

		private static void StartDragSliding(DragSliderCallback newDraggingUpdateMethod, DragSliderCallback newCompletedMethod)
		{
			dragging = true;
			draggingUpdateMethod = newDraggingUpdateMethod;
			completedMethod = newCompletedMethod;
			rootX = UI.MousePositionOnUI.x;
		}

		private static float CurMouseOffset()
		{
			return UI.MousePositionOnUI.x - rootX;
		}

		public static void DragSlidersOnGUI()
		{
			if (dragging && Event.current.type == EventType.MouseUp && Event.current.button == 0)
			{
				dragging = false;
				if (completedMethod != null)
				{
					completedMethod(CurMouseOffset(), lastRateFactor);
				}
			}
		}

		public static void DragSlidersUpdate()
		{
			if (dragging && draggingUpdateMethod != null)
			{
				draggingUpdateMethod(CurMouseOffset(), lastRateFactor);
			}
		}
	}
}
