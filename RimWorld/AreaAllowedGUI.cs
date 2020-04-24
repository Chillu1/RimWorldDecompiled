using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class AreaAllowedGUI
	{
		private static bool dragging;

		public static void DoAllowedAreaSelectors(Rect rect, Pawn p)
		{
			if (Find.CurrentMap == null)
			{
				return;
			}
			List<Area> allAreas = Find.CurrentMap.areaManager.AllAreas;
			int num = 1;
			for (int i = 0; i < allAreas.Count; i++)
			{
				if (allAreas[i].AssignableAsAllowed())
				{
					num++;
				}
			}
			float num2 = rect.width / (float)num;
			Text.WordWrap = false;
			Text.Font = GameFont.Tiny;
			DoAreaSelector(new Rect(rect.x + 0f, rect.y, num2, rect.height), p, null);
			int num3 = 1;
			for (int j = 0; j < allAreas.Count; j++)
			{
				if (allAreas[j].AssignableAsAllowed())
				{
					float num4 = (float)num3 * num2;
					DoAreaSelector(new Rect(rect.x + num4, rect.y, num2, rect.height), p, allAreas[j]);
					num3++;
				}
			}
			Text.WordWrap = true;
			Text.Font = GameFont.Small;
		}

		private static void DoAreaSelector(Rect rect, Pawn p, Area area)
		{
			MouseoverSounds.DoRegion(rect);
			rect = rect.ContractedBy(1f);
			GUI.DrawTexture(rect, (area != null) ? area.ColorTexture : BaseContent.GreyTex);
			Text.Anchor = TextAnchor.MiddleLeft;
			string text = AreaUtility.AreaAllowedLabel_Area(area);
			Rect rect2 = rect;
			rect2.xMin += 3f;
			rect2.yMin += 2f;
			Widgets.Label(rect2, text);
			if (p.playerSettings.AreaRestriction == area)
			{
				Widgets.DrawBox(rect, 2);
			}
			if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
			{
				dragging = false;
			}
			if (!Input.GetMouseButton(0) && Event.current.type != 0)
			{
				dragging = false;
			}
			if (Mouse.IsOver(rect))
			{
				area?.MarkForDraw();
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					dragging = true;
				}
				if (dragging && p.playerSettings.AreaRestriction != area)
				{
					p.playerSettings.AreaRestriction = area;
					SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			TooltipHandler.TipRegion(rect, text);
		}
	}
}
