using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class TabDrawer
	{
		private const float MaxTabWidth = 200f;

		public const float TabHeight = 32f;

		public const float TabHoriztonalOverlap = 10f;

		private static List<TabRecord> tmpTabs = new List<TabRecord>();

		public static TabRecord DrawTabs(Rect baseRect, List<TabRecord> tabs, int rows)
		{
			if (rows <= 1)
			{
				return DrawTabs(baseRect, tabs);
			}
			int num = Mathf.FloorToInt(tabs.Count / rows);
			int num2 = 0;
			TabRecord result = null;
			Rect rect = baseRect;
			baseRect.yMin -= (float)(rows - 1) * 31f;
			Rect rect2 = baseRect;
			rect2.yMax = rect.y;
			Widgets.DrawMenuSection(rect2);
			for (int i = 0; i < rows; i++)
			{
				int num3 = (i == 0) ? (tabs.Count - (rows - 1) * num) : num;
				tmpTabs.Clear();
				for (int j = num2; j < num2 + num3; j++)
				{
					tmpTabs.Add(tabs[j]);
				}
				TabRecord tabRecord = DrawTabs(baseRect, tmpTabs, baseRect.width);
				if (tabRecord != null)
				{
					result = tabRecord;
				}
				baseRect.yMin += 31f;
				num2 += num3;
			}
			tmpTabs.Clear();
			return result;
		}

		public static TabRecord DrawTabs(Rect baseRect, List<TabRecord> tabs, float maxTabWidth = 200f)
		{
			TabRecord tabRecord = null;
			TabRecord tabRecord2 = tabs.Find((TabRecord t) => t.Selected);
			float num = baseRect.width + (float)(tabs.Count - 1) * 10f;
			float tabWidth = num / (float)tabs.Count;
			if (tabWidth > maxTabWidth)
			{
				tabWidth = maxTabWidth;
			}
			Rect position = new Rect(baseRect);
			position.y -= 32f;
			position.height = 9999f;
			GUI.BeginGroup(position);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			Func<TabRecord, Rect> func = (TabRecord tab) => new Rect((float)tabs.IndexOf(tab) * (tabWidth - 10f), 1f, tabWidth, 32f);
			List<TabRecord> list = tabs.ListFullCopy();
			if (tabRecord2 != null)
			{
				list.Remove(tabRecord2);
				list.Add(tabRecord2);
			}
			TabRecord tabRecord3 = null;
			List<TabRecord> list2 = list.ListFullCopy();
			list2.Reverse();
			for (int i = 0; i < list2.Count; i++)
			{
				TabRecord tabRecord4 = list2[i];
				Rect rect = func(tabRecord4);
				if (tabRecord3 == null && Mouse.IsOver(rect))
				{
					tabRecord3 = tabRecord4;
				}
				MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Tab);
				if (Widgets.ButtonInvisible(rect))
				{
					tabRecord = tabRecord4;
				}
			}
			foreach (TabRecord item in list)
			{
				Rect rect2 = func(item);
				item.Draw(rect2);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			if (tabRecord != null && tabRecord != tabRecord2)
			{
				SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
				if (tabRecord.clickedAction != null)
				{
					tabRecord.clickedAction();
				}
			}
			return tabRecord;
		}
	}
}
