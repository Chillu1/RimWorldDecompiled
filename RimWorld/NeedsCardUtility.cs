using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class NeedsCardUtility
	{
		private static List<Need> displayNeeds = new List<Need>();

		private static readonly Color MoodColor = new Color(0.1f, 1f, 0.1f);

		private static readonly Color MoodColorNegative = new Color(0.8f, 0.4f, 0.4f);

		private static readonly Color NoEffectColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

		private const float ThoughtHeight = 20f;

		private const float ThoughtSpacing = 4f;

		private const float ThoughtIntervalY = 24f;

		private const float MoodX = 235f;

		private const float MoodNumberWidth = 32f;

		private const float NeedsColumnWidth = 225f;

		public static readonly Vector2 FullSize = new Vector2(580f, 520f);

		private static List<Thought> thoughtGroupsPresent = new List<Thought>();

		private static List<Thought> thoughtGroup = new List<Thought>();

		public static Vector2 GetSize(Pawn pawn)
		{
			UpdateDisplayNeeds(pawn);
			if (pawn.needs.mood != null)
			{
				return FullSize;
			}
			return new Vector2(225f, (float)displayNeeds.Count * Mathf.Min(70f, FullSize.y / (float)displayNeeds.Count));
		}

		public static void DoNeedsMoodAndThoughts(Rect rect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			Rect rect2 = new Rect(rect.x, rect.y, 225f, rect.height);
			DoNeeds(rect2, pawn);
			if (pawn.needs.mood != null)
			{
				DoMoodAndThoughts(new Rect(rect2.xMax, rect.y, rect.width - rect2.width, rect.height), pawn, ref thoughtScrollPosition);
			}
		}

		public static void DoNeeds(Rect rect, Pawn pawn)
		{
			UpdateDisplayNeeds(pawn);
			float num = 0f;
			for (int i = 0; i < displayNeeds.Count; i++)
			{
				Need need = displayNeeds[i];
				Rect rect2 = new Rect(rect.x, rect.y + num, rect.width, Mathf.Min(70f, rect.height / (float)displayNeeds.Count));
				if (!need.def.major)
				{
					if (i > 0 && displayNeeds[i - 1].def.major)
					{
						rect2.y += 10f;
					}
					rect2.width *= 0.73f;
					rect2.height = Mathf.Max(rect2.height * 0.666f, 30f);
				}
				need.DrawOnGUI(rect2);
				num = rect2.yMax;
			}
		}

		private static void DoMoodAndThoughts(Rect rect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, rect.width * 0.8f, 70f);
			pawn.needs.mood.DrawOnGUI(rect2);
			DrawThoughtListing(new Rect(0f, 80f, rect.width, rect.height - 70f - 10f).ContractedBy(10f), pawn, ref thoughtScrollPosition);
			GUI.EndGroup();
		}

		private static void UpdateDisplayNeeds(Pawn pawn)
		{
			displayNeeds.Clear();
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				if (allNeeds[i].ShowOnNeedList)
				{
					displayNeeds.Add(allNeeds[i]);
				}
			}
			PawnNeedsUIUtility.SortInDisplayOrder(displayNeeds);
		}

		private static void DrawThoughtListing(Rect listingRect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			Text.Font = GameFont.Small;
			PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(pawn.needs.mood, thoughtGroupsPresent);
			float height = (float)thoughtGroupsPresent.Count * 24f;
			Widgets.BeginScrollView(listingRect, ref thoughtScrollPosition, new Rect(0f, 0f, listingRect.width - 16f, height));
			Text.Anchor = TextAnchor.MiddleLeft;
			float num = 0f;
			for (int i = 0; i < thoughtGroupsPresent.Count; i++)
			{
				if (DrawThoughtGroup(new Rect(0f, num, listingRect.width - 16f, 20f), thoughtGroupsPresent[i], pawn))
				{
					num += 24f;
				}
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static bool DrawThoughtGroup(Rect rect, Thought group, Pawn pawn)
		{
			try
			{
				pawn.needs.mood.thoughts.GetMoodThoughts(group, thoughtGroup);
				Thought leadingThoughtInGroup = PawnNeedsUIUtility.GetLeadingThoughtInGroup(thoughtGroup);
				if (!leadingThoughtInGroup.VisibleInNeedsTab)
				{
					thoughtGroup.Clear();
					return false;
				}
				if (leadingThoughtInGroup != thoughtGroup[0])
				{
					thoughtGroup.Remove(leadingThoughtInGroup);
					thoughtGroup.Insert(0, leadingThoughtInGroup);
				}
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				if (Mouse.IsOver(rect))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(leadingThoughtInGroup.Description);
					if (group.def.DurationTicks > 5)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
						Thought_Memory thought_Memory = leadingThoughtInGroup as Thought_Memory;
						if (thought_Memory != null)
						{
							if (thoughtGroup.Count == 1)
							{
								stringBuilder.Append("ThoughtExpiresIn".Translate((group.def.DurationTicks - thought_Memory.age).ToStringTicksToPeriod()));
							}
							else
							{
								int num = int.MaxValue;
								int num2 = int.MinValue;
								foreach (Thought_Memory item in thoughtGroup)
								{
									num = Mathf.Min(num, item.age);
									num2 = Mathf.Max(num2, item.age);
								}
								stringBuilder.Append("ThoughtStartsExpiringIn".Translate((group.def.DurationTicks - num2).ToStringTicksToPeriod()));
								stringBuilder.AppendLine();
								stringBuilder.Append("ThoughtFinishesExpiringIn".Translate((group.def.DurationTicks - num).ToStringTicksToPeriod()));
							}
						}
					}
					if (thoughtGroup.Count > 1)
					{
						bool flag = false;
						for (int i = 1; i < thoughtGroup.Count; i++)
						{
							bool flag2 = false;
							for (int j = 0; j < i; j++)
							{
								if (thoughtGroup[i].LabelCap == thoughtGroup[j].LabelCap)
								{
									flag2 = true;
									break;
								}
							}
							if (!flag2)
							{
								if (!flag)
								{
									stringBuilder.AppendLine();
									stringBuilder.AppendLine();
									flag = true;
								}
								stringBuilder.AppendLine("+ " + thoughtGroup[i].LabelCap);
							}
						}
					}
					TooltipHandler.TipRegion(rect, new TipSignal(stringBuilder.ToString(), 7291));
				}
				Text.WordWrap = false;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = new Rect(rect.x + 10f, rect.y, 225f, rect.height);
				rect2.yMin -= 3f;
				rect2.yMax += 3f;
				string text = leadingThoughtInGroup.LabelCap;
				if (thoughtGroup.Count > 1)
				{
					text = text + " x" + thoughtGroup.Count;
				}
				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.MiddleCenter;
				float num3 = pawn.needs.mood.thoughts.MoodOffsetOfGroup(group);
				if (num3 == 0f)
				{
					GUI.color = NoEffectColor;
				}
				else if (num3 > 0f)
				{
					GUI.color = MoodColor;
				}
				else
				{
					GUI.color = MoodColorNegative;
				}
				Widgets.Label(new Rect(rect.x + 235f, rect.y, 32f, rect.height), num3.ToString("##0"));
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.WordWrap = true;
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(string.Concat("Exception in DrawThoughtGroup for ", group.def, " on ", pawn, ": ", ex.ToString()), 3452698);
			}
			return true;
		}
	}
}
