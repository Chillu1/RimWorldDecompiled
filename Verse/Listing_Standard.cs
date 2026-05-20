using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

[StaticConstructorOnStartup]
public class Listing_Standard : Listing
{
	private GameFont font;

	private Rect? boundingRect;

	private Func<Vector2> boundingScrollPositionGetter;

	private List<Pair<Vector2, Vector2>> labelScrollbarPositions;

	private List<Vector2> labelScrollbarPositionsSetThisFrame;

	private int boundingRectCachedForFrame = -1;

	private Rect? boundingRectCached;

	private static readonly Texture2D PinTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin");

	private static readonly Texture2D PinOutlineTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin-Outline");

	public const float PinnableActionHeight = 22f;

	private const float DefSelectionLineHeight = 21f;

	public Rect? BoundingRectCached
	{
		get
		{
			if (boundingRectCachedForFrame != Time.frameCount)
			{
				if (boundingRect.HasValue && boundingScrollPositionGetter != null)
				{
					Rect value = boundingRect.Value;
					Vector2 vector = boundingScrollPositionGetter();
					value.x += vector.x;
					value.y += vector.y;
					boundingRectCached = value;
				}
				boundingRectCachedForFrame = Time.frameCount;
			}
			return boundingRectCached;
		}
	}

	public Listing_Standard(GameFont font)
	{
		this.font = font;
	}

	public Listing_Standard()
	{
		font = GameFont.Small;
	}

	public Listing_Standard(Rect boundingRect, Func<Vector2> boundingScrollPositionGetter)
	{
		font = GameFont.Small;
		this.boundingRect = boundingRect;
		this.boundingScrollPositionGetter = boundingScrollPositionGetter;
	}

	public override void Begin(Rect rect)
	{
		base.Begin(rect);
		Text.Font = font;
	}

	public override void End()
	{
		base.End();
		if (labelScrollbarPositions == null)
		{
			return;
		}
		for (int num = labelScrollbarPositions.Count - 1; num >= 0; num--)
		{
			if (!labelScrollbarPositionsSetThisFrame.Contains(labelScrollbarPositions[num].First))
			{
				labelScrollbarPositions.RemoveAt(num);
			}
		}
		labelScrollbarPositionsSetThisFrame.Clear();
	}

	public Rect Label(TaggedString label, float maxHeight = -1f, string tooltip = null)
	{
		return Label(label.Resolve(), maxHeight, (TipSignal?)(TipSignal)tooltip);
	}

	public Rect Label(string label, float maxHeight = -1f, TipSignal? tipSignal = null)
	{
		float num = Text.CalcHeight(label, base.ColumnWidth);
		bool flag = false;
		if (maxHeight >= 0f && num > maxHeight)
		{
			num = maxHeight;
			flag = true;
		}
		Rect rect = GetRect(num);
		if (BoundingRectCached.HasValue && !rect.Overlaps(BoundingRectCached.Value))
		{
			return rect;
		}
		if (flag)
		{
			Vector2 scrollbarPosition = GetLabelScrollbarPosition(curX, curY);
			Widgets.LabelScrollable(rect, label, ref scrollbarPosition);
			SetLabelScrollbarPosition(curX, curY, scrollbarPosition);
		}
		else
		{
			Widgets.Label(rect, label);
		}
		if (tipSignal.HasValue)
		{
			TooltipHandler.TipRegion(rect, tipSignal.Value);
		}
		Gap(verticalSpacing);
		return rect;
	}

