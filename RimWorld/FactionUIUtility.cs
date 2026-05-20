using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class FactionUIUtility
{
	private static bool showAll;

	private static List<Faction> visibleFactions = new List<Faction>();

	private const float FactionIconRectSize = 42f;

	private const float FactionIconRectGapX = 24f;

	private const float RowHeight = 80f;

	private const float LabelRowHeight = 50f;

	private const float FactionIconSpacing = 5f;

	private const float IdeoIconSpacing = 5f;

	private const float BasicsColumnWidth = 300f;

	private const float InfoColumnWidth = 40f;

	private const float IdeosColumnWidth = 60f;

	private const float RelationsColumnWidth = 70f;

	private const float NaturalGoodwillColumnWidth = 54f;

	private static List<int> tmpTicks = new List<int>();

	private static List<int> tmpCustomGoodwill = new List<int>();

	public static void DoWindowContents(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight, Faction scrollToFaction = null)
	{
		Rect rect = new Rect(0f, 0f, fillRect.width, fillRect.height);
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		if (Prefs.DevMode)
		{
			Widgets.CheckboxLabeled(new Rect(rect.width - 120f, 0f, 120f, 24f), "DEV: Show all", ref showAll);
		}
		else
		{
			showAll = false;
		}
		Rect outRect = new Rect(0f, 50f, rect.width, rect.height - 50f);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		visibleFactions.Clear();
		foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
		{
			if ((!item.IsPlayer && !item.Hidden) || showAll)
			{
				visibleFactions.Add(item);
			}
		}
		if (visibleFactions.Count > 0)
		{
			Widgets.Label(new Rect(614f, 50f, 200f, 100f), "EnemyOf".Translate());
			outRect.yMin += Text.LineHeight;
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
			float num = 0f;
			int num2 = 0;
			foreach (Faction visibleFaction in visibleFactions)
			{
				if ((!visibleFaction.IsPlayer && !visibleFaction.Hidden) || showAll)
				{
					if (visibleFaction == scrollToFaction)
					{
						scrollPosition.y = num;
					}
					if (num2 % 2 == 1)
					{
						Widgets.DrawLightHighlight(new Rect(rect2.x, num, rect2.width, 80f));
					}
					num += DrawFactionRow(visibleFaction, num, rect2);
					num2++;
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = num;
			}
			Widgets.EndScrollView();
		}
		else
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "NoFactions".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
		}
		Widgets.EndGroup();
	}

	private static float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
	{
		float num = fillRect.width - 300f - 40f - 70f - 54f - 16f - 120f;
		Faction[] array = Find.FactionManager.AllFactionsInViewOrder.Where((Faction f) => f != faction && f.HostileTo(faction) && ((!f.IsPlayer && !f.Hidden) || showAll)).ToArray();
		Rect rect = new Rect(90f, rowY, 300f, 80f);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect position = new Rect(24f, rowY + (rect.height - 42f) / 2f, 42f, 42f);
		GUI.color = faction.Color;
		GUI.DrawTexture(position, faction.def.FactionIcon);
		GUI.color = Color.white;
		string label = faction.Name.CapitalizeFirst() + "\n" + faction.def.LabelCap + "\n" + ((faction.leader != null) ? (faction.LeaderTitle.CapitalizeFirst() + ": " + faction.leader.Name.ToStringFull) : "");
		Widgets.Label(rect, label);
		Rect rect2 = new Rect(0f, rowY, rect.xMax, 80f);
		if (Mouse.IsOver(rect2))
		{
			TipSignal tip = new TipSignal(() => faction.Name.Colorize(ColoredText.TipSectionTitleColor) + "\n" + faction.def.LabelCap.Resolve() + "\n\n" + faction.def.Description, faction.loadID ^ 0x738AC053);
			TooltipHandler.TipRegion(rect2, tip);
			Widgets.DrawHighlight(rect2);
		}
		if (Widgets.ButtonInvisible(rect2, doMouseoverSound: false))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(faction));
		}
		Rect rect3 = new Rect(rect.xMax, rowY, 40f, 80f);
		Widgets.InfoCardButtonCentered(rect3, faction);
		Rect rect4 = new Rect(rect3.xMax, rowY, 60f, 80f);
		if (ModsConfig.IdeologyActive && !Find.IdeoManager.classicMode)
		{
			if (faction.ideos != null)
			{
				float num2 = rect4.x;
				float num3 = rect4.y;
				if (faction.ideos.PrimaryIdeo != null)
				{
					if (num2 + 40f > rect4.xMax)
					{
						num2 = rect4.x;
						num3 += 45f;
					}
					Rect rect5 = new Rect(num2, num3 + (rect4.height - 40f) / 2f, 40f, 40f);
					IdeoUIUtility.DoIdeoIcon(rect5, faction.ideos.PrimaryIdeo, doTooltip: true, delegate
					{
						IdeoUIUtility.OpenIdeoInfo(faction.ideos.PrimaryIdeo);
					});
					num2 += rect5.width + 5f;
					num2 = rect4.x;
					num3 += 45f;
				}
				List<Ideo> minor = faction.ideos.IdeosMinorListForReading;
				int i;
				for (i = 0; i < minor.Count; i++)
				{
					if (num2 + 22f > rect4.xMax)
					{
						num2 = rect4.x;
						num3 += 27f;
					}
					if (num3 + 22f > rect4.yMax)
					{
						break;
					}
					Rect rect6 = new Rect(num2, num3 + (rect4.height - 22f) / 2f, 22f, 22f);
					IdeoUIUtility.DoIdeoIcon(rect6, minor[i], doTooltip: true, delegate
					{
						IdeoUIUtility.OpenIdeoInfo(minor[i]);
					});
					num2 += rect6.width + 5f;
				}
			}
		}
		else
		{
			rect4.width = 0f;
		}
		Rect rect7 = new Rect(rect4.xMax, rowY, 70f, 80f);
		if (!faction.IsPlayer)
		{
			string text = faction.PlayerRelationKind.GetLabelCap();
			if (faction.defeated)
			{
				text = text.Colorize(ColorLibrary.Grey);
			}
			GUI.color = faction.PlayerRelationKind.GetColor();
			Text.Anchor = TextAnchor.MiddleCenter;
			if (faction.HasGoodwill && !faction.def.permanentEnemy)
			{
				Widgets.Label(new Rect(rect7.x, rect7.y - 10f, rect7.width, rect7.height), text);
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(rect7.x, rect7.y + 10f, rect7.width, rect7.height), faction.PlayerGoodwill.ToStringWithSign());
				Text.Font = GameFont.Small;
			}
			else
			{
				Widgets.Label(rect7, text);
			}
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
			if (Mouse.IsOver(rect7))
			{
				TaggedString taggedString = "";
				if (faction.def.permanentEnemy)
				{
					taggedString = "CurrentGoodwillTip_PermanentEnemy".Translate();
				}
				else if (faction.HasGoodwill)
				{
					taggedString = "Goodwill".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + (faction.PlayerGoodwill.ToStringWithSign() + ", " + faction.PlayerRelationKind.GetLabel()).Colorize(faction.PlayerRelationKind.GetColor());
					TaggedString ongoingEvents = GetOngoingEvents(faction);
					if (!ongoingEvents.NullOrEmpty())
					{
						taggedString += "\n\n" + "OngoingEvents".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + ongoingEvents;
					}
					TaggedString recentEvents = GetRecentEvents(faction);
					if (!recentEvents.NullOrEmpty())
					{
						taggedString += "\n\n" + "RecentEvents".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + recentEvents;
					}
					string s = "";
					switch (faction.PlayerRelationKind)
					{
					case FactionRelationKind.Ally:
						s = "CurrentGoodwillTip_Ally".Translate(0.ToString("F0"));
						break;
					case FactionRelationKind.Neutral:
						s = "CurrentGoodwillTip_Neutral".Translate((-75).ToString("F0"), 75.ToString("F0"));
						break;
					case FactionRelationKind.Hostile:
						s = "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
						break;
					}
					taggedString += "\n\n" + s.Colorize(ColoredText.SubtleGrayColor);
				}
				if (taggedString != "")
				{
					TooltipHandler.TipRegion(rect7, taggedString);
				}
				Widgets.DrawHighlight(rect7);
			}
		}
		Rect rect8 = new Rect(rect7.xMax, rowY, 54f, 80f);
		if (!faction.IsPlayer && faction.HasGoodwill && !faction.def.permanentEnemy)
		{
			FactionRelationKind relationKindForGoodwill = GetRelationKindForGoodwill(faction.NaturalGoodwill);
			GUI.color = relationKindForGoodwill.GetColor();
			Rect rect9 = rect8.ContractedBy(7f);
			rect9.y = rowY + 30f;
			rect9.height = 20f;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.DrawRectFast(rect9, Color.black);
			Widgets.Label(rect9, faction.NaturalGoodwill.ToStringWithSign());
			GenUI.ResetLabelAlign();
			GUI.color = Color.white;
			if (Mouse.IsOver(rect8))
			{
				TaggedString taggedString2 = "NaturalGoodwill".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + faction.NaturalGoodwill.ToStringWithSign().Colorize(relationKindForGoodwill.GetColor());
				int goodwill = Mathf.Clamp(faction.NaturalGoodwill - 50, -100, 100);
				int goodwill2 = Mathf.Clamp(faction.NaturalGoodwill + 50, -100, 100);
				taggedString2 += "\n" + "NaturalGoodwillRange".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + goodwill.ToString().Colorize(GetRelationKindForGoodwill(goodwill).GetColor()) + " " + "RangeTo".Translate() + " " + goodwill2.ToString().Colorize(GetRelationKindForGoodwill(goodwill2).GetColor());
				TaggedString naturalGoodwillExplanation = GetNaturalGoodwillExplanation(faction);
				if (!naturalGoodwillExplanation.NullOrEmpty())
				{
					taggedString2 += "\n\n" + "AffectedBy".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n" + naturalGoodwillExplanation;
				}
				taggedString2 += "\n\n" + "NaturalGoodwillDescription".Translate(1.25f.ToStringPercent()).Colorize(ColoredText.SubtleGrayColor);
				TooltipHandler.TipRegion(rect8, taggedString2);
				Widgets.DrawHighlight(rect8);
			}
		}
		float num4 = rect8.xMax + 17f;
		for (int num5 = 0; num5 < array.Length; num5++)
		{
			if (num4 >= rect8.xMax + num)
			{
				num4 = rect8.xMax;
				rowY += 27f;
			}
			DrawFactionIconWithTooltip(new Rect(num4, rowY + 29f, 22f, 22f), array[num5]);
			num4 += 27f;
		}
		Text.Anchor = TextAnchor.UpperLeft;
		return 80f;
	}

	public static void DrawFactionIconWithTooltip(Rect r, Faction faction)
	{
		GUI.color = faction.Color;
		GUI.DrawTexture(r, faction.def.FactionIcon);
		GUI.color = Color.white;
		if (Mouse.IsOver(r))
		{
			TipSignal tip = new TipSignal(() => faction.Name.Colorize(ColoredText.TipSectionTitleColor) + "\n" + faction.def.LabelCap.Resolve() + "\n\n" + faction.def.Description, faction.loadID ^ 0x738AC053);
			TooltipHandler.TipRegion(r, tip);
			Widgets.DrawHighlight(r);
		}
		if (Widgets.ButtonInvisible(r, doMouseoverSound: false))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(faction));
		}
	}

	public static void DrawRelatedFactionInfo(Rect rect, Faction faction, ref float curY)
	{
		Text.Anchor = TextAnchor.LowerRight;
		curY += 10f;
		FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
		string text = faction.Name.CapitalizeFirst() + "\n" + "goodwill".Translate().CapitalizeFirst() + ": " + faction.PlayerGoodwill.ToStringWithSign();
		GUI.color = Color.gray;
		Rect rect2 = new Rect(rect.x, curY, rect.width, Text.CalcHeight(text, rect.width));
		Widgets.Label(rect2, text);
		curY += rect2.height;
		GUI.color = playerRelationKind.GetColor();
		Rect rect3 = new Rect(rect2.x, curY - 7f, rect2.width, 25f);
		Widgets.Label(rect3, playerRelationKind.GetLabelCap());
		curY += rect3.height;
		GUI.color = Color.white;
		GenUI.ResetLabelAlign();
	}

	private static TaggedString GetRecentEvents(Faction other)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<HistoryEventDef> allDefsListForReading = DefDatabase<HistoryEventDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			int recentCountWithinTicks = Find.HistoryEventsManager.GetRecentCountWithinTicks(allDefsListForReading[i], 3600000, other);
			if (recentCountWithinTicks <= 0)
			{
				continue;
			}
			Find.HistoryEventsManager.GetRecent(allDefsListForReading[i], 3600000, tmpTicks, tmpCustomGoodwill, other);
			int num = 0;
			for (int j = 0; j < tmpTicks.Count; j++)
			{
				num += tmpCustomGoodwill[j];
			}
			if (num != 0)
			{
				string text = "- " + allDefsListForReading[i].LabelCap;
				if (recentCountWithinTicks != 1)
				{
					text = text + " x" + recentCountWithinTicks;
				}
				text = text + ": " + num.ToStringWithSign().Colorize((num >= 0) ? FactionRelationKind.Ally.GetColor() : FactionRelationKind.Hostile.GetColor());
				stringBuilder.AppendInNewLine(text);
			}
		}
		return stringBuilder.ToString();
	}

	private static FactionRelationKind GetRelationKindForGoodwill(int goodwill)
	{
		if (goodwill <= -75)
		{
			return FactionRelationKind.Hostile;
		}
		if (goodwill >= 75)
		{
			return FactionRelationKind.Ally;
		}
		return FactionRelationKind.Neutral;
	}

	private static TaggedString GetOngoingEvents(Faction other)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<GoodwillSituationManager.CachedSituation> situations = Find.GoodwillSituationManager.GetSituations(other);
		for (int i = 0; i < situations.Count; i++)
		{
			if (situations[i].maxGoodwill < 100)
			{
				string text = "- " + situations[i].def.Worker.GetPostProcessedLabelCap(other);
				text = text + ": " + (situations[i].maxGoodwill.ToStringWithSign() + " " + "max".Translate()).Colorize(FactionRelationKind.Hostile.GetColor());
				stringBuilder.AppendInNewLine(text);
			}
		}
		return stringBuilder.ToString();
	}

	private static TaggedString GetNaturalGoodwillExplanation(Faction other)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<GoodwillSituationManager.CachedSituation> situations = Find.GoodwillSituationManager.GetSituations(other);
		for (int i = 0; i < situations.Count; i++)
		{
			if (situations[i].naturalGoodwillOffset != 0)
			{
				string text = "- " + situations[i].def.Worker.GetPostProcessedLabelCap(other);
				text = text + ": " + situations[i].naturalGoodwillOffset.ToStringWithSign().Colorize((situations[i].naturalGoodwillOffset >= 0) ? FactionRelationKind.Ally.GetColor() : FactionRelationKind.Hostile.GetColor());
				stringBuilder.AppendInNewLine(text);
			}
		}
		return stringBuilder.ToString();
	}
}
