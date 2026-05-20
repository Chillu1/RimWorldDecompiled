using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_Label : PawnColumnWorker
{
	private const int LeftMargin = 3;

	private const float PortraitCameraZoom = 1.2f;

	private static Dictionary<string, TaggedString> labelCache = new Dictionary<string, TaggedString>();

	private static float labelCacheForWidth = -1f;

	protected virtual TextAnchor LabelAlignment => TextAnchor.MiddleLeft;

	protected override TextAnchor DefaultHeaderAlignment => TextAnchor.LowerLeft;

	protected override float GetHeaderOffsetX(Rect rect)
	{
		return 33f;
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, def.groupable ? rect.height : ((float)GetMinCellHeight(pawn))));
		Rect rect3 = rect2;
		rect3.xMin += 3f;
		if (def.showIcon)
		{
			rect3.xMin += rect2.height;
			Rect rect4 = new Rect(rect2.x, rect2.y, rect2.height, rect2.height);
			if (Find.Selector.IsSelected(pawn))
			{
				SelectionDrawerUtility.DrawSelectionOverlayWholeGUI(rect4.ContractedBy(2f));
			}
			Widgets.ThingIcon(rect4, pawn);
		}
		if (pawn.health.summaryHealth.SummaryHealthPercent < 0.99f)
		{
			Rect rect5 = new Rect(rect3.x - 3f, rect3.y, rect3.width + 3f, rect3.height);
			rect5.yMin += 4f;
			rect5.yMax -= 6f;
			Widgets.FillableBar(rect5, pawn.health.summaryHealth.SummaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, doBorder: false);
		}
		if (Mouse.IsOver(rect2))
		{
			GUI.DrawTexture(rect2, TexUI.HighlightTex);
		}
		TaggedString taggedString = GetLabel(pawn);
		if (rect3.width != labelCacheForWidth)
		{
			labelCacheForWidth = rect3.width;
			labelCache.Clear();
		}
		if (Text.CalcSize(taggedString).x > rect3.width)
		{
			taggedString = taggedString.Truncate(rect3.width, labelCache);
		}
		if (pawn.IsSlave || pawn.IsColonyMech)
		{
			taggedString = taggedString.Colorize(PawnNameColorUtility.PawnNameColorOf(pawn));
		}
		Text.Font = GameFont.Small;
		Text.Anchor = LabelAlignment;
		Text.WordWrap = false;
		Widgets.Label(rect3, taggedString);
		Text.WordWrap = true;
		Text.Anchor = TextAnchor.UpperLeft;
		if (Widgets.ButtonInvisible(rect2))
		{
			CameraJumper.TryJumpAndSelect(pawn);
			if (Current.ProgramState == ProgramState.Playing && Event.current.button == 0)
			{
				Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
			}
		}
		else if (Mouse.IsOver(rect2))
		{
			TipSignal tooltip = pawn.GetTooltip();
			tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
			TooltipHandler.TipRegion(rect2, tooltip);
		}
	}

	private TaggedString GetLabel(Pawn pawn)
	{
		if (!pawn.RaceProps.Humanlike && !pawn.RaceProps.Animal && pawn.Name != null && !pawn.Name.Numerical)
		{
			return pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel.Colorize(ColoredText.SubtleGrayColor);
		}
		if (def.useLabelShort)
		{
			return pawn.LabelShortCap;
		}
		return pawn.LabelNoCount.CapitalizeFirst();
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 80);
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(165, GetMinWidth(table), GetMaxWidth(table));
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return string.Compare(GetValueToCompare(a), GetValueToCompare(b), StringComparison.Ordinal);
	}

	private string GetValueToCompare(Pawn pawn)
	{
		return GetLabel(pawn);
	}
}
