using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanNeedsTabUtility
	{
		private const float RowHeight = 50f;

		private const float LabelHeight = 18f;

		private const float LabelColumnWidth = 100f;

		private const float NeedWidth = 100f;

		private const float NeedMargin = 10f;

		private static List<Need> tmpNeeds = new List<Need>();

		private static List<Thought> thoughtGroupsPresent = new List<Thought>();

		private static List<Thought> thoughtGroup = new List<Thought>();

		public static void DoRows(Vector2 size, List<Pawn> pawns, Caravan caravan, ref Vector2 scrollPosition, ref float scrollViewHeight, ref Pawn specificNeedsTabForPawn, bool doNeeds = true)
		{
			if (specificNeedsTabForPawn != null && (!pawns.Contains(specificNeedsTabForPawn) || specificNeedsTabForPawn.Dead))
			{
				specificNeedsTabForPawn = null;
			}
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			float curY = 0f;
			bool flag = false;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.IsColonist)
				{
					if (!flag)
					{
						Widgets.ListSeparator(ref curY, viewRect.width, "CaravanColonists".Translate());
						flag = true;
					}
					DoRow(ref curY, viewRect, rect, scrollPosition, pawn, caravan, ref specificNeedsTabForPawn, doNeeds);
				}
			}
			bool flag2 = false;
			for (int j = 0; j < pawns.Count; j++)
			{
				Pawn pawn2 = pawns[j];
				if (!pawn2.IsColonist)
				{
					if (!flag2)
					{
						Widgets.ListSeparator(ref curY, viewRect.width, "CaravanPrisonersAndAnimals".Translate());
						flag2 = true;
					}
					DoRow(ref curY, viewRect, rect, scrollPosition, pawn2, caravan, ref specificNeedsTabForPawn, doNeeds);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = curY + 30f;
			}
			Widgets.EndScrollView();
		}

		public static Vector2 GetSize(List<Pawn> pawns, float paneTopY, bool doNeeds = true)
		{
			float num = 100f;
			if (doNeeds)
			{
				num += (float)MaxNeedsCount(pawns) * 100f;
			}
			num += 24f;
			Vector2 result = default(Vector2);
			result.x = 103f + num + 16f;
			result.y = Mathf.Min(550f, paneTopY - 30f);
			return result;
		}

		private static int MaxNeedsCount(List<Pawn> pawns)
		{
			int num = 0;
			for (int i = 0; i < pawns.Count; i++)
			{
				GetNeedsToDisplay(pawns[i], tmpNeeds);
				num = Mathf.Max(num, tmpNeeds.Count);
			}
			return num;
		}

		private static void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Vector2 scrollPosition, Pawn pawn, Caravan caravan, ref Pawn specificNeedsTabForPawn, bool doNeeds)
		{
			float num = scrollPosition.y - 50f;
			float num2 = scrollPosition.y + scrollOutRect.height;
			if (curY > num && curY < num2)
			{
				DoRow(new Rect(0f, curY, viewRect.width, 50f), pawn, caravan, ref specificNeedsTabForPawn, doNeeds);
			}
			curY += 50f;
		}

		private static void DoRow(Rect rect, Pawn pawn, Caravan caravan, ref Pawn specificNeedsTabForPawn, bool doNeeds)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			CaravanThingsTabUtility.DoAbandonButton(rect2, pawn, caravan);
			rect2.width -= 24f;
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, pawn);
			rect2.width -= 24f;
			if (!pawn.Dead)
			{
				CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, pawn, ref specificNeedsTabForPawn);
				rect2.width -= 24f;
			}
			Widgets.DrawHighlightIfMouseover(rect2);
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, pawn);
			Rect bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
			GenMapUI.DrawPawnLabel(pawn, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
			if (doNeeds)
			{
				GetNeedsToDisplay(pawn, tmpNeeds);
				float xMax = bgRect.xMax;
				for (int i = 0; i < tmpNeeds.Count; i++)
				{
					Need need = tmpNeeds[i];
					int maxThresholdMarkers = 0;
					bool doTooltip = true;
					Rect rect4 = new Rect(xMax, 0f, 100f, 50f);
					Need_Mood mood = need as Need_Mood;
					if (mood != null)
					{
						maxThresholdMarkers = 1;
						doTooltip = false;
						if (Mouse.IsOver(rect4))
						{
							TooltipHandler.TipRegion(rect4, new TipSignal(() => CustomMoodNeedTooltip(mood), rect4.GetHashCode()));
						}
					}
					need.DrawOnGUI(rect4, maxThresholdMarkers, 10f, drawArrows: false, doTooltip);
					xMax = rect4.xMax;
				}
			}
			if (pawn.Downed)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
				GUI.color = Color.white;
			}
			GUI.EndGroup();
		}

		private static void GetNeedsToDisplay(Pawn p, List<Need> outNeeds)
		{
			outNeeds.Clear();
			List<Need> allNeeds = p.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need need = allNeeds[i];
				if (need.def.showForCaravanMembers)
				{
					outNeeds.Add(need);
				}
			}
			PawnNeedsUIUtility.SortInDisplayOrder(outNeeds);
		}

		private static string CustomMoodNeedTooltip(Need_Mood mood)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(mood.GetTipString());
			PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(mood, thoughtGroupsPresent);
			bool flag = false;
			for (int i = 0; i < thoughtGroupsPresent.Count; i++)
			{
				Thought group = thoughtGroupsPresent[i];
				mood.thoughts.GetMoodThoughts(group, thoughtGroup);
				Thought leadingThoughtInGroup = PawnNeedsUIUtility.GetLeadingThoughtInGroup(thoughtGroup);
				if (leadingThoughtInGroup.VisibleInNeedsTab)
				{
					if (!flag)
					{
						flag = true;
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(leadingThoughtInGroup.LabelCap);
					if (thoughtGroup.Count > 1)
					{
						stringBuilder.Append(" x");
						stringBuilder.Append(thoughtGroup.Count);
					}
					stringBuilder.Append(": ");
					stringBuilder.AppendLine(mood.thoughts.MoodOffsetOfGroup(group).ToString("##0"));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
