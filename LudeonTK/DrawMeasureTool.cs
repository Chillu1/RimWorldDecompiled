using System;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class DrawMeasureTool : MeasureTool
{
	public DrawMeasureTool(string label, Action clickAction, Action onGUIAction = null)
	{
		base.label = label;
		base.clickAction = clickAction;
		base.onGUIAction = onGUIAction;
	}

	public DrawMeasureTool(string label, Action clickAction, Vector3 firstRectCorner)
	{
		base.label = label;
		base.clickAction = clickAction;
		onGUIAction = delegate
		{
			Vector3 v = UI.MouseMapPosition();
			Vector2 start = firstRectCorner.MapToUIPosition();
			Vector2 end = v.MapToUIPosition();
			DevGUI.DrawLine(start, end, Color.white, 0.25f);
		};
	}
}
