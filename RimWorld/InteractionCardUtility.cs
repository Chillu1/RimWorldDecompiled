using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InteractionCardUtility
	{
		private static Vector2 logScrollPosition = Vector2.zero;

		public const float ImageSize = 26f;

		public const float ImagePadRight = 3f;

		public const float TextOffset = 29f;

		private static List<Pair<string, int>> logStrings = new List<Pair<string, int>>();

		public static void DrawInteractionsLog(Rect rect, Pawn pawn, List<LogEntry> entries, int maxEntries)
		{
			float width = rect.width - 29f - 16f - 10f;
			logStrings.Clear();
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Concerns(pawn))
				{
					TaggedString taggedString = entries[i].ToGameStringFromPOV(pawn);
					logStrings.Add(new Pair<string, int>(taggedString, i));
					num += Mathf.Max(26f, Text.CalcHeight(taggedString, width));
					num2++;
					if (num2 >= maxEntries)
					{
						break;
					}
				}
			}
			Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, rect.width - 16f, num), outRect: rect, scrollPosition: ref logScrollPosition);
			float num3 = 0f;
			for (int j = 0; j < logStrings.Count; j++)
			{
				TaggedString taggedString2 = logStrings[j].First;
				LogEntry entry = entries[logStrings[j].Second];
				if (entry.Age > 7500)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				float num4 = Mathf.Max(26f, Text.CalcHeight(taggedString2, width));
				Texture2D texture2D = entry.IconFromPOV(pawn);
				if (texture2D != null)
				{
					GUI.DrawTexture(new Rect(0f, num3, 26f, 26f), texture2D);
				}
				Rect rect2 = new Rect(29f, num3, width, num4);
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, () => entry.GetTipString(), 613261 + j * 611);
					Widgets.DrawHighlight(rect2);
				}
				Widgets.Label(rect2, taggedString2);
				if (Widgets.ButtonInvisible(rect2, entry.CanBeClickedFromPOV(pawn)))
				{
					entry.ClickedFromPOV(pawn);
				}
				GUI.color = Color.white;
				num3 += num4;
			}
			Widgets.EndScrollView();
		}
	}
}
