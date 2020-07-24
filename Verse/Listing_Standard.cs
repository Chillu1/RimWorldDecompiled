using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Listing_Standard : Listing
	{
		private GameFont font;

		private List<Pair<Vector2, Vector2>> labelScrollbarPositions;

		private List<Vector2> labelScrollbarPositionsSetThisFrame;

		private const float DefSelectionLineHeight = 21f;

		public Listing_Standard(GameFont font)
		{
			this.font = font;
		}

		public Listing_Standard()
		{
			font = GameFont.Small;
		}

		public override void Begin(Rect rect)
		{
			base.Begin(rect);
			Text.Font = font;
		}

		public void BeginScrollView(Rect rect, ref Vector2 scrollPosition, ref Rect viewRect)
		{
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			rect.height = 100000f;
			rect.width -= 20f;
			Begin(rect.AtZero());
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

		public void EndScrollView(ref Rect viewRect)
		{
			viewRect = new Rect(0f, 0f, listingRect.width, curY);
			Widgets.EndScrollView();
			End();
		}

		public Rect Label(TaggedString label, float maxHeight = -1f, string tooltip = null)
		{
			return Label(label.Resolve(), maxHeight, tooltip);
		}

		public Rect Label(string label, float maxHeight = -1f, string tooltip = null)
		{
			float num = Text.CalcHeight(label, base.ColumnWidth);
			bool flag = false;
			if (maxHeight >= 0f && num > maxHeight)
			{
				num = maxHeight;
				flag = true;
			}
			Rect rect = GetRect(num);
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
			if (tooltip != null)
			{
				TooltipHandler.TipRegion(rect, tooltip);
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
			if (!tip.NullOrEmpty())
			{
				Widgets.DrawHighlightIfMouseover(rect);
				TooltipHandler.TipRegion(rect, tip);
			}
			Widgets.Label(rect.LeftHalf(), leftLabel);
			Widgets.Label(rect.RightHalf(), rightLabel);
			Gap(verticalSpacing);
		}

		[Obsolete]
		public bool RadioButton(string label, bool active, float tabIn = 0f, string tooltip = null)
		{
			return RadioButton_NewTemp(label, active, tabIn, tooltip);
		}

		public bool RadioButton_NewTemp(string label, bool active, float tabIn = 0f, string tooltip = null, float? tooltipDelay = null)
		{
			float lineHeight = Text.LineHeight;
			Rect rect = GetRect(lineHeight);
			rect.xMin += tabIn;
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TipSignal tip = tooltipDelay.HasValue ? new TipSignal(tooltip, tooltipDelay.Value) : new TipSignal(tooltip);
				TooltipHandler.TipRegion(rect, tip);
			}
			bool result = Widgets.RadioButtonLabeled(rect, label, active);
			Gap(verticalSpacing);
			return result;
		}

		public void CheckboxLabeled(string label, ref bool checkOn, string tooltip = null)
		{
			float lineHeight = Text.LineHeight;
			Rect rect = GetRect(lineHeight);
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}
			Widgets.CheckboxLabeled(rect, label, ref checkOn);
			Gap(verticalSpacing);
		}

		public bool CheckboxLabeledSelectable(string label, ref bool selected, ref bool checkOn)
		{
			float lineHeight = Text.LineHeight;
			bool result = Widgets.CheckboxLabeledSelectable(GetRect(lineHeight), label, ref selected, ref checkOn);
			Gap(verticalSpacing);
			return result;
		}

		public bool ButtonText(string label, string highlightTag = null)
		{
			Rect rect = GetRect(30f);
			bool result = Widgets.ButtonText(rect, label);
			if (highlightTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, highlightTag);
			}
			Gap(verticalSpacing);
			return result;
		}

		public bool ButtonTextLabeled(string label, string buttonLabel)
		{
			Rect rect = GetRect(30f);
			Widgets.Label(rect.LeftHalf(), label);
			bool result = Widgets.ButtonText(rect.RightHalf(), buttonLabel);
			Gap(verticalSpacing);
			return result;
		}

		public bool ButtonImage(Texture2D tex, float width, float height)
		{
			NewColumnIfNeeded(height);
			bool result = Widgets.ButtonImage(new Rect(curX, curY, width, height), tex);
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
			string result = (lineCount != 1) ? Widgets.TextArea(rect, text) : Widgets.TextField(rect, text);
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
			Widgets.TextFieldNumeric(GetRect(Text.LineHeight), ref val, ref buffer, min, max);
			Gap(verticalSpacing);
		}

		public void TextFieldNumericLabeled<T>(string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			Widgets.TextFieldNumericLabeled(GetRect(Text.LineHeight), label, ref val, ref buffer, min, max);
			Gap(verticalSpacing);
		}

		public void IntRange(ref IntRange range, int min, int max)
		{
			Widgets.IntRange(GetRect(28f), (int)base.CurHeight, ref range, min, max);
			Gap(verticalSpacing);
		}

		public float Slider(float val, float min, float max)
		{
			float num = Widgets.HorizontalSlider(GetRect(22f), val, min, max);
			if (num != val)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			Gap(verticalSpacing);
			return num;
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

		public void IntEntry(ref int val, ref string editBuffer, int multiplier = 1)
		{
			Widgets.IntEntry(GetRect(24f), ref val, ref editBuffer, multiplier);
			Gap(verticalSpacing);
		}

		public Listing_Standard BeginSection(float height)
		{
			Rect rect = GetRect(height + 8f);
			Widgets.DrawMenuSection(rect);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect.ContractedBy(4f));
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
			if (deleteCallback != null && Widgets.ButtonImage(new Rect(rect.xMax, rect.y, 21f, 21f), TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
			{
				deleteCallback();
			}
			Text.Anchor = TextAnchor.UpperLeft;
			curY += 21f;
			return Widgets.ButtonInvisible(rect);
		}

		public void LabelCheckboxDebug(string label, ref bool checkOn)
		{
			Text.Font = GameFont.Tiny;
			NewColumnIfNeeded(22f);
			Widgets.CheckboxLabeled(new Rect(curX, curY, base.ColumnWidth, 22f), label, ref checkOn);
			Gap(22f + verticalSpacing);
		}

		public bool ButtonDebug(string label)
		{
			Text.Font = GameFont.Tiny;
			NewColumnIfNeeded(22f);
			bool wordWrap = Text.WordWrap;
			Text.WordWrap = false;
			bool result = Widgets.ButtonText(new Rect(curX, curY, base.ColumnWidth, 22f), label);
			Text.WordWrap = wordWrap;
			Gap(22f + verticalSpacing);
			return result;
		}
	}
}
