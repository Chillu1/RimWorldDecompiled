using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace LudeonTK;

public abstract class Dialog_DebugOptionLister : Dialog_OptionLister
{
	protected int currentHighlightIndex;

	protected int prioritizedHighlightedIndex;

	private const float DebugOptionsGap = 7f;

	protected virtual int HighlightedIndex => -1;

	public Dialog_DebugOptionLister()
	{
		forcePause = true;
	}

	public void NewColumn(float columnWidth)
	{
		curY = 0f;
		curX += columnWidth + 17f;
	}

	protected void NewColumnIfNeeded(float columnWidth, float neededHeight)
	{
		if (curY + neededHeight > windowRect.height)
		{
			NewColumn(columnWidth);
		}
	}

	public bool ButtonDebug(string label, float columnWidth, bool highlight)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(columnWidth, 22f);
		Rect rect = new Rect(curX, curY, columnWidth, 22f);
		bool result = false;
		if (!base.BoundingRectCached.HasValue || rect.Overlaps(base.BoundingRectCached.Value))
		{
			bool wordWrap = Text.WordWrap;
			Text.WordWrap = false;
			result = DevGUI.ButtonText(rect, "  " + label, TextAnchor.MiddleLeft);
			Text.WordWrap = wordWrap;
			if (highlight)
			{
				GUI.color = Color.yellow;
				DevGUI.DrawBox(rect, 2);
				GUI.color = Color.white;
			}
		}
		curY += 22f + verticalSpacing;
		return result;
	}

	protected void DebugLabel(string label, float columnWidth)
	{
		Text.Font = GameFont.Small;
		float num = Text.CalcHeight(label, columnWidth);
		NewColumnIfNeeded(columnWidth, num);
		DevGUI.Label(new Rect(curX, curY, columnWidth, num), label);
		curY += num + verticalSpacing;
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += 24f;
		}
	}

	protected bool DebugAction(string label, float columnWidth, Action action, bool highlight)
	{
		bool result = false;
		if (!FilterAllows(label))
		{
			return false;
		}
		if (ButtonDebug(label, columnWidth, highlight))
		{
			Close();
			action();
			result = true;
		}
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += 24f;
		}
		return result;
	}

	protected void DebugToolMap(string label, float columnWidth, Action toolAction, bool highlight)
	{
		if (!WorldRendererUtility.WorldSelected)
		{
			if (!FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (ButtonDebug(label, columnWidth, highlight))
			{
				Close();
				DebugTools.curTool = new DebugTool(label, toolAction);
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 24f;
			}
		}
	}

	protected virtual void ChangeHighlightedOption()
	{
	}
}
