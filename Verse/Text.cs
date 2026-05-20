using System;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public static class Text
{
	private static GameFont fontInt;

	private static TextAnchor anchorInt;

	private static bool wordWrapInt;

	private static Font[] fonts;

	public static readonly GUIStyle[] fontStyles;

	public static readonly GUIStyle[] textFieldStyles;

	public static readonly GUIStyle[] textAreaStyles;

	public static readonly GUIStyle[] textAreaReadOnlyStyles;

	private static readonly float[] lineHeights;

	private static readonly float[] spaceBetweenLines;

	private static GUIContent tmpTextGUIContent;

	private const int NumFonts = 3;

	public const float SmallFontHeight = 22f;

	public static bool TinyFontSupported
	{
		get
		{
			if (!LongEventHandler.AnyEventNowOrWaiting && !LanguageDatabase.activeLanguage.info.canBeTiny)
			{
				return false;
			}
			if (Prefs.DisableTinyText)
			{
				return false;
			}
			if (SteamDeck.IsSteamDeck)
			{
				return false;
			}
			return true;
		}
	}

	public static GameFont Font
	{
		get
		{
			return fontInt;
		}
		set
		{
			if (value == GameFont.Tiny && !TinyFontSupported)
			{
				fontInt = GameFont.Small;
			}
			else
			{
				fontInt = value;
			}
		}
	}

	public static TextAnchor Anchor
	{
		get
		{
			return anchorInt;
		}
		set
		{
			anchorInt = value;
		}
	}

	public static bool WordWrap
	{
		get
		{
			return wordWrapInt;
		}
		set
		{
			wordWrapInt = value;
		}
	}

	public static float LineHeight => lineHeights[(uint)Font];

	public static float SpaceBetweenLines => spaceBetweenLines[(uint)Font];

	public static GUIStyle CurFontStyle
	{
		get
		{
			GUIStyle gUIStyle = null;
			gUIStyle = fontInt switch
			{
				GameFont.Tiny => fontStyles[0], 
				GameFont.Small => fontStyles[1], 
				GameFont.Medium => fontStyles[2], 
				_ => throw new NotImplementedException(), 
			};
			gUIStyle.alignment = anchorInt;
			gUIStyle.wordWrap = wordWrapInt;
			return gUIStyle;
		}
	}

	public static GUIStyle CurTextFieldStyle => fontInt switch
	{
		GameFont.Tiny => textFieldStyles[0], 
		GameFont.Small => textFieldStyles[1], 
		GameFont.Medium => textFieldStyles[2], 
		_ => throw new NotImplementedException(), 
	};

	public static GUIStyle CurTextAreaStyle => fontInt switch
	{
		GameFont.Tiny => textAreaStyles[0], 
		GameFont.Small => textAreaStyles[1], 
		GameFont.Medium => textAreaStyles[2], 
		_ => throw new NotImplementedException(), 
	};

	public static GUIStyle CurTextAreaReadOnlyStyle => fontInt switch
	{
		GameFont.Tiny => textAreaReadOnlyStyles[0], 
		GameFont.Small => textAreaReadOnlyStyles[1], 
		GameFont.Medium => textAreaReadOnlyStyles[2], 
		_ => throw new NotImplementedException(), 
	};

	static Text()
	{
		fontInt = GameFont.Small;
		anchorInt = TextAnchor.UpperLeft;
		wordWrapInt = true;
		fonts = new Font[3];
		fontStyles = new GUIStyle[3];
		textFieldStyles = new GUIStyle[3];
		textAreaStyles = new GUIStyle[3];
		textAreaReadOnlyStyles = new GUIStyle[3];
		lineHeights = new float[3];
		spaceBetweenLines = new float[3];
		tmpTextGUIContent = new GUIContent();
		fonts[0] = (Font)Resources.Load("Fonts/Calibri_tiny");
		fonts[1] = (Font)Resources.Load("Fonts/Arial_small");
		fonts[2] = (Font)Resources.Load("Fonts/Arial_medium");
		fontStyles[0] = new GUIStyle(GUI.skin.label);
		fontStyles[0].font = fonts[0];
		fontStyles[1] = new GUIStyle(GUI.skin.label);
		fontStyles[1].font = fonts[1];
		fontStyles[1].contentOffset = new Vector2(0f, -1f);
		fontStyles[2] = new GUIStyle(GUI.skin.label);
		fontStyles[2].font = fonts[2];
		for (int i = 0; i < textFieldStyles.Length; i++)
		{
			textFieldStyles[i] = new GUIStyle(GUI.skin.textField);
			textFieldStyles[i].alignment = TextAnchor.MiddleLeft;
		}
		textFieldStyles[0].font = fonts[0];
		textFieldStyles[1].font = fonts[1];
		textFieldStyles[2].font = fonts[2];
		for (int j = 0; j < textAreaStyles.Length; j++)
		{
			textAreaStyles[j] = new GUIStyle(textFieldStyles[j]);
			textAreaStyles[j].alignment = TextAnchor.UpperLeft;
			textAreaStyles[j].wordWrap = true;
		}
		for (int k = 0; k < textAreaReadOnlyStyles.Length; k++)
		{
			textAreaReadOnlyStyles[k] = new GUIStyle(textAreaStyles[k]);
			GUIStyle obj = textAreaReadOnlyStyles[k];
			obj.normal.background = null;
			obj.active.background = null;
			obj.onHover.background = null;
			obj.hover.background = null;
			obj.onFocused.background = null;
			obj.focused.background = null;
		}
		GUI.skin.settings.doubleClickSelectsWord = true;
		int num = 0;
		foreach (GameFont value in Enum.GetValues(typeof(GameFont)))
		{
			Font = value;
			lineHeights[num] = CalcHeight("W", 999f);
			spaceBetweenLines[num] = CalcHeight("W\nW", 999f) - CalcHeight("W", 999f) * 2f;
			num++;
		}
		Font = GameFont.Small;
	}

	public static float LineHeightOf(GameFont font)
	{
		return lineHeights[(uint)font];
	}

	public static float CalcHeight(string text, float width)
	{
		tmpTextGUIContent.text = text.StripTags();
		return CurFontStyle.CalcHeight(tmpTextGUIContent, width);
	}

	public static Vector2 CalcSize(string text)
	{
		tmpTextGUIContent.text = text.StripTags();
		return CurFontStyle.CalcSize(tmpTextGUIContent);
	}

	public static string ClampTextWithEllipsis(Rect rect, string text)
	{
		if (text.Length <= 4)
		{
			return text;
		}
		Vector2 vector = CalcSize(text);
		if (vector.x <= rect.width - 13f)
		{
			return text;
		}
		while (vector.x > rect.width - 13f)
		{
			if (text.Length == 0)
			{
				return "";
			}
			text = text.Substring(0, text.Length - 1);
			vector = CalcSize(text + "...");
		}
		return text + "...";
	}

	public static void StartOfOnGUI()
	{
		if (!WordWrap)
		{
			Log.ErrorOnce("Word wrap was false at end of frame.", 764362);
			WordWrap = true;
		}
		if (Anchor != TextAnchor.UpperLeft)
		{
			Log.ErrorOnce("Alignment was " + Anchor.ToString() + " at end of frame.", 15558);
			Anchor = TextAnchor.UpperLeft;
		}
		Font = GameFont.Small;
	}
}
