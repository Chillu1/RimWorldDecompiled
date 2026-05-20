using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class EnvironmentStatsDrawer
{
	private const float StatLabelColumnWidth = 110f;

	private const float StatGutterColumnWidth = 10f;

	private const float ScoreColumnWidth = 50f;

	private const float ScoreStageLabelColumnWidth = 160f;

	private static readonly Color RelatedStatColor = new Color(0.85f, 0.85f, 0.85f);

	private static readonly Color UnrelatedStatColor = Color.gray;

	private const float DistFromMouse = 26f;

	public const float WindowPadding = 12f;

	private const float LineHeight = 23f;

	private const float FootnoteHeight = 23f;

	private const float TitleHeight = 30f;

	private const float SpaceBetweenLines = 2f;

	private const float SpaceBetweenColumns = 35f;

	private static int DisplayedRoomStatsCount
	{
		get
		{
			int num = 0;
			List<RoomStatDef> allDefsListForReading = DefDatabase<RoomStatDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (!allDefsListForReading[i].isHidden || DebugViewSettings.showAllRoomStats)
				{
					num++;
				}
			}
			return num;
		}
	}

	private static bool ShouldShowWindowNow()
	{
		if (!ShouldShowRoomStats() && !ShouldShowBeauty())
		{
			return false;
		}
		if (Mouse.IsInputBlockedNow)
		{
			return false;
		}
		return true;
	}

	private static bool ShouldShowRoomStats()
	{
		if (!Find.PlaySettings.showRoomStats)
		{
			return false;
		}
		if (Find.CurrentMap == null)
		{
			return false;
		}
		if (!UI.MouseCell().InBounds(Find.CurrentMap) || UI.MouseCell().Fogged(Find.CurrentMap))
		{
			return false;
		}
		Room room = UI.MouseCell().GetRoom(Find.CurrentMap);
		if (room != null)
		{
			return room.Role != RoomRoleDefOf.None;
		}
		return false;
	}

	private static bool ShouldShowBeauty()
	{
		if (!Find.PlaySettings.showBeauty)
		{
			return false;
		}
		if (!UI.MouseCell().InBounds(Find.CurrentMap) || UI.MouseCell().Fogged(Find.CurrentMap))
		{
			return false;
		}
		return UI.MouseCell().GetRoom(Find.CurrentMap) != null;
	}

	public static void EnvironmentStatsOnGUI()
	{
		if (Event.current.type == EventType.Repaint && ShouldShowWindowNow())
		{
			DrawInfoWindow();
		}
	}

	private static void DrawInfoWindow()
	{
		Text.Font = GameFont.Small;
		Rect windowRect = GetWindowRect(ShouldShowBeauty(), ShouldShowRoomStats());
		Find.WindowStack.ImmediateWindow(74975, windowRect, WindowLayer.GameUI, delegate
		{
			FillWindow(windowRect);
		});
	}

	public static Rect GetWindowRect(bool shouldShowBeauty, bool shouldShowRoomStats)
	{
		Rect result = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 424f, 24f);
		int num = 0;
		if (shouldShowBeauty)
		{
			num++;
			result.height += 25f;
		}
		if (shouldShowRoomStats)
		{
			num++;
			result.height += 23f;
			result.height += (float)DisplayedRoomStatsCount * 25f + 23f;
		}
		result.height += 13f * (float)(num - 1);
		result.x += 26f;
		result.y += 26f;
		if (result.xMax > (float)UI.screenWidth)
		{
			result.x -= result.width + 52f;
		}
		if (result.yMax > (float)UI.screenHeight)
		{
			result.y -= result.height + 52f;
		}
		return result;
	}

	private static void FillWindow(Rect windowRect)
	{
		Text.Font = GameFont.Small;
		float curY = 12f;
		int dividingLinesSeen = 0;
		if (ShouldShowBeauty())
		{
			DrawDividingLineIfNecessary();
			float beauty = BeautyUtility.AverageBeautyPerceptible(UI.MouseCell(), Find.CurrentMap);
			Rect rect = new Rect(22f, curY, windowRect.width - 24f - 10f, 100f);
			GUI.color = BeautyDrawer.BeautyColor(beauty, 40f);
			Widgets.Label(rect, "BeautyHere".Translate() + ": " + beauty.ToString("F1"));
			curY += 25f;
		}
		if (ShouldShowRoomStats())
		{
			DrawDividingLineIfNecessary();
			DoRoomInfo(UI.MouseCell().GetRoom(Find.CurrentMap), ref curY, windowRect);
		}
		GUI.color = Color.white;
		void DrawDividingLineIfNecessary()
		{
			dividingLinesSeen++;
			if (dividingLinesSeen > 1)
			{
				curY += 5f;
				GUI.color = new Color(1f, 1f, 1f, 0.4f);
				Widgets.DrawLineHorizontal(12f, curY, windowRect.width - 24f);
				GUI.color = Color.white;
				curY += 8f;
			}
		}
	}

	public static void DrawRoomOverlays()
	{
		if (Find.PlaySettings.showBeauty && UI.MouseCell().InBounds(Find.CurrentMap))
		{
			GenUI.RenderMouseoverBracket();
		}
		if (ShouldShowWindowNow() && ShouldShowRoomStats())
		{
			Room room = UI.MouseCell().GetRoom(Find.CurrentMap);
			if (room != null && room.Role != RoomRoleDefOf.None)
			{
				room.DrawFieldEdges();
			}
		}
	}

	public static void DoRoomInfo(Room room, ref float curY, Rect windowRect)
	{
		Rect rect = new Rect(12f, curY, windowRect.width - 24f, 100f);
		GUI.color = Color.white;
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(rect.x + 10f, curY, rect.width - 10f, rect.height), room.GetRoomRoleLabel().CapitalizeFirst());
		curY += 30f;
		Text.Font = GameFont.Small;
		Text.WordWrap = false;
		int num = 0;
		bool flag = false;
		for (int i = 0; i < DefDatabase<RoomStatDef>.AllDefsListForReading.Count; i++)
		{
			RoomStatDef roomStatDef = DefDatabase<RoomStatDef>.AllDefsListForReading[i];
			if (!roomStatDef.isHidden || DebugViewSettings.showAllRoomStats)
			{
				float stat = room.GetStat(roomStatDef);
				RoomStatScoreStage scoreStage = roomStatDef.GetScoreStage(stat);
				GUI.color = Color.white;
				Rect rect2 = new Rect(rect.x, curY, rect.width, 23f);
				if (num % 2 == 1)
				{
					Widgets.DrawLightHighlight(rect2);
				}
				Rect rect3 = new Rect(rect.x, curY, 10f, 23f);
				if (room.Role.IsStatRelated(roomStatDef))
				{
					flag = true;
					Widgets.Label(rect3, "*");
					GUI.color = RelatedStatColor;
				}
				else
				{
					GUI.color = UnrelatedStatColor;
				}
				Rect rect4 = new Rect(rect3.xMax, curY, 110f, 23f);
				Widgets.Label(rect4, roomStatDef.LabelCap);
				Rect rect5 = new Rect(rect4.xMax + 35f, curY, 50f, 23f);
				string label = roomStatDef.ScoreToString(stat);
				Widgets.Label(rect5, label);
				Widgets.Label(new Rect(rect5.xMax + 35f, curY, 160f, 23f), (scoreStage == null) ? "" : scoreStage.label.CapitalizeFirst());
				curY += 25f;
				num++;
			}
		}
		if (flag)
		{
			GUI.color = Color.grey;
			Text.Font = GameFont.Tiny;
			Widgets.Label(new Rect(rect.x, curY, rect.width, 23f), "* " + "StatRelatesToCurrentRoom".Translate());
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
		}
		Text.WordWrap = true;
	}
}
