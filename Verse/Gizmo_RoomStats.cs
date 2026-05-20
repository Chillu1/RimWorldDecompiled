using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Gizmo_RoomStats : Gizmo
{
	private Building building;

	private static readonly Color RoomStatsColor = new Color(0.75f, 0.75f, 0.75f);

	private Room Room => GetRoomToShowStatsFor(building);

	public Gizmo_RoomStats(Building building)
	{
		this.building = building;
		Order = -100f;
	}

	public override float GetWidth(float maxWidth)
	{
		return Mathf.Min(300f, maxWidth);
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Room room = Room;
		if (room == null)
		{
			return new GizmoResult(GizmoState.Clear);
		}
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Widgets.DrawWindowBackground(rect);
		Text.WordWrap = false;
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero().ContractedBy(10f);
		Text.Font = GameFont.Small;
		Rect rect3 = new Rect(rect2.x, rect2.y - 2f, rect2.width, 100f);
		float stat = room.GetStat(RoomStatDefOf.Impressiveness);
		RoomStatScoreStage scoreStage = RoomStatDefOf.Impressiveness.GetScoreStage(stat);
		string str = room.Role.PostProcessedLabelCap(room) + ", " + scoreStage.label + " (" + RoomStatDefOf.Impressiveness.ScoreToString(stat) + ")";
		Widgets.Label(rect3, str.Truncate(rect3.width));
		float num = rect2.y + Text.LineHeight + Text.SpaceBetweenLines + 7f;
		GUI.color = RoomStatsColor;
		Text.Font = GameFont.Tiny;
		List<RoomStatDef> allDefsListForReading = DefDatabase<RoomStatDef>.AllDefsListForReading;
		int num2 = 0;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (!allDefsListForReading[i].isHidden && allDefsListForReading[i] != RoomStatDefOf.Impressiveness)
			{
				float stat2 = room.GetStat(allDefsListForReading[i]);
				RoomStatScoreStage scoreStage2 = allDefsListForReading[i].GetScoreStage(stat2);
				Rect rect4 = ((num2 % 2 != 0) ? new Rect(rect2.x + rect2.width / 2f, num, rect2.width / 2f, 100f) : new Rect(rect2.x, num, rect2.width / 2f, 100f));
				string str2 = scoreStage2.label.CapitalizeFirst() + " (" + allDefsListForReading[i].ScoreToString(stat2) + ")";
				Widgets.Label(rect4, str2.Truncate(rect4.width));
				if (num2 % 2 == 1)
				{
					num += Text.LineHeight + Text.SpaceBetweenLines;
				}
				num2++;
			}
		}
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Widgets.EndGroup();
		Text.WordWrap = true;
		GenUI.AbsorbClicksInRect(rect);
		if (Mouse.IsOver(rect))
		{
			Rect windowRect = EnvironmentStatsDrawer.GetWindowRect(shouldShowBeauty: false, shouldShowRoomStats: true);
			Find.WindowStack.ImmediateWindow(74975, windowRect, WindowLayer.Super, delegate
			{
				float curY = 12f;
				EnvironmentStatsDrawer.DoRoomInfo(room, ref curY, windowRect);
			});
			return new GizmoResult(GizmoState.Mouseover);
		}
		return new GizmoResult(GizmoState.Clear);
	}

	public override void GizmoUpdateOnMouseover()
	{
		base.GizmoUpdateOnMouseover();
		Room?.DrawFieldEdges();
	}

	public static Room GetRoomToShowStatsFor(Building building)
	{
		if (!building.Spawned || building.Fogged())
		{
			return null;
		}
		Room room = null;
		if (building.def.passability != Traversability.Impassable)
		{
			room = building.GetRoom();
		}
		else if (building.def.hasInteractionCell)
		{
			room = building.InteractionCell.GetRoom(building.Map);
		}
		else
		{
			CellRect cellRect = building.OccupiedRect().ExpandedBy(1);
			foreach (IntVec3 item in cellRect)
			{
				if (cellRect.IsOnEdge(item))
				{
					room = item.GetRoom(building.Map);
					if (IsValid(room))
					{
						break;
					}
				}
			}
		}
		if (!IsValid(room))
		{
			return null;
		}
		return room;
		static bool IsValid(Room r)
		{
			if (r != null && !r.Fogged)
			{
				return r.Role != RoomRoleDefOf.None;
			}
			return false;
		}
	}
}
