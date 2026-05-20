using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class NeedsCardUtility
{
	private static List<Need> displayNeeds = new List<Need>();

	public static readonly Color MoodColor = new Color(0.1f, 1f, 0.1f);

	public static readonly Color MoodColorNegative = new Color(0.8f, 0.4f, 0.4f);

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
		Widgets.BeginGroup(rect);
		Rect rect2 = new Rect(0f, 0f, rect.width * 0.8f, 70f);
		pawn.needs.mood.DrawOnGUI(rect2);
		DrawThoughtListing(new Rect(0f, 80f, rect.width, rect.height - 70f - 10f).ContractedBy(10f), pawn, ref thoughtScrollPosition);
		Widgets.EndGroup();
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
		IdeoUIUtility.DrawExtraThoughtInfoFromIdeo(pawn, ref listingRect);
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
			if (thoughtGroup.Count == 0)
			{
				return false;
			}
			Thought leadingThought = PawnNeedsUIUtility.GetLeadingThoughtInGroup(thoughtGroup);
			if (!leadingThought.VisibleInNeedsTab)
			{
				thoughtGroup.Clear();
				return false;
			}
			if (leadingThought != thoughtGroup[0])
			{
				thoughtGroup.Remove(leadingThought);
				thoughtGroup.Insert(0, leadingThought);
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (Mouse.IsOver(rect))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(leadingThought.LabelCap.AsTipTitle()).AppendLine().AppendLine();
				if (pawn.DevelopmentalStage.Baby())
				{
					stringBuilder.AppendLine(leadingThought.BabyTalk);
					stringBuilder.AppendLine();
					stringBuilder.AppendTagged(("Translation".Translate() + ": " + leadingThought.Description).Colorize(ColoredText.SubtleGrayColor));
				}
				else
				{
					stringBuilder.Append(leadingThought.Description);
				}
				int durationTicks = group.DurationTicks;
				if (durationTicks > 5)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					if (leadingThought is Thought_Memory thought_Memory)
					{
						if (thoughtGroup.Count == 1)
						{
							stringBuilder.AppendTagged("ThoughtExpiresIn".Translate((durationTicks - thought_Memory.age).ToStringTicksToPeriod()));
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
							stringBuilder.AppendTagged("ThoughtStartsExpiringIn".Translate((durationTicks - num2).ToStringTicksToPeriod()));
							stringBuilder.AppendLine();
							stringBuilder.AppendTagged("ThoughtFinishesExpiringIn".Translate((durationTicks - num).ToStringTicksToPeriod()));
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
			string text = leadingThought.LabelCap;
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
			if (ModsConfig.IdeologyActive && leadingThought.sourcePrecept != null && !Find.IdeoManager.classicMode)
			{
				IdeoUIUtility.DoIdeoIcon(new Rect(rect.x + 235f + 32f + 10f, rect.y, 20f, 20f), leadingThought.sourcePrecept.ideo, doTooltip: false, delegate
				{
					IdeoUIUtility.OpenIdeoInfo(leadingThought.sourcePrecept.ideo);
				});
			}
		}
		catch (Exception ex)
		{
			Log.ErrorOnce("Exception in DrawThoughtGroup for " + group.def?.ToString() + " on " + pawn?.ToString() + ": " + ex, 3452698);
		}
		return true;
	}
}
