using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LudeonTK;

public class Window_DebugTable : Window_Dev
{
	private enum SortMode
	{
		Off,
		Ascending,
		Descending
	}

	private string[,] tableRaw;

	private Vector2 scrollPosition = Vector2.zero;

	private string[,] tableSorted;

	private List<float> colWidths = new List<float>();

	private List<float> rowHeights = new List<float>();

	private int sortColumn = -1;

	private SortMode sortMode;

	private bool[] colVisible;

	private bool[] rowsVisible;

	private float visibleWidth;

	private float visibleHeight;

	private bool focusFilter;

	private readonly QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	private const float ColExtraWidth = 4f;

	private const float RowExtraHeight = 2f;

	private const float HiddenColumnWidth = 10f;

	private const float MouseoverOffset = 2f;

	public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

	public Window_DebugTable(string[,] tables)
	{
		tableRaw = tables;
		colVisible = new bool[tableRaw.GetLength(0)];
		rowsVisible = new bool[tableRaw.GetLength(1)];
		for (int i = 0; i < colVisible.Length; i++)
		{
			colVisible[i] = true;
		}
		for (int j = 0; j < rowsVisible.Length; j++)
		{
			rowsVisible[j] = true;
		}
		doCloseButton = true;
		doCloseX = true;
		focusFilter = true;
		Text.Font = GameFont.Tiny;
		BuildTableSorted();
		visibleWidth = colWidths.Sum();
		visibleHeight = rowHeights.Sum() - rowHeights[0];
	}

	private void BuildTableSorted()
	{
		if (sortMode == SortMode.Off)
		{
			tableSorted = tableRaw;
		}
		else
		{
			List<List<string>> list = new List<List<string>>();
			for (int i = 1; i < tableRaw.GetLength(1); i++)
			{
				list.Add(new List<string>());
				for (int j = 0; j < tableRaw.GetLength(0); j++)
				{
					list[i - 1].Add(tableRaw[j, i]);
				}
			}
			NumericStringComparer comparer = new NumericStringComparer();
			switch (sortMode)
			{
			case SortMode.Ascending:
				list = list.OrderBy((List<string> list2) => list2[sortColumn], comparer).ToList();
				break;
			case SortMode.Descending:
				list = list.OrderByDescending((List<string> list2) => list2[sortColumn], comparer).ToList();
				break;
			case SortMode.Off:
				throw new Exception();
			}
			tableSorted = new string[tableRaw.GetLength(0), tableRaw.GetLength(1)];
			for (int num = 0; num < tableRaw.GetLength(1); num++)
			{
				for (int num2 = 0; num2 < tableRaw.GetLength(0); num2++)
				{
					if (num == 0)
					{
						tableSorted[num2, num] = tableRaw[num2, num];
					}
					else
					{
						tableSorted[num2, num] = list[num - 1][num2];
					}
				}
			}
		}
		colWidths.Clear();
		for (int num3 = 0; num3 < tableRaw.GetLength(0); num3++)
		{
			float item;
			if (colVisible[num3])
			{
				float num4 = 0f;
				for (int num5 = 0; num5 < tableRaw.GetLength(1); num5++)
				{
					float x = Text.CalcSize(tableRaw[num3, num5]).x;
					if (x > num4)
					{
						num4 = x;
					}
				}
				item = num4 + 4f;
			}
			else
			{
				item = 10f;
			}
			colWidths.Add(item);
		}
		rowHeights.Clear();
		for (int num6 = 0; num6 < tableSorted.GetLength(1); num6++)
		{
			float num7 = 0f;
			for (int num8 = 0; num8 < tableSorted.GetLength(0); num8++)
			{
				float y = Text.CalcSize(tableSorted[num8, num6]).y;
				if (y > num7)
				{
					num7 = y;
				}
			}
			rowHeights.Add(num7 + 2f);
		}
	}

