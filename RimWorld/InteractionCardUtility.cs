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
			float num = rect.width - 29f - 16f - 10f;
			logStrings.Clear();
			float num2 = 0f;
			int num3 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Concerns(pawn))
				{
					TaggedString taggedString = entries[i].ToGameStringFromPOV(pawn);
					logStrings.Add(new Pair<string, int>(taggedString, i));
					num2 += Mathf.Max(26f, Text.CalcHeight(taggedString, num + 1f));
					num3++;
					if (num3 >= maxEntries)
					{
						break;
					}
				}
			}
			Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, rect.width - 16f, num2), outRect: rect, scrollPosition: ref logScrollPosition);
			float num4 = 0f;
			for (int j = 0; j < logStrings.Count; j++)
			{
				TaggedString taggedString2 = logStrings[j].First;
				LogEntry entry = entries[logStrings[j].Second];
				float num5 = Mathf.Max(26f, Text.CalcHeight(taggedString2, num + 1f));
				Texture2D texture2D = entry.IconFromPOV(pawn);
				if (texture2D != null)
				{
					GUI.color = entry.IconColorFromPOV(pawn) ?? Color.white;
					GUI.DrawTexture(new Rect(0f, num4, 26f, 26f), texture2D);
				}
				GUI.color = ((entry.Age > 7500) ? new Color(1f, 1f, 1f, 0.5f) : Color.white);
				Rect rect2 = new Rect(29f, num4, num, num5);
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
				num4 += num5;
			}
			Widgets.EndScrollView();
		}
	}
}
