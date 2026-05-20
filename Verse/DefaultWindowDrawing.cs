using UnityEngine;

namespace Verse;

public class DefaultWindowDrawing : IWindowDrawing
{
	public GUIStyle EmptyStyle => Widgets.EmptyStyle;

	public bool DoCloseButton(Rect rect, string text)
	{
		return Widgets.ButtonText(rect, text);
	}

	public bool DoClostButtonSmall(Rect rect)
	{
		return Widgets.CloseButtonFor(rect);
	}

	public void DoGrayOut(Rect rect)
	{
		Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.5f));
	}

	public void DoWindowBackground(Rect rect)
	{
		Widgets.DrawWindowBackground(rect);
	}

	public void BeginGroup(Rect rect)
	{
		Widgets.BeginGroup(rect);
	}

	public void EndGroup()
	{
		Widgets.EndGroup();
	}
}
