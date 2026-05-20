using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class WidgetRow
{
	private float startX;

	private float curX;

	private float curY;

	private float maxWidth = 99999f;

	private float gap;

	private UIDirection growDirection = UIDirection.RightThenUp;

	public const float IconSize = 24f;

	public const float DefaultGap = 4f;

	private const float DefaultMaxWidth = 99999f;

	public const float LabelGap = 2f;

	public const float ButtonExtraSpace = 16f;

	public float FinalX => curX;

	public float FinalY => curY;

	public float CellGap
	{
		get
		{
			return gap;
		}
		set
		{
			gap = value;
		}
	}

	public WidgetRow()
	{
	}

	public WidgetRow(float x, float y, UIDirection growDirection = UIDirection.RightThenUp, float maxWidth = 99999f, float gap = 4f)
	{
		Init(x, y, growDirection, maxWidth, gap);
	}

	public void Init(float x, float y, UIDirection growDirection = UIDirection.RightThenUp, float maxWidth = 99999f, float gap = 4f)
	{
		this.growDirection = growDirection;
		startX = x;
		curX = x;
		curY = y;
		this.maxWidth = maxWidth;
		this.gap = gap;
	}

	private float LeftX(float elementWidth)
	{
		if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.RightThenDown)
		{
			return curX;
		}
		return curX - elementWidth;
	}

	private void IncrementPosition(float amount)
	{
		if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.RightThenDown)
		{
			curX += amount;
		}
		else
		{
			curX -= amount;
		}
		if (Mathf.Abs(curX - startX) > maxWidth)
		{
			IncrementY();
		}
	}

	private void IncrementY()
	{
		if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.LeftThenUp)
		{
			curY -= 24f + gap;
		}
		else
		{
			curY += 24f + gap;
		}
		curX = startX;
	}

	private void IncrementYIfWillExceedMaxWidth(float width)
	{
		if (Mathf.Abs(curX - startX) + Mathf.Abs(width) > maxWidth)
		{
			IncrementY();
		}
	}

	public void Gap(float width)
	{
		if (curX != startX)
		{
			IncrementPosition(width);
		}
	}

	public bool ButtonIcon(Texture2D tex, string tooltip = null, Color? mouseoverColor = null, Color? backgroundColor = null, Color? mouseoverBackgroundColor = null, bool doMouseoverSound = true, float overrideSize = -1f)
	{
		Rect rect = ButtonIconRect(overrideSize);
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(rect);
		}
		if (mouseoverBackgroundColor.HasValue && Mouse.IsOver(rect))
		{
			Widgets.DrawRectFast(rect, mouseoverBackgroundColor.Value);
		}
		else if (backgroundColor.HasValue && !Mouse.IsOver(rect))
		{
			Widgets.DrawRectFast(rect, backgroundColor.Value);
		}
		bool result = Widgets.ButtonImage(rect, tex, Color.white, mouseoverColor ?? GenUI.MouseoverColor);
		IncrementPosition((overrideSize > 0f) ? overrideSize : 24f);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		return result;
	}

	public Rect ButtonIconRect(float overrideSize = -1f)
	{
		float num = ((overrideSize > 0f) ? overrideSize : 24f);
		float num2 = (24f - num) / 2f;
		IncrementYIfWillExceedMaxWidth(num);
		return new Rect(LeftX(num) + num2, curY + num2, num, num);
	}

	public bool ButtonIconWithBG(Texture2D texture, float width = -1f, string tooltip = null, bool doMouseoverSound = true)
	{
		if (width < 0f)
		{
			width = 24f;
		}
		width += 16f;
		IncrementYIfWillExceedMaxWidth(width);
		Rect rect = new Rect(LeftX(width), curY, width, 26f);
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(rect);
		}
		bool result = Widgets.ButtonImageWithBG(rect, texture, Vector2.one * 24f);
		IncrementPosition(width + gap);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		return result;
	}

	public void ToggleableIcon(ref bool toggleable, Texture2D tex, string tooltip, SoundDef mouseoverSound = null, string tutorTag = null)
	{
		IncrementYIfWillExceedMaxWidth(24f);
		Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
		bool num = Widgets.ButtonImage(rect, tex);
		IncrementPosition(24f + gap);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		Rect position = new Rect(rect.x + rect.width / 2f, rect.y, rect.height / 2f, rect.height / 2f);
		Texture2D image = (toggleable ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
		GUI.DrawTexture(position, image);
		if (mouseoverSound != null)
		{
			MouseoverSounds.DoRegion(rect, mouseoverSound);
		}
		if (num)
		{
			toggleable = !toggleable;
			if (toggleable)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
		}
		if (tutorTag != null)
		{
			UIHighlighter.HighlightOpportunity(rect, tutorTag);
		}
	}

	public Rect Icon(Texture tex, string tooltip = null, float contractedBy = 0f)
	{
		IncrementYIfWillExceedMaxWidth(24f);
		Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
		rect = rect.ContractedBy(contractedBy);
		GUI.DrawTexture(rect, tex);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		IncrementPosition(24f + gap);
		return rect;
	}

	public Rect DefIcon(ThingDef def, string tooltip = null)
	{
		IncrementYIfWillExceedMaxWidth(24f);
		Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
		Widgets.DefIcon(rect, def);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		IncrementPosition(24f + gap);
		return rect;
	}

	public bool ButtonText(string label, string tooltip = null, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, float? fixedWidth = null)
	{
		Rect rect = ButtonRect(label, fixedWidth);
		bool result = Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, active);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		return result;
	}

	public Rect ButtonRect(string label, float? fixedWidth = null)
	{
		Vector2 vector = (fixedWidth.HasValue ? new Vector2(fixedWidth.Value, 24f) : Text.CalcSize(label));
		vector.x += 16f;
		vector.y += 2f;
		IncrementYIfWillExceedMaxWidth(vector.x);
		Rect result = new Rect(LeftX(vector.x), curY, vector.x, vector.y);
		IncrementPosition(result.width + gap);
		return result;
	}

	public Rect Label(string text, float width = -1f, string tooltip = null, float height = -1f)
	{
		if (height < 0f)
		{
			height = 24f;
		}
		if (width < 0f)
		{
			width = Text.CalcSize(text).x;
		}
		IncrementYIfWillExceedMaxWidth(width + 2f);
		IncrementPosition(2f);
		Rect rect = new Rect(LeftX(width), curY, width, height);
		Widgets.Label(rect, text);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		IncrementPosition(2f);
		IncrementPosition(rect.width);
		return rect;
	}

	public Rect LabelEllipses(string text, float width, string tooltip = null, float height = -1f)
	{
		if (height < 0f)
		{
			height = 24f;
		}
		IncrementYIfWillExceedMaxWidth(width + 2f);
		IncrementPosition(2f);
		Rect rect = new Rect(LeftX(width), curY, width, height);
		Widgets.LabelEllipses(rect, text);
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, tooltip);
		}
		IncrementPosition(2f);
		IncrementPosition(rect.width);
		return rect;
	}

	public Rect TextFieldNumeric<T>(ref int val, ref string buffer, float width = -1f) where T : struct
	{
		if (width < 0f)
		{
			width = Text.CalcSize(val.ToString()).x;
		}
		IncrementYIfWillExceedMaxWidth(width + 2f);
		IncrementPosition(2f);
		Rect rect = new Rect(LeftX(width), curY, width, 24f);
		Widgets.TextFieldNumeric(rect, ref val, ref buffer);
		IncrementPosition(2f);
		IncrementPosition(rect.width);
		return rect;
	}

	public Rect FillableBar(float width, float height, float fillPct, string label, Texture2D fillTex, Texture2D bgTex = null)
	{
		IncrementYIfWillExceedMaxWidth(width);
		Rect rect = new Rect(LeftX(width), curY, width, height);
		Widgets.FillableBar(rect, fillPct, fillTex, bgTex, doBorder: false);
		if (!label.NullOrEmpty())
		{
			Rect rect2 = rect;
			rect2.xMin += 2f;
			rect2.xMax -= 2f;
			if (!Text.TinyFontSupported)
			{
				rect2.y -= 2f;
			}
			if (Text.Anchor >= TextAnchor.UpperLeft)
			{
				rect2.height += 14f;
			}
			using (new TextBlock(GameFont.Tiny, null, false))
			{
				Widgets.Label(rect2, label);
			}
		}
		IncrementPosition(width);
		return rect;
	}
}
