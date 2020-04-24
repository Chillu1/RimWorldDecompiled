using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
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
			rect2.y += rect2.height;
			rect2.x -= rect2.width;
			DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Joy);
			rect2.x += rect2.width;
			DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Sleep);
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
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = Color.white;
			Widgets.Label(rect, ta.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			if (selectedAssignment == ta)
			{
				Widgets.DrawBox(rect, 2);
			}
		}
	}
}
