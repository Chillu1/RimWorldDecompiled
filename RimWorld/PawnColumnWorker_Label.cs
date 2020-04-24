using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Label : PawnColumnWorker
	{
		private const int LeftMargin = 3;

		private static Dictionary<string, string> labelCache = new Dictionary<string, string>();

		private static float labelCacheForWidth = -1f;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
			if (pawn.health.summaryHealth.SummaryHealthPercent < 0.99f)
			{
				Rect rect3 = new Rect(rect2);
				rect3.xMin -= 4f;
				rect3.yMin += 4f;
				rect3.yMax -= 6f;
				Widgets.FillableBar(rect3, pawn.health.summaryHealth.SummaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, doBorder: false);
			}
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			string str = (pawn.RaceProps.Humanlike || pawn.Name == null || pawn.Name.Numerical) ? pawn.LabelCap : (pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel);
			Rect rect4 = rect2;
			rect4.xMin += 3f;
			if (rect4.width != labelCacheForWidth)
			{
				labelCacheForWidth = rect4.width;
				labelCache.Clear();
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Widgets.Label(rect4, str.Truncate(rect4.width, labelCache));
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

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 80);
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(165, GetMinWidth(table), GetMaxWidth(table));
		}
	}
}
