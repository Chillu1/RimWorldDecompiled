using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionUIUtility
	{
		private static bool showAll;

		private const float FactionIconRectSize = 42f;

		private const float FactionIconRectGapX = 24f;

		private const float FactionIconRectGapY = 4f;

		private const float RowMinHeight = 80f;

		private const float LabelRowHeight = 50f;

		private const float NameLeftMargin = 15f;

		private const float FactionIconSpacing = 5f;

		public static void DoWindowContents_NewTemp(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight, Faction scrollToFaction = null)
		{
			Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			if (Prefs.DevMode)
			{
				Widgets.CheckboxLabeled(new Rect(position.width - 120f, 0f, 120f, 24f), "Dev: Show all", ref showAll);
			}
			else
			{
				showAll = false;
			}
			Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
			Rect rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			float num = 0f;
			foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
			{
				if ((!item.IsPlayer && !item.Hidden) || showAll)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, rect.width);
					GUI.color = Color.white;
					if (item == scrollToFaction)
					{
						scrollPosition.y = num;
					}
					num += DrawFactionRow(item, num, rect);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		public static void DoWindowContents(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight)
		{
			DoWindowContents_NewTemp(fillRect, ref scrollPosition, ref scrollViewHeight);
		}

		private static float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
		{
			float num = fillRect.width - 250f - 40f - 90f - 16f - 120f;
			Faction[] array = Find.FactionManager.AllFactionsInViewOrder.Where((Faction f) => f != faction && f.HostileTo(faction) && ((!f.IsPlayer && !f.Hidden) || showAll)).ToArray();
			Rect rect = new Rect(90f, rowY, 250f, 80f);
			Rect r = new Rect(24f, rowY + 4f, 42f, 42f);
			float num2 = 62f;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			DrawFactionIconWithTooltip(r, faction);
			string label = faction.Name.CapitalizeFirst() + "\n" + faction.def.LabelCap + "\n" + ((faction.leader != null) ? (faction.LeaderTitle.CapitalizeFirst() + ": " + faction.leader.Name.ToStringFull) : "");
			Widgets.Label(rect, label);
			Rect rect2 = new Rect(rect.xMax, rowY, 40f, 80f);
			Widgets.InfoCardButton(rect2.x, rect2.y, faction);
			Rect rect3 = new Rect(rect2.xMax, rowY, 90f, 80f);
			if (!faction.IsPlayer)
			{
				string str = (faction.HasGoodwill ? (faction.PlayerGoodwill.ToStringWithSign() + "\n") : "");
				str += faction.PlayerRelationKind.GetLabel();
				if (faction.defeated)
				{
					str += "\n(" + "DefeatedLower".Translate() + ")";
				}
				GUI.color = faction.PlayerRelationKind.GetColor();
				Widgets.Label(rect3, str);
				GUI.color = Color.white;
				if (Mouse.IsOver(rect3))
				{
					TaggedString str2 = (faction.HasGoodwill ? "CurrentGoodwillTip".Translate() : "CurrentRelationTip".Translate());
					if (faction.HasGoodwill && faction.def.permanentEnemy)
					{
						str2 += "\n\n" + "CurrentGoodwillTip_PermanentEnemy".Translate();
					}
					else if (faction.HasGoodwill)
					{
						str2 += "\n\n";
						switch (faction.PlayerRelationKind)
						{
						case FactionRelationKind.Ally:
							str2 += "CurrentGoodwillTip_Ally".Translate(0.ToString("F0"));
							break;
						case FactionRelationKind.Neutral:
							str2 += "CurrentGoodwillTip_Neutral".Translate((-75).ToString("F0"), 75.ToString("F0"));
							break;
						case FactionRelationKind.Hostile:
							str2 += "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
							break;
						}
						if (faction.def.goodwillDailyGain > 0f || faction.def.goodwillDailyFall > 0f)
						{
							float num3 = faction.def.goodwillDailyGain * 60f;
							float num4 = faction.def.goodwillDailyFall * 60f;
							str2 += "\n\n" + "CurrentGoodwillTip_NaturalGoodwill".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), faction.def.naturalColonyGoodwill.max.ToString("F0"));
							if (faction.def.naturalColonyGoodwill.min > -100)
							{
								str2 += " " + "CurrentGoodwillTip_NaturalGoodwillRise".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), num3.ToString("F0"));
							}
							if (faction.def.naturalColonyGoodwill.max < 100)
							{
								str2 += " " + "CurrentGoodwillTip_NaturalGoodwillFall".Translate(faction.def.naturalColonyGoodwill.max.ToString("F0"), num4.ToString("F0"));
							}
						}
					}
					TooltipHandler.TipRegion(rect3, str2);
				}
				if (Mouse.IsOver(rect3))
				{
					GUI.DrawTexture(rect3, TexUI.HighlightTex);
				}
			}
			float xMax = rect3.xMax;
			string text = "EnemyOf".Translate();
			Vector2 vector = Text.CalcSize(text);
			Rect rect4 = new Rect(xMax, rowY + 4f, vector.x + 10f, 42f);
			xMax += rect4.width;
			Widgets.Label(rect4, text);
			for (int i = 0; i < array.Length; i++)
			{
				if (xMax >= rect3.xMax + num)
				{
					xMax = rect3.xMax + rect4.width;
					rowY += vector.y + 5f;
					num2 += vector.y + 5f;
				}
				DrawFactionIconWithTooltip(new Rect(xMax, rowY + 4f, vector.y, vector.y), array[i]);
				xMax += vector.y + 5f;
			}
			return Mathf.Max(80f, num2);
		}

		public static void DrawFactionIconWithTooltip(Rect r, Faction faction)
		{
			GUI.color = faction.Color;
			GUI.DrawTexture(r, faction.def.FactionIcon);
			GUI.color = Color.white;
			if (Mouse.IsOver(r))
			{
				TipSignal tip = new TipSignal(() => faction.Name + "\n\n" + faction.def.description, faction.loadID ^ 0x738AC053);
				TooltipHandler.TipRegion(r, tip);
				Widgets.DrawHighlight(r);
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
			Widgets.Label(rect3, playerRelationKind.GetLabel());
			curY += rect3.height;
			GUI.color = Color.white;
			GenUI.ResetLabelAlign();
		}
	}
}