	private void RefreshSearchFilter()
	{
		visibleHeight = 0f;
		for (int i = 1; i < tableSorted.GetLength(1); i++)
		{
			bool flag = false;
			for (int j = 0; j < tableSorted.GetLength(0); j++)
			{
				if (quickSearchWidget.filter.Matches(tableSorted[j, i]))
				{
					flag = true;
					break;
				}
			}
			if (rowsVisible[i] != flag)
			{
				scrollPosition = Vector2.zero;
			}
			if (flag)
			{
				visibleHeight += rowHeights[i];
			}
			rowsVisible[i] = flag;
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Tiny;
		Rect outRect = inRect;
		float num = (outRect.y = rowHeights[0] + 2f);
		outRect.yMax -= 40f + num;
		Rect viewRect = new Rect(0f, 0f, colWidths.Sum(), visibleHeight);
		float num3 = 0f;
		for (int i = 0; i < tableSorted.GetLength(0); i++)
		{
			Rect rect = new Rect(num3 - scrollPosition.x, 0f, colWidths[i], rowHeights[0]);
			num3 += colWidths[i];
			MouseoverSounds.DoRegion(rect);
			if (!Mouse.IsOver(rect))
			{
				DevGUI.Label(rect, tableSorted[i, 0]);
				continue;
			}
			DevGUI.DrawHighlight(rect);
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (i != sortColumn)
					{
						sortMode = SortMode.Off;
					}
					switch (sortMode)
					{
					case SortMode.Off:
						sortMode = SortMode.Descending;
						sortColumn = i;
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						break;
					case SortMode.Descending:
						sortMode = SortMode.Ascending;
						sortColumn = i;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						break;
					case SortMode.Ascending:
						sortMode = SortMode.Off;
						sortColumn = -1;
						SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
						break;
					}
					BuildTableSorted();
				}
				else if (Event.current.button == 1)
				{
					colVisible[i] = !colVisible[i];
					SoundDefOf.Crunch.PlayOneShotOnCamera();
					BuildTableSorted();
				}
				Event.current.Use();
			}
			DevGUI.Label(rect, tableSorted[i, 0]);
		}
		Rect rect2 = new Rect
		{
			x = 0f,
			width = 300f,
			y = inRect.yMax - 24f - 4f,
			height = 24f
		};
		quickSearchWidget.OnGUI(rect2, RefreshSearchFilter);
		if (focusFilter)
		{
			quickSearchWidget.Focus();
			focusFilter = false;
		}
		DevGUI.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float num4 = 0f;
		for (int j = 1; j < tableSorted.GetLength(1); j++)
		{
			if (!rowsVisible[j])
			{
				continue;
			}
			if (num4 + rowHeights[j] < viewRect.yMin + scrollPosition.y || num4 > viewRect.yMax + scrollPosition.y)
			{
				num4 += rowHeights[j];
				continue;
			}
			float num5 = 0f;
			bool flag = Mouse.IsOver(new Rect(num5, num4, visibleWidth, rowHeights[j]));
			for (int k = 0; k < tableSorted.GetLength(0); k++)
			{
				Rect rect3 = new Rect(num5, num4, colWidths[k], rowHeights[j]);
				if (flag)
				{
					DevGUI.DrawHighlight(rect3);
				}
				else if (k % 2 == 0)
				{
					DevGUI.DrawLightHighlight(rect3);
				}
				if (colVisible[k])
				{
					DevGUI.Label(rect3, tableSorted[k, j]);
				}
				num5 += colWidths[k];
			}
			num4 += rowHeights[j];
		}
		DevGUI.EndScrollView();
		Text.Font = GameFont.Small;
		if (DevGUI.ButtonText(new Rect(inRect.xMax - 120f, inRect.yMax - 30f, 120f, 30f), "Copy CSV"))
		{
			CopyCSVToClipboard();
			Messages.Message("Copied table data to clipboard in CSV format.", MessageTypeDefOf.PositiveEvent);
		}
	}

	private void MouseOverHeaderRowCell(Rect cell, int col)
	{
		MouseoverSounds.DoRegion(cell);
		if (!Mouse.IsOver(cell) || Event.current.type != EventType.MouseDown)
		{
			return;
		}
		if (Event.current.button == 0)
		{
			if (col != sortColumn)
			{
				sortMode = SortMode.Off;
			}
			switch (sortMode)
			{
			case SortMode.Off:
				sortMode = SortMode.Descending;
				sortColumn = col;
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				break;
			case SortMode.Descending:
				sortMode = SortMode.Ascending;
				sortColumn = col;
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				break;
			case SortMode.Ascending:
				sortMode = SortMode.Off;
				sortColumn = -1;
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				break;
			}
			BuildTableSorted();
		}
		else if (Event.current.button == 1)
		{
			colVisible[col] = !colVisible[col];
			SoundDefOf.Crunch.PlayOneShotOnCamera();
			BuildTableSorted();
		}
		Event.current.Use();
	}

	private static void MouseCenterButtonDrag(ref Vector2 scroll)
	{
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
		{
			Vector2 currentEventDelta = UnityGUIBugsFixer.CurrentEventDelta;
			Event.current.Use();
			if (currentEventDelta != Vector2.zero)
			{
				scroll += -1f * currentEventDelta;
			}
		}
	}

	private static bool IsMouseOverRow(Rect cell)
	{
		cell.xMin -= 9999999f;
		cell.xMax += 9999999f;
		return Mouse.IsOver(cell);
	}

	private static bool IsMouseOverCol(Rect cell)
	{
		cell.yMin -= 9999999f;
		cell.yMax += 9999999f;
		return Mouse.IsOver(cell);
	}

	private static void DrawOpaqueLabel(Rect r, string label, bool highlighted = true, bool selectedRow = false, bool selectedCol = false, bool offsetLabel = false)
	{
		if (offsetLabel)
		{
			r.height += 2f;
		}
		Color color = GUI.color;
		GUI.color = DevGUI.WindowBGFillColor;
		GUI.DrawTexture(r, BaseContent.WhiteTex);
		GUI.color = color;
		if (selectedRow)
		{
			DevGUI.DrawHighlightSelected(r);
		}
		if (selectedCol)
		{
			DevGUI.DrawHighlightSelected(r);
		}
		if (highlighted)
		{
			DevGUI.DrawHighlight(r);
		}
		else
		{
			DevGUI.DrawLightHighlight(r);
		}
		if (offsetLabel)
		{
			r.y += 2f;
		}
		DevGUI.Label(r, label);
	}

	private void CopyCSVToClipboard()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < tableSorted.GetLength(1); i++)
		{
			for (int j = 0; j < tableSorted.GetLength(0); j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(",");
				}
				string text = tableSorted[j, i] ?? "";
				stringBuilder.Append("\"" + text.Replace("\n", " ") + "\"");
			}
			stringBuilder.Append("\n");
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
	}
}
