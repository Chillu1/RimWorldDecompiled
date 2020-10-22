using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnTable
	{
		private PawnTableDef def;

		private Func<IEnumerable<Pawn>> pawnsGetter;

		private int minTableWidth;

		private int maxTableWidth;

		private int minTableHeight;

		private int maxTableHeight;

		private Vector2 fixedSize;

		private bool hasFixedSize;

		private bool dirty;

		private List<bool> columnAtMaxWidth = new List<bool>();

		private List<bool> columnAtOptimalWidth = new List<bool>();

		private Vector2 scrollPosition;

		private PawnColumnDef sortByColumn;

		private bool sortDescending;

		private Vector2 cachedSize;

		private List<Pawn> cachedPawns = new List<Pawn>();

		private List<float> cachedColumnWidths = new List<float>();

		private List<float> cachedRowHeights = new List<float>();

		private List<LookTargets> cachedLookTargets = new List<LookTargets>();

		private float cachedHeaderHeight;

		private float cachedHeightNoScrollbar;

		public List<PawnColumnDef> ColumnsListForReading => def.columns;

		public PawnColumnDef SortingBy => sortByColumn;

		public bool SortingDescending
		{
			get
			{
				if (SortingBy != null)
				{
					return sortDescending;
				}
				return false;
			}
		}

		public Vector2 Size
		{
			get
			{
				RecacheIfDirty();
				return cachedSize;
			}
		}

		public float HeightNoScrollbar
		{
			get
			{
				RecacheIfDirty();
				return cachedHeightNoScrollbar;
			}
		}

		public float HeaderHeight
		{
			get
			{
				RecacheIfDirty();
				return cachedHeaderHeight;
			}
		}

		public List<Pawn> PawnsListForReading
		{
			get
			{
				RecacheIfDirty();
				return cachedPawns;
			}
		}

		public PawnTable(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
		{
			this.def = def;
			this.pawnsGetter = pawnsGetter;
			SetMinMaxSize(def.minWidth, uiWidth, 0, uiHeight);
			SetDirty();
		}

		public void PawnTableOnGUI(Vector2 position)
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			RecacheIfDirty();
			float num = cachedSize.x - 16f;
			int num2 = 0;
			for (int i = 0; i < def.columns.Count; i++)
			{
				int num3 = ((i != def.columns.Count - 1) ? ((int)cachedColumnWidths[i]) : ((int)(num - (float)num2)));
				Rect rect = new Rect((int)position.x + num2, (int)position.y, num3, (int)cachedHeaderHeight);
				def.columns[i].Worker.DoHeader(rect, this);
				num2 += num3;
			}
			Rect outRect = new Rect((int)position.x, (int)position.y + (int)cachedHeaderHeight, (int)cachedSize.x, (int)cachedSize.y - (int)cachedHeaderHeight);
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (int)cachedHeightNoScrollbar - (int)cachedHeaderHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			int num4 = 0;
			for (int j = 0; j < cachedPawns.Count; j++)
			{
				num2 = 0;
				if (!((float)num4 - scrollPosition.y + (float)(int)cachedRowHeights[j] < 0f) && !((float)num4 - scrollPosition.y > outRect.height))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num4, viewRect.width);
					GUI.color = Color.white;
					if (!CanAssignPawn(cachedPawns[j]))
					{
						GUI.color = Color.gray;
					}
					Rect rect2 = new Rect(0f, num4, viewRect.width, (int)cachedRowHeights[j]);
					if (Mouse.IsOver(rect2))
					{
						GUI.DrawTexture(rect2, TexUI.HighlightTex);
						cachedLookTargets[j].Highlight(arrow: true, cachedPawns[j].IsColonist);
					}
					for (int k = 0; k < def.columns.Count; k++)
					{
						int num5 = ((k != def.columns.Count - 1) ? ((int)cachedColumnWidths[k]) : ((int)(num - (float)num2)));
						Rect rect3 = new Rect(num2, num4, num5, (int)cachedRowHeights[j]);
						def.columns[k].Worker.DoCell(rect3, cachedPawns[j], this);
						num2 += num5;
					}
					if (cachedPawns[j].Downed)
					{
						GUI.color = new Color(1f, 0f, 0f, 0.5f);
						Widgets.DrawLineHorizontal(0f, rect2.center.y, viewRect.width);
					}
					GUI.color = Color.white;
				}
				num4 += (int)cachedRowHeights[j];
			}
			Widgets.EndScrollView();
		}

		public void SetDirty()
		{
			dirty = true;
		}

		public void SetMinMaxSize(int minTableWidth, int maxTableWidth, int minTableHeight, int maxTableHeight)
		{
			this.minTableWidth = minTableWidth;
			this.maxTableWidth = maxTableWidth;
			this.minTableHeight = minTableHeight;
			this.maxTableHeight = maxTableHeight;
			hasFixedSize = false;
			SetDirty();
		}

		public void SetFixedSize(Vector2 size)
		{
			fixedSize = size;
			hasFixedSize = true;
			SetDirty();
		}

		public void SortBy(PawnColumnDef column, bool descending)
		{
			sortByColumn = column;
			sortDescending = descending;
			SetDirty();
		}

		protected virtual bool CanAssignPawn(Pawn p)
		{
			return true;
		}

		private void RecacheIfDirty()
		{
			if (dirty)
			{
				dirty = false;
				RecachePawns();
				RecacheRowHeights();
				cachedHeaderHeight = CalculateHeaderHeight();
				cachedHeightNoScrollbar = CalculateTotalRequiredHeight();
				RecacheSize();
				RecacheColumnWidths();
				RecacheLookTargets();
			}
		}

		private void RecachePawns()
		{
			cachedPawns.Clear();
			cachedPawns.AddRange(pawnsGetter());
			cachedPawns = LabelSortFunction(cachedPawns).ToList();
			if (sortByColumn != null)
			{
				if (sortDescending)
				{
					cachedPawns.SortStable(sortByColumn.Worker.Compare);
				}
				else
				{
					cachedPawns.SortStable((Pawn a, Pawn b) => sortByColumn.Worker.Compare(b, a));
				}
			}
			cachedPawns = PrimarySortFunction(cachedPawns).ToList();
		}

		protected virtual IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
		{
			return input.OrderBy((Pawn p) => p.Label);
		}

		protected virtual IEnumerable<Pawn> PrimarySortFunction(IEnumerable<Pawn> input)
		{
			return input;
		}

		private void RecacheColumnWidths()
		{
			float num = cachedSize.x - 16f;
			float minWidthsSum = 0f;
			RecacheColumnWidths_StartWithMinWidths(out minWidthsSum);
			if (minWidthsSum == num)
			{
				return;
			}
			if (minWidthsSum > num)
			{
				SubtractProportionally(minWidthsSum - num, minWidthsSum);
				return;
			}
			RecacheColumnWidths_DistributeUntilOptimal(num, ref minWidthsSum, out var noMoreFreeSpace);
			if (!noMoreFreeSpace)
			{
				RecacheColumnWidths_DistributeAboveOptimal(num, ref minWidthsSum);
			}
		}

		private void RecacheColumnWidths_StartWithMinWidths(out float minWidthsSum)
		{
			minWidthsSum = 0f;
			cachedColumnWidths.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				float minWidth = GetMinWidth(def.columns[i]);
				cachedColumnWidths.Add(minWidth);
				minWidthsSum += minWidth;
			}
		}

		private void RecacheColumnWidths_DistributeUntilOptimal(float totalAvailableSpaceForColumns, ref float usedWidth, out bool noMoreFreeSpace)
		{
			columnAtOptimalWidth.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				columnAtOptimalWidth.Add(cachedColumnWidths[i] >= GetOptimalWidth(def.columns[i]));
			}
			int num = 0;
			bool flag;
			bool flag2;
			do
			{
				num++;
				if (num >= 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				float num2 = float.MinValue;
				for (int j = 0; j < def.columns.Count; j++)
				{
					if (!columnAtOptimalWidth[j])
					{
						num2 = Mathf.Max(num2, def.columns[j].widthPriority);
					}
				}
				float num3 = 0f;
				for (int k = 0; k < cachedColumnWidths.Count; k++)
				{
					if (!columnAtOptimalWidth[k] && (float)def.columns[k].widthPriority == num2)
					{
						num3 += GetOptimalWidth(def.columns[k]);
					}
				}
				float num4 = totalAvailableSpaceForColumns - usedWidth;
				flag = false;
				flag2 = false;
				for (int l = 0; l < cachedColumnWidths.Count; l++)
				{
					if (columnAtOptimalWidth[l])
					{
						continue;
					}
					if ((float)def.columns[l].widthPriority != num2)
					{
						flag = true;
						continue;
					}
					float num5 = num4 * GetOptimalWidth(def.columns[l]) / num3;
					float num6 = GetOptimalWidth(def.columns[l]) - cachedColumnWidths[l];
					if (num5 >= num6)
					{
						num5 = num6;
						columnAtOptimalWidth[l] = true;
						flag2 = true;
					}
					else
					{
						flag = true;
					}
					if (num5 > 0f)
					{
						cachedColumnWidths[l] += num5;
						usedWidth += num5;
					}
				}
				if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
				{
					noMoreFreeSpace = true;
					break;
				}
			}
			while (flag && flag2);
			noMoreFreeSpace = false;
		}

		private void RecacheColumnWidths_DistributeAboveOptimal(float totalAvailableSpaceForColumns, ref float usedWidth)
		{
			columnAtMaxWidth.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				columnAtMaxWidth.Add(cachedColumnWidths[i] >= GetMaxWidth(def.columns[i]));
			}
			int num = 0;
			while (true)
			{
				num++;
				if (num >= 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				float num2 = 0f;
				for (int j = 0; j < def.columns.Count; j++)
				{
					if (!columnAtMaxWidth[j])
					{
						num2 += Mathf.Max(GetOptimalWidth(def.columns[j]), 1f);
					}
				}
				float num3 = totalAvailableSpaceForColumns - usedWidth;
				bool flag = false;
				for (int k = 0; k < def.columns.Count; k++)
				{
					if (!columnAtMaxWidth[k])
					{
						float num4 = num3 * Mathf.Max(GetOptimalWidth(def.columns[k]), 1f) / num2;
						float num5 = GetMaxWidth(def.columns[k]) - cachedColumnWidths[k];
						if (num4 >= num5)
						{
							num4 = num5;
							columnAtMaxWidth[k] = true;
						}
						else
						{
							flag = true;
						}
						if (num4 > 0f)
						{
							cachedColumnWidths[k] += num4;
							usedWidth += num4;
						}
					}
				}
				if (!(usedWidth >= totalAvailableSpaceForColumns - 0.1f))
				{
					if (!flag)
					{
						DistributeRemainingWidthProportionallyAboveMax(totalAvailableSpaceForColumns - usedWidth);
						break;
					}
					continue;
				}
				break;
			}
		}

		private void RecacheRowHeights()
		{
			cachedRowHeights.Clear();
			for (int i = 0; i < cachedPawns.Count; i++)
			{
				cachedRowHeights.Add(CalculateRowHeight(cachedPawns[i]));
			}
		}

		private void RecacheSize()
		{
			if (hasFixedSize)
			{
				cachedSize = fixedSize;
				return;
			}
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				if (!def.columns[i].ignoreWhenCalculatingOptimalTableSize)
				{
					num += GetOptimalWidth(def.columns[i]);
				}
			}
			float a = Mathf.Clamp(num + 16f, minTableWidth, maxTableWidth);
			float a2 = Mathf.Clamp(cachedHeightNoScrollbar, minTableHeight, maxTableHeight);
			a = Mathf.Min(a, UI.screenWidth);
			a2 = Mathf.Min(a2, UI.screenHeight);
			cachedSize = new Vector2(a, a2);
		}

		private void RecacheLookTargets()
		{
			cachedLookTargets.Clear();
			cachedLookTargets.AddRange(cachedPawns.Select((Pawn p) => new LookTargets(p)));
		}

		private void SubtractProportionally(float toSubtract, float totalUsedWidth)
		{
			for (int i = 0; i < cachedColumnWidths.Count; i++)
			{
				cachedColumnWidths[i] -= toSubtract * cachedColumnWidths[i] / totalUsedWidth;
			}
		}

		private void DistributeRemainingWidthProportionallyAboveMax(float toDistribute)
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num += Mathf.Max(GetOptimalWidth(def.columns[i]), 1f);
			}
			for (int j = 0; j < def.columns.Count; j++)
			{
				cachedColumnWidths[j] += toDistribute * Mathf.Max(GetOptimalWidth(def.columns[j]), 1f) / num;
			}
		}

		private float GetOptimalWidth(PawnColumnDef column)
		{
			return Mathf.Max(column.Worker.GetOptimalWidth(this), 0f);
		}

		private float GetMinWidth(PawnColumnDef column)
		{
			return Mathf.Max(column.Worker.GetMinWidth(this), 0f);
		}

		private float GetMaxWidth(PawnColumnDef column)
		{
			return Mathf.Max(column.Worker.GetMaxWidth(this), 0f);
		}

		private float CalculateRowHeight(Pawn pawn)
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num = Mathf.Max(num, def.columns[i].Worker.GetMinCellHeight(pawn));
			}
			return num;
		}

		private float CalculateHeaderHeight()
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num = Mathf.Max(num, def.columns[i].Worker.GetMinHeaderHeight(this));
			}
			return num;
		}

		private float CalculateTotalRequiredHeight()
		{
			float num = CalculateHeaderHeight();
			for (int i = 0; i < cachedPawns.Count; i++)
			{
				num += CalculateRowHeight(cachedPawns[i]);
			}
			return num;
		}
	}
}