	public void LabelDouble(string leftLabel, string rightLabel, string tip = null)
	{
		float num = base.ColumnWidth / 2f;
		float width = base.ColumnWidth - num;
		float a = Text.CalcHeight(leftLabel, num);
		float b = Text.CalcHeight(rightLabel, width);
		float height = Mathf.Max(a, b);
		Rect rect = GetRect(height);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			if (!tip.NullOrEmpty())
			{
				Widgets.DrawHighlightIfMouseover(rect);
				TooltipHandler.TipRegion(rect, tip);
			}
			Widgets.Label(rect.LeftHalf(), leftLabel);
			Widgets.Label(rect.RightHalf(), rightLabel);
			Gap(verticalSpacing);
		}
	}

	public Rect SubLabel(string label, float widthPct)
	{
		float height = Text.CalcHeight(label, base.ColumnWidth * widthPct);
		Rect rect = GetRect(height, widthPct);
		float num = 20f;
		rect.x += num;
		rect.width -= num;
		Text.Font = GameFont.Tiny;
		GUI.color = Color.gray;
		Widgets.Label(rect, label);
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Gap(verticalSpacing);
		return rect;
	}

	public bool RadioButton(string label, bool active, float tabIn = 0f, string tooltip = null, float? tooltipDelay = null)
	{
		return RadioButton(label, active, tabIn, tooltip, tooltipDelay, disabled: false);
	}

	public bool RadioButton(string label, bool active, float tabIn, string tooltip, float? tooltipDelay, bool disabled)
	{
		return RadioButton(label, active, tabIn, 0f, tooltip, tooltipDelay, disabled);
	}

	public bool RadioButton(string label, bool active, float tabIn, float tabInRight, string tooltip, float? tooltipDelay, bool disabled)
	{
		float lineHeight = Text.LineHeight;
		Rect rect = GetRect(lineHeight);
		rect.xMin += tabIn;
		rect.xMax -= tabInRight;
		if (BoundingRectCached.HasValue && !rect.Overlaps(BoundingRectCached.Value))
		{
			return false;
		}
		if (!tooltip.NullOrEmpty())
		{
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			TipSignal tip = (tooltipDelay.HasValue ? new TipSignal(tooltip, tooltipDelay.Value) : new TipSignal(tooltip));
			TooltipHandler.TipRegion(rect, tip);
		}
		bool result = Widgets.RadioButtonLabeled(rect, label, active, disabled);
		Gap(verticalSpacing);
		return result;
	}

	public void CheckboxLabeled(string label, ref bool checkOn, float tabIn)
	{
		float height = Text.CalcHeight(label, base.ColumnWidth);
		Rect rect = GetRect(height);
		rect.xMin += tabIn;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.CheckboxLabeled(rect, label, ref checkOn);
			Gap(verticalSpacing);
		}
	}

	public void CheckboxLabeled(string label, ref bool checkOn, string tooltip = null, float height = 0f, float labelPct = 1f)
	{
		float height2 = ((height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct));
		Rect rect = GetRect(height2, labelPct);
		rect.width = Math.Min(rect.width + 24f, base.ColumnWidth);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}
			Widgets.CheckboxLabeled(rect, label, ref checkOn);
		}
		Gap(verticalSpacing);
	}

	public bool CheckboxLabeledSelectable(string label, ref bool selected, ref bool checkOn)
	{
		float lineHeight = Text.LineHeight;
		Rect rect = GetRect(lineHeight);
		bool result = false;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			result = Widgets.CheckboxLabeledSelectable(rect, label, ref selected, ref checkOn);
		}
		Gap(verticalSpacing);
		return result;
	}

	public bool ButtonText(string label, string highlightTag = null, float widthPct = 1f)
	{
		Rect rect = GetRect(30f, widthPct);
		bool result = false;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			result = Widgets.ButtonText(rect, label);
			if (highlightTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, highlightTag);
			}
		}
		Gap(verticalSpacing);
		return result;
	}

	public bool ButtonTextLabeled(string label, string buttonLabel, TextAnchor anchor = TextAnchor.UpperLeft, string highlightTag = null, string tooltip = null)
	{
		return ButtonTextLabeledPct(label, buttonLabel, 0.5f, anchor, highlightTag, tooltip);
	}

	public bool ButtonTextLabeledPct(string label, string buttonLabel, float labelPct, TextAnchor anchor = TextAnchor.UpperLeft, string highlightTag = null, string tooltip = null, Texture2D labelIcon = null)
	{
		float height = Math.Max(Text.CalcHeight(label, base.ColumnWidth * labelPct), 30f);
		Rect rect = GetRect(height);
		Rect rect2 = rect.RightPart(1f - labelPct);
		rect2.height = 30f;
		if (highlightTag != null)
		{
			UIHighlighter.HighlightOpportunity(rect, highlightTag);
		}
		bool result = false;
		Rect rect3 = rect.LeftPart(labelPct);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Text.Anchor = anchor;
			Widgets.Label(rect3, label);
			result = Widgets.ButtonText(rect2, buttonLabel.Truncate(rect2.width - 20f));
			Text.Anchor = TextAnchor.UpperLeft;
		}
		if (labelIcon != null)
		{
			GUI.DrawTexture(new Rect(Text.CalcSize(label).x + 10f, rect3.y + (rect3.height - Text.LineHeight) / 2f, Text.LineHeight, Text.LineHeight), labelIcon);
		}
		if (!tooltip.NullOrEmpty())
		{
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
			}
			TooltipHandler.TipRegion(rect3, tooltip);
		}
		Gap(verticalSpacing);
		return result;
	}

	public bool ButtonImage(Texture2D tex, float width, float height)
	{
		NewColumnIfNeeded(height);
		Rect butRect = new Rect(curX, curY, width, height);
		bool result = false;
		if (!BoundingRectCached.HasValue || butRect.Overlaps(BoundingRectCached.Value))
		{
			result = Widgets.ButtonImage(butRect, tex);
		}
		Gap(height + verticalSpacing);
		return result;
	}

	public void None()
	{
		GUI.color = Color.gray;
		Text.Anchor = TextAnchor.UpperCenter;
		Label("NoneBrackets".Translate());
		GenUI.ResetLabelAlign();
		GUI.color = Color.white;
	}

	public string TextEntry(string text, int lineCount = 1)
	{
		Rect rect = GetRect(Text.LineHeight * (float)lineCount);
		string result = ((lineCount != 1) ? Widgets.TextArea(rect, text) : Widgets.TextField(rect, text));
		Gap(verticalSpacing);
		return result;
	}

	public string TextEntryLabeled(string label, string text, int lineCount = 1)
	{
		string result = Widgets.TextEntryLabeled(GetRect(Text.LineHeight * (float)lineCount), label, text);
		Gap(verticalSpacing);
		return result;
	}

	public void TextFieldNumeric<T>(ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
	{
		Rect rect = GetRect(Text.LineHeight);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.TextFieldNumeric(rect, ref val, ref buffer, min, max);
		}
		Gap(verticalSpacing);
	}

	public void TextFieldNumericLabeled<T>(string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
	{
		Rect rect = GetRect(Text.LineHeight);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.TextFieldNumericLabeled(rect, label, ref val, ref buffer, min, max);
		}
		Gap(verticalSpacing);
	}

	public Rect IntRange(ref IntRange range, int min, int max)
	{
		Rect rect = GetRect(32f);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.IntRange(rect, (int)base.CurHeight, ref range, min, max);
		}
		Gap(verticalSpacing);
		return rect;
	}

	public float Slider(float val, float min, float max)
	{
		float result = Widgets.HorizontalSlider(GetRect(22f), val, min, max);
		Gap(verticalSpacing);
		return result;
	}

	public float SliderLabeled(string label, float val, float min, float max, float labelPct = 0.5f, string tooltip = null)
	{
		Rect rect = GetRect(30f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect.LeftPart(labelPct), label);
		if (tooltip != null)
		{
			TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
		}
		Text.Anchor = TextAnchor.UpperLeft;
		float result = Widgets.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, middleAlignment: true);
		Gap(verticalSpacing);
		return result;
	}

	public void IntAdjuster(ref int val, int countChange, int min = 0)
	{
		Rect rect = GetRect(24f);
		rect.width = 42f;
		if (Widgets.ButtonText(rect, "-" + countChange))
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			val -= countChange * GenUI.CurrentAdjustmentMultiplier();
			if (val < min)
			{
				val = min;
			}
		}
		rect.x += rect.width + 2f;
		if (Widgets.ButtonText(rect, "+" + countChange))
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			val += countChange * GenUI.CurrentAdjustmentMultiplier();
			if (val < min)
			{
				val = min;
			}
		}
		Gap(verticalSpacing);
	}

	public void IntSetter(ref int val, int target, string label, float width = 42f)
	{
		if (Widgets.ButtonText(GetRect(24f), label))
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			val = target;
		}
		Gap(verticalSpacing);
	}

	public void IntEntry(ref int val, ref string editBuffer, int multiplier = 1, int min = 0)
	{
		Rect rect = GetRect(24f);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.IntEntry(rect, ref val, ref editBuffer, multiplier);
			if (val < min)
			{
				val = min;
				editBuffer = val.ToString();
			}
		}
		Gap(verticalSpacing);
	}

	public Listing_Standard BeginSection(float height, float sectionBorder = 4f, float bottomBorder = 4f)
	{
		Rect rect = GetRect(height + sectionBorder + bottomBorder);
		Widgets.DrawMenuSection(rect);
		Listing_Standard listing_Standard = new Listing_Standard();
		Rect rect2 = new Rect(rect.x + sectionBorder, rect.y + sectionBorder, rect.width - sectionBorder * 2f, rect.height - (sectionBorder + bottomBorder));
		listing_Standard.Begin(rect2);
		return listing_Standard;
	}

	public void EndSection(Listing_Standard listing)
	{
		listing.End();
	}

	private Vector2 GetLabelScrollbarPosition(float x, float y)
	{
		if (labelScrollbarPositions == null)
		{
			return Vector2.zero;
		}
		for (int i = 0; i < labelScrollbarPositions.Count; i++)
		{
			Vector2 first = labelScrollbarPositions[i].First;
			if (first.x == x && first.y == y)
			{
				return labelScrollbarPositions[i].Second;
			}
		}
		return Vector2.zero;
	}

	private void SetLabelScrollbarPosition(float x, float y, Vector2 scrollbarPosition)
	{
		if (labelScrollbarPositions == null)
		{
			labelScrollbarPositions = new List<Pair<Vector2, Vector2>>();
			labelScrollbarPositionsSetThisFrame = new List<Vector2>();
		}
		labelScrollbarPositionsSetThisFrame.Add(new Vector2(x, y));
		for (int i = 0; i < labelScrollbarPositions.Count; i++)
		{
			Vector2 first = labelScrollbarPositions[i].First;
			if (first.x == x && first.y == y)
			{
				labelScrollbarPositions[i] = new Pair<Vector2, Vector2>(new Vector2(x, y), scrollbarPosition);
				return;
			}
		}
		labelScrollbarPositions.Add(new Pair<Vector2, Vector2>(new Vector2(x, y), scrollbarPosition));
	}

	public bool SelectableDef(string name, bool selected, Action deleteCallback)
	{
		Text.Font = GameFont.Tiny;
		float width = listingRect.width - 21f;
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect = new Rect(curX, curY, width, 21f);
		if (selected)
		{
			Widgets.DrawHighlight(rect);
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawBox(rect);
		}
		Text.WordWrap = false;
		Widgets.Label(rect, name);
		Text.WordWrap = true;
		if (deleteCallback != null && Widgets.ButtonImage(new Rect(rect.xMax, rect.y, 21f, 21f), TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor))
		{
			deleteCallback();
		}
		Text.Anchor = TextAnchor.UpperLeft;
		curY += 21f;
		return Widgets.ButtonInvisible(rect);
	}

	public void LabelCheckboxDebug(string label, ref bool checkOn, bool highlight)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(22f);
		Rect rect = new Rect(curX, curY, base.ColumnWidth, 22f);
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.CheckboxLabeled(rect, label.Truncate(rect.width - 15f), ref checkOn);
			if (highlight)
			{
				GUI.color = Color.yellow;
				Widgets.DrawBox(rect, 2);
				GUI.color = Color.white;
			}
		}
		Gap(22f + verticalSpacing);
	}

	public bool ButtonDebug(string label, bool highlight)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(22f);
		Rect rect = new Rect(curX, curY, base.ColumnWidth, 22f);
		bool result = false;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			bool wordWrap = Text.WordWrap;
			Text.WordWrap = false;
			result = Widgets.ButtonText(rect, "  " + label, drawBackground: true, doMouseoverSound: true, active: true, TextAnchor.MiddleLeft);
			Text.WordWrap = wordWrap;
			if (highlight)
			{
				GUI.color = Color.yellow;
				Widgets.DrawBox(rect, 2);
				GUI.color = Color.white;
			}
		}
		Gap(22f + verticalSpacing);
		return result;
	}

	public DebugActionButtonResult ButtonDebugPinnable(string label, bool highlight, bool pinned)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(22f);
		Rect rect = new Rect(curX, curY, base.ColumnWidth - 22f, 22f);
		DebugActionButtonResult result = DebugActionButtonResult.None;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			bool wordWrap = Text.WordWrap;
			Text.WordWrap = false;
			if (Widgets.ButtonText(rect, "  " + label, drawBackground: true, doMouseoverSound: true, active: true, TextAnchor.MiddleLeft))
			{
				result = DebugActionButtonResult.ButtonPressed;
			}
			Text.WordWrap = wordWrap;
			if (highlight)
			{
				GUI.color = Color.yellow;
				Widgets.DrawBox(rect, 2);
				GUI.color = Color.white;
			}
			Rect rect2 = new Rect(rect.xMax + 2f, rect.y, 22f, 22f).ContractedBy(4f);
			GUI.color = (pinned ? Color.white : new Color(1f, 1f, 1f, 0.2f));
			GUI.DrawTexture(rect2, pinned ? PinTex : PinOutlineTex);
			GUI.color = Color.white;
			if (Widgets.ButtonInvisible(rect2))
			{
				result = DebugActionButtonResult.PinPressed;
			}
			Widgets.DrawHighlightIfMouseover(rect2);
		}
		Gap(22f + verticalSpacing);
		return result;
	}

	public DebugActionButtonResult CheckboxPinnable(string label, ref bool checkOn, bool highlight, bool pinned)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(22f);
		Rect rect = new Rect(curX, curY, base.ColumnWidth - 22f, 22f);
		DebugActionButtonResult result = DebugActionButtonResult.None;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			Widgets.CheckboxLabeled(rect, label.Truncate(rect.width - 24f - 15f), ref checkOn);
			if (highlight)
			{
				GUI.color = Color.yellow;
				Widgets.DrawBox(rect, 2);
				GUI.color = Color.white;
			}
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, label);
			}
			Rect rect2 = new Rect(rect.xMax + 2f, rect.y, 22f, 22f).ContractedBy(4f);
			GUI.color = (pinned ? Color.white : new Color(1f, 1f, 1f, 0.2f));
			GUI.DrawTexture(rect2, pinned ? PinTex : PinOutlineTex);
			GUI.color = Color.white;
			if (Widgets.ButtonInvisible(rect2))
			{
				result = DebugActionButtonResult.PinPressed;
			}
			Widgets.DrawHighlightIfMouseover(rect2);
		}
		Gap(22f + verticalSpacing);
		return result;
	}
}
