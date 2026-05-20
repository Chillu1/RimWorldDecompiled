using System;
using UnityEngine;

namespace Verse;

public readonly struct TextBlock : IDisposable
{
	private readonly GameFont oldFont;

	private readonly TextAnchor oldAnchor;

	private readonly bool oldWordWrap;

	private readonly Color oldColor;

	public TextBlock(GameFont newFont)
		: this(newFont, null, null, null)
	{
	}

	public TextBlock(TextAnchor newAnchor)
		: this(null, newAnchor, null, null)
	{
	}

	public TextBlock(bool newWordWrap)
		: this(null, null, newWordWrap, null)
	{
	}

	public TextBlock(Color newColor, TextAnchor newAnchor, bool newWordWrap)
		: this(null, newAnchor, newWordWrap, newColor)
	{
	}

	public TextBlock(Color newColor)
		: this(null, null, null, newColor)
	{
	}

	public TextBlock(TextAnchor newAnchor, Color newColor)
		: this(null, newAnchor, null, newColor)
	{
	}

	public TextBlock(GameFont newFont, TextAnchor newAnchor)
		: this(newFont, newAnchor, null, null)
	{
	}

	public TextBlock(GameFont newFont, Color newColor)
		: this(newFont, null, null, newColor)
	{
	}

	public TextBlock(GameFont? newFont, TextAnchor? newAnchor, Color? newColor)
		: this(newFont, newAnchor, null, newColor)
	{
	}

	public TextBlock(GameFont? newFont, TextAnchor? newAnchor, bool? newWordWrap)
		: this(newFont, newAnchor, newWordWrap, null)
	{
	}

	public TextBlock(GameFont? newFont, TextAnchor? newAnchor, bool? newWordWrap, Color? newColor)
	{
		oldFont = Text.Font;
		oldAnchor = Text.Anchor;
		oldWordWrap = Text.WordWrap;
		oldColor = GUI.color;
		if (newFont.HasValue)
		{
			Text.Font = newFont.Value;
		}
		if (newAnchor.HasValue)
		{
			Text.Anchor = newAnchor.Value;
		}
		if (newWordWrap.HasValue)
		{
			Text.WordWrap = newWordWrap.Value;
		}
		if (newColor.HasValue)
		{
			GUI.color = newColor.Value;
		}
	}

	public static TextBlock Default()
	{
		return new TextBlock(GameFont.Small, TextAnchor.UpperLeft, true, Color.white);
	}

	public void Dispose()
	{
		Text.Font = oldFont;
		Text.Anchor = oldAnchor;
		Text.WordWrap = oldWordWrap;
		GUI.color = oldColor;
	}
}
