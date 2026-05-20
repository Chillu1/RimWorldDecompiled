using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class TimeAssignmentSelector
{
	public static TimeAssignmentDef selectedAssignment = TimeAssignmentDefOf.Work;

	public static void DrawTimeAssignmentSelectorGrid(Rect rect)
	{
		rect.yMax -= 2f;
		Rect rect2 = rect;
		rect2.xMax = rect2.center.x;
		rect2.yMax = rect2.center.y;
		DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Anything);
		rect2.x += rect2.width;
		DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Work);
		rect2.x += rect2.width;
		DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Joy);
		rect2.x += rect2.width;
		DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Sleep);
		if (ModsConfig.RoyaltyActive)
		{
			rect2.x += rect2.width;
			DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Meditate);
		}
	}

	private static void DrawTimeAssignmentSelectorFor(Rect rect, TimeAssignmentDef ta)
	{
		rect = rect.ContractedBy(2f);
		GUI.DrawTexture(rect, ta.ColorTexture);
		if (Widgets.ButtonInvisible(rect))
		{
			selectedAssignment = ta;
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		using (new TextBlock(TextAnchor.MiddleCenter))
		{
			Widgets.Label(rect, ta.LabelCap);
		}
		if (selectedAssignment == ta)
		{
			Widgets.DrawBox(rect, 2);
		}
		else
		{
			UIHighlighter.HighlightOpportunity(rect, ta.cachedHighlightNotSelectedTag);
		}
	}
}
