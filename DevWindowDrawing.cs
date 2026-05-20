using LudeonTK;
using UnityEngine;
using Verse;

public class DevWindowDrawing : IWindowDrawing
{
	public GUIStyle EmptyStyle => DevGUI.EmptyStyle;

	public bool DoCloseButton(Rect rect, string text)
	{
		return DevGUI.ButtonText(rect, text);
	}

	public bool DoClostButtonSmall(Rect rect)
	{
		return DevGUI.ButtonImage(new Rect(rect.x + rect.width - 22f - 4f, rect.y + 4f, 22f, 22f), DevGUI.Close);
	}

	public void DoGrayOut(Rect rect)
	{
		DevGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.5f));
	}

	public void DoWindowBackground(Rect rect)
	{
		GUI.color = DevGUI.WindowBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = Color.white;
	}

	public void BeginGroup(Rect rect)
	{
		DevGUI.BeginGroup(rect);
	}

	public void EndGroup()
	{
		DevGUI.EndGroup();
	}
}
