using System;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class MeasureTool
{
	public string label;

	public Action clickAction;

	public Action onGUIAction;

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
				DebugTools.curMeasureTool = null;
			}
			Event.current.Use();
		}
		Vector2 vector = Event.current.mousePosition + new Vector2(15f, 15f);
		Rect rect = new Rect(vector.x, vector.y, 999f, 999f);
		Text.Font = GameFont.Small;
		DevGUI.Label(rect, label);
		if (onGUIAction != null)
		{
			onGUIAction();
		}
	}
}
