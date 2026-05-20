using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace LudeonTK;

public abstract class EditWindow : Window_Dev
{
	private const float SuperimposeAvoidThreshold = 8f;

	private const float SuperimposeAvoidOffset = 16f;

	private const float SuperimposeAvoidOffsetMinEdge = 200f;

	public override Vector2 InitialSize => new Vector2(500f, 500f);

	protected override float Margin => 8f;

	public EditWindow()
	{
		resizeable = true;
		draggable = true;
		preventCameraMotion = false;
		doCloseX = true;
		windowRect.x = 5f;
		windowRect.y = 5f;
	}

	protected void DoRowButton(ref float x, float y, string text, string tooltip, Action action)
	{
		Vector2 vector = Text.CalcSize(text);
		Rect rect = new Rect(x, y, vector.x + 10f, 24f);
		if (DevGUI.ButtonText(rect, text))
		{
			action();
		}
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		x += rect.width + 4f;
	}

	protected void DoImageToggle(ref float x, float y, Texture2D texture, string tooltip, ref bool toggle)
	{
		Rect rect = new Rect(x, y, 24f, 24f);
		DevGUI.CheckboxImage(rect, texture, ref toggle);
		TooltipHandler.TipRegion(rect, tooltip);
		x += 28f;
	}

	public override void PostOpen()
	{
		while (!(windowRect.x > (float)UI.screenWidth - 200f) && !(windowRect.y > (float)UI.screenHeight - 200f))
		{
			bool flag = false;
			foreach (EditWindow item in Find.WindowStack.Windows.Where((Window di) => di is EditWindow).Cast<EditWindow>())
			{
				if (item != this && Mathf.Abs(item.windowRect.x - windowRect.x) < 8f && Mathf.Abs(item.windowRect.y - windowRect.y) < 8f)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				windowRect.x += 16f;
				windowRect.y += 16f;
				continue;
			}
			break;
		}
	}
}
