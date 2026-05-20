using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class RecordsCardUtility
{
	private static Vector2 scrollPosition;

	private static float listHeight;

	private const float RecordsLeftPadding = 8f;

	private static List<RecordDef> allRecordsSorted;

	private static List<RecordDef> AllRecordsSorted
	{
		get
		{
			if (allRecordsSorted == null)
			{
				allRecordsSorted = DefDatabase<RecordDef>.AllDefsListForReading.OrderBy((RecordDef x) => x.displayOrder).ToList();
			}
			return allRecordsSorted;
		}
	}

	public static void DrawRecordsCard(Rect rect, Pawn pawn)
	{
		Text.Font = GameFont.Small;
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, listHeight);
		Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
		Rect leftRect = rect2;
		leftRect.width *= 0.5f;
		Rect rightRect = rect2;
		rightRect.x = leftRect.xMax;
		rightRect.width = rect2.width - rightRect.x;
		leftRect.xMax -= 6f;
		rightRect.xMin += 6f;
		float a = DrawTimeRecords(leftRect, pawn);
		float b = DrawMiscRecords(rightRect, pawn);
		listHeight = Mathf.Max(a, b) + 100f;
		Widgets.EndScrollView();
	}

	private static float DrawTimeRecords(Rect leftRect, Pawn pawn)
	{
		List<RecordDef> list = AllRecordsSorted;
		float curY = 0f;
		Widgets.BeginGroup(leftRect);
		Widgets.ListSeparator(ref curY, leftRect.width, "TimeRecordsCategory".Translate());
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].type == RecordType.Time)
			{
				curY += DrawRecord(8f, curY, leftRect.width - 8f, list[i], pawn);
			}
		}
		Widgets.EndGroup();
		return curY;
	}

	private static float DrawMiscRecords(Rect rightRect, Pawn pawn)
	{
		List<RecordDef> list = AllRecordsSorted;
		float curY = 0f;
		Widgets.BeginGroup(rightRect);
		Widgets.ListSeparator(ref curY, rightRect.width, "MiscRecordsCategory".Translate());
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].type == RecordType.Int || list[i].type == RecordType.Float)
			{
				curY += DrawRecord(8f, curY, rightRect.width - 8f, list[i], pawn);
			}
		}
		Widgets.EndGroup();
		return curY;
	}

	private static float DrawRecord(float x, float y, float width, RecordDef record, Pawn pawn)
	{
		float num = width * 0.45f;
		string text = ((record.type != RecordType.Time) ? pawn.records.GetValue(record).ToString("0.##") : pawn.records.GetAsInt(record).ToStringTicksToPeriod());
		Rect rect = new Rect(8f, y, width, Text.CalcHeight(text, num));
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		Rect rect2 = rect;
		rect2.width -= num;
		Widgets.Label(rect2, record.LabelCap);
		Rect rect3 = rect;
		rect3.x = rect2.xMax;
		rect3.width = num;
		Widgets.Label(rect3, text);
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, new TipSignal(() => record.description, record.GetHashCode()));
		}
		return rect.height;
	}
}
