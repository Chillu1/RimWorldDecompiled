using System;
using UnityEngine;

namespace Verse
{
	public class DebugTool
	{
		private string label;

		private Action clickAction;

		private Action onGUIAction;

		public DebugTool(string label, Action clickAction, Action onGUIAction = null)
		{
			this.label = label;
			this.clickAction = clickAction;
			this.onGUIAction = onGUIAction;
		}

		public DebugTool(string label, Action clickAction, IntVec3 firstRectCorner)
		{
			this.label = label;
			this.clickAction = clickAction;
			onGUIAction = delegate
			{
				IntVec3 intVec = UI.MouseCell();
				Vector3 v = firstRectCorner.ToVector3Shifted();
				Vector3 v2 = intVec.ToVector3Shifted();
				if (v.x < v2.x)
				{
					v.x -= 0.5f;
					v2.x += 0.5f;
				}
				else
				{
					v.x += 0.5f;
					v2.x -= 0.5f;
				}
				if (v.z < v2.z)
				{
					v.z -= 0.5f;
					v2.z += 0.5f;
				}
				else
				{
					v.z += 0.5f;
					v2.z -= 0.5f;
				}
				Vector2 vector = v.MapToUIPosition();
				Vector2 vector2 = v2.MapToUIPosition();
				Widgets.DrawBox(new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y), 3);
			};
		}

		public void DebugToolOnGUI()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					clickAction();
				}
				if (Event.current.button == 1)
				{
					DebugTools.curTool = null;
				}
				Event.current.Use();
			}
			Vector2 vector = Event.current.mousePosition + new Vector2(15f, 15f);
			Rect rect = new Rect(vector.x, vector.y, 999f, 999f);
			Text.Font = GameFont.Small;
			Widgets.Label(rect, label);
			if (onGUIAction != null)
			{
				onGUIAction();
			}
		}
	}
}
