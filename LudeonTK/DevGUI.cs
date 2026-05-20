using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace LudeonTK;

[StaticConstructorOnStartup]
public static class DevGUI
{
	private static Stack<bool> mouseOverScrollViewStack;

	public static readonly GUIStyle EmptyStyle;

	public static readonly Color WindowBGFillColor;

	public const float CheckboxSize = 24f;

	private static Texture2D ButtonBackground;

	private static Texture2D ButtonBackgroundMouseover;

	public static Texture2D ButtonBackgroundClick;

	public static readonly Texture2D LightHighlight;

	private static readonly Texture2D PinTex;

	private static readonly Texture2D PinOutlineTex;

	public static readonly Texture2D CheckOn;

	public static readonly Texture2D CheckOff;

	public static readonly Texture2D InspectMode;

	public static readonly Texture2D Close;

	private static Texture2D LineTexAA;

	public const float CloseButtonSize = 22f;

	public const float CloseButtonMargin = 4f;

	static DevGUI()
	{
		mouseOverScrollViewStack = new Stack<bool>();
		EmptyStyle = new GUIStyle();
		WindowBGFillColor = new ColorInt(21, 25, 29).ToColor;
		ButtonBackground = null;
		ButtonBackgroundMouseover = null;
		ButtonBackgroundClick = null;
		LightHighlight = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.04f));
		PinTex = ContentFinder<Texture2D>.Get("UI/Developer/Pin");
		PinOutlineTex = ContentFinder<Texture2D>.Get("UI/Developer/Pin-Outline");
		CheckOn = ContentFinder<Texture2D>.Get("UI/Developer/CheckOn");
		CheckOff = ContentFinder<Texture2D>.Get("UI/Developer/CheckOff");
		InspectMode = ContentFinder<Texture2D>.Get("UI/Developer/InspectModeToggle");
		Close = ContentFinder<Texture2D>.Get("UI/Developer/Close");
		LineTexAA = null;
		Color color = new Color(1f, 1f, 1f, 0f);
		LineTexAA = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
		LineTexAA.name = "LineTexAA";
		LineTexAA.SetPixel(0, 0, color);
		LineTexAA.SetPixel(0, 1, Color.white);
		LineTexAA.SetPixel(0, 2, color);
		LineTexAA.Apply();
		ButtonBackground = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		ButtonBackground.name = "ButtonBGAtlas";
		ButtonBackground.SetPixel(0, 0, new Color32(65, 65, 65, byte.MaxValue));
		ButtonBackground.Apply();
		ButtonBackgroundMouseover = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		ButtonBackgroundMouseover.name = "ButtonBGAtlasMouseover";
		ButtonBackgroundMouseover.SetPixel(0, 0, new Color32(85, 85, 85, byte.MaxValue));
		ButtonBackgroundMouseover.Apply();
		ButtonBackgroundClick = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		ButtonBackgroundClick.name = "ButtonBGAtlasClick";
		ButtonBackgroundClick.SetPixel(0, 0, new Color32(45, 45, 45, byte.MaxValue));
		ButtonBackgroundClick.Apply();
	}

	public static void BeginGroup(Rect rect)
	{
		GUI.BeginGroup(rect);
		UnityGUIBugsFixer.Notify_BeginGroup();
	}

	public static void EndGroup()
	{
		GUI.EndGroup();
		UnityGUIBugsFixer.Notify_EndGroup();
	}

	public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
	{
		float num = end.x - start.x;
		float num2 = end.y - start.y;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		if (!(num3 < 0.01f))
		{
			width *= 3f;
			float num4 = width * num2 / num3;
			float num5 = width * num / num3;
			float z = (0f - Mathf.Atan2(0f - num2, num)) * 57.29578f;
			Vector2 vector = start + new Vector2(0.5f * num4, -0.5f * num5);
			Matrix4x4 m = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
			Rect position = new Rect(start.x, start.y - 0.5f * num5, num3, width);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GUI.DrawTexture(position, LineTexAA, ScaleMode.StretchToFill, alphaBlend: true, 0f, color, 0f, 0f);
			GL.PopMatrix();
		}
	}

	public static void DrawBox(Rect rect, int thickness = 1, Texture2D lineTexture = null)
	{
		Vector2 vector = new Vector2(rect.x, rect.y);
		Vector2 vector2 = new Vector2(rect.x + rect.width, rect.y + rect.height);
		if (vector.x > vector2.x)
		{
			float x = vector.x;
			vector.x = vector2.x;
			vector2.x = x;
		}
		if (vector.y > vector2.y)
		{
			float y = vector.y;
			vector.y = vector2.y;
			vector2.y = y;
		}
		Vector3 vector3 = vector2 - vector;
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x, vector.y, thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2.x - (float)thickness, vector.y, thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector.y, vector3.x - (float)(thickness * 2), thickness)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector2.y - (float)thickness, vector3.x - (float)(thickness * 2), thickness)), lineTexture ?? BaseContent.WhiteTex);
	}

	public static void Label(Rect rect, string label)
	{
		Rect position = rect;
		float num = Prefs.UIScale / 2f;
		if (Prefs.UIScale > 1f && Math.Abs(num - Mathf.Floor(num)) > float.Epsilon)
		{
			position = UIScaling.AdjustRectToUIScaling(rect);
		}
		GUI.Label(position, label, Text.CurFontStyle);
	}

	public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn)
	{
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleLeft;
		Label(rect, label);
		if (ButtonInvisible(rect))
		{
			checkOn = !checkOn;
			if (checkOn)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
		Checkbox(new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f), ref checkOn);
		Text.Anchor = anchor;
	}

	public static void CheckboxImage(Rect rect, Texture2D icon, ref bool checkOn)
	{
		if (ButtonImage(rect, icon))
		{
			checkOn = !checkOn;
			if (checkOn)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
		float num = rect.height / 2f;
		Checkbox(new Rect(rect.center.x, rect.yMin, num, num), ref checkOn);
	}

	public static void Checkbox(Rect rect, ref bool checkOn)
	{
		if (ButtonImage(rect, checkOn ? CheckOn : CheckOff))
		{
			checkOn = !checkOn;
			if (checkOn)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
	}

	public static bool ButtonText(Rect rect, string label, TextAnchor? overrideTextAnchor = null)
	{
		TextAnchor anchor = Text.Anchor;
		Color color = GUI.color;
		Texture2D image = ButtonBackground;
		if (Mouse.IsOver(rect))
		{
			image = ButtonBackgroundMouseover;
			if (Input.GetMouseButton(0))
			{
				image = ButtonBackgroundClick;
			}
		}
		GUI.DrawTexture(rect, image);
		if (overrideTextAnchor.HasValue)
		{
			Text.Anchor = overrideTextAnchor.Value;
		}
		else
		{
			Text.Anchor = TextAnchor.MiddleCenter;
		}
		bool wordWrap = Text.WordWrap;
		if (rect.height < Text.LineHeight * 2f)
		{
			Text.WordWrap = false;
		}
		GUI.color = Color.white;
		Label(rect, label);
		Text.Anchor = anchor;
		GUI.color = color;
		Text.WordWrap = wordWrap;
		return ButtonInvisible(rect);
	}

	public static void DrawRect(Rect position, Color color)
	{
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUI.Box(position, GUIContent.none, TexUI.FastFillStyle);
		GUI.backgroundColor = backgroundColor;
	}

	public static bool ButtonImage(Rect butRect, Texture2D tex)
	{
		if (Mouse.IsOver(butRect) && !ReorderableWidget.Dragging)
		{
			GUI.color = GenUI.MouseoverColor;
		}
		else
		{
			GUI.color = Color.white;
		}
		GUI.DrawTexture(butRect, tex);
		GUI.color = Color.white;
		return ButtonInvisible(butRect);
	}

	public static bool ButtonInvisible(Rect butRect)
	{
		MouseoverSounds.DoRegion(butRect);
		return GUI.Button(butRect, "", EmptyStyle);
	}

	public static string TextField(Rect rect, string text)
	{
		if (text == null)
		{
			text = "";
		}
		return GUI.TextField(rect, text, Text.CurTextFieldStyle);
	}

	public static void BeginScrollView(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, bool showScrollbars = true)
	{
		if (mouseOverScrollViewStack.Count > 0)
		{
			mouseOverScrollViewStack.Push(mouseOverScrollViewStack.Peek() && outRect.Contains(Event.current.mousePosition));
		}
		else
		{
			mouseOverScrollViewStack.Push(outRect.Contains(Event.current.mousePosition));
		}
		SteamDeck.HandleTouchScreenScrollViewScroll(outRect, ref scrollPosition);
		if (showScrollbars)
		{
			scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect);
		}
		else
		{
			scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect, GUIStyle.none, GUIStyle.none);
		}
		UnityGUIBugsFixer.Notify_BeginScrollView();
	}

	public static void EndScrollView()
	{
		mouseOverScrollViewStack.Pop();
		GUI.EndScrollView();
	}

	public static void DrawHighlightSelected(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.HighlightSelectedTex);
	}

	public static void DrawHighlightIfMouseover(Rect rect)
	{
		if (Mouse.IsOver(rect))
		{
			DrawHighlight(rect);
		}
	}

	public static void DrawHighlight(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.HighlightTex);
	}

	public static void DrawLightHighlight(Rect rect)
	{
		GUI.DrawTexture(rect, LightHighlight);
	}

	public static DebugActionButtonResult CheckboxPinnable(Rect rect, string label, ref bool checkOn, bool highlight, bool pinned)
	{
		new Rect(rect.x, rect.y, rect.width - rect.height, rect.height);
		DebugActionButtonResult result = DebugActionButtonResult.None;
		CheckboxLabeled(rect, label.Truncate(rect.width - 15f), ref checkOn);
		if (highlight)
		{
			GUI.color = Color.yellow;
			DrawBox(rect, 2);
			GUI.color = Color.white;
		}
		Rect rect2 = new Rect(rect.xMax + 2f, rect.y, rect.height, rect.height).ContractedBy(4f);
		GUI.color = (pinned ? Color.white : new Color(1f, 1f, 1f, 0.2f));
		GUI.DrawTexture(rect2, pinned ? PinTex : PinOutlineTex);
		GUI.color = Color.white;
		if (ButtonInvisible(rect2))
		{
			result = DebugActionButtonResult.PinPressed;
		}
		DrawHighlightIfMouseover(rect2);
		return result;
	}

	public static DebugActionButtonResult ButtonDebugPinnable(Rect rect, string label, bool highlight, bool pinned)
	{
		DebugActionButtonResult result = DebugActionButtonResult.None;
		bool wordWrap = Text.WordWrap;
		Text.WordWrap = false;
		if (ButtonText(rect, "  " + label, TextAnchor.MiddleLeft))
		{
			result = DebugActionButtonResult.ButtonPressed;
		}
		Text.WordWrap = wordWrap;
		if (highlight)
		{
			GUI.color = Color.yellow;
			DrawBox(rect, 2);
			GUI.color = Color.white;
		}
		Rect rect2 = new Rect(rect.xMax + 2f, rect.y, rect.height, rect.height).ContractedBy(4f);
		GUI.color = (pinned ? Color.white : new Color(1f, 1f, 1f, 0.2f));
		GUI.DrawTexture(rect2, pinned ? PinTex : PinOutlineTex);
		GUI.color = Color.white;
		if (ButtonInvisible(rect2))
		{
			result = DebugActionButtonResult.PinPressed;
		}
		DrawHighlightIfMouseover(rect2);
		return result;
	}

	public static string TextAreaScrollable(Rect rect, string text, ref Vector2 scrollbarPosition, bool readOnly = false)
	{
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(Text.CalcHeight(text, rect.width) + 10f, rect.height));
		BeginScrollView(rect, ref scrollbarPosition, rect2);
		string result = GUI.TextArea(rect2, text, readOnly ? Text.CurTextAreaReadOnlyStyle : Text.CurTextAreaStyle);
		EndScrollView();
		return result;
	}

	public static float HorizontalSlider(Rect rect, float value, float leftValue, float rightValue)
	{
		float num = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
		if (value != num)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
		}
		return num;
	}
}
