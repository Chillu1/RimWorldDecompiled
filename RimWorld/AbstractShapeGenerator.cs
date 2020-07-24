using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class AbstractShapeGenerator
	{
		private const int MaxIterations = 500;

		private const float MinTruesPct = 0.6f;

		private const float MaxTruesPct = 0.85f;

		private const float MinTruesPct_PreferOutlines = 0.24f;

		private const float MaxTruesPct_PreferOutlines = 0.53f;

		private static HashSet<IntVec3> tmpCircleCells = new HashSet<IntVec3>();

		private static Stack<Pair<int, int>> tmpStack = new Stack<Pair<int, int>>();

		public static bool[,] Generate(int width, int height, bool horizontalSymmetry, bool verticalSymmetry, bool allTruesMustBeConnected = false, bool allowEnclosedFalses = true, bool preferOutlines = false, float wipedCircleRadiusPct = 0f)
		{
			bool[,] array = new bool[width, height];
			int num = 0;
			while (true)
			{
				GenerateInt(array, horizontalSymmetry, verticalSymmetry, allowEnclosedFalses, preferOutlines, wipedCircleRadiusPct);
				if (IsValid(array, allTruesMustBeConnected, allowEnclosedFalses, preferOutlines, wipedCircleRadiusPct))
				{
					break;
				}
				num++;
				if (num > 500)
				{
					Log.Error("AbstractShapeGenerator could not generate a valid shape after " + 500 + " tries. width=" + width + " height=" + height + " preferOutlines=" + preferOutlines.ToString());
					break;
				}
			}
			return array;
		}

		private static void GenerateInt(bool[,] grid, bool horizontalSymmetry, bool verticalSymmetry, bool allowEnclosedFalses, bool preferOutlines, float wipedCircleRadiusPct)
		{
			int length = grid.GetLength(0);
			int length2 = grid.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					grid[i, j] = false;
				}
			}
			int num;
			int num2;
			int num3;
			int num4;
			int num5;
			int num6;
			if (preferOutlines)
			{
				num = 0;
				num2 = 2;
				num3 = 0;
				num4 = 4;
				num5 = 2;
				num6 = ((length < 16 && length2 < 16) ? ((length < 13 && length2 < 13) ? ((length < 12 && length2 < 12) ? 1 : 2) : 3) : 4);
			}
			else
			{
				num = Rand.RangeInclusive(1, 3);
				num2 = 0;
				num3 = Rand.RangeInclusive(1, 3);
				num6 = 0;
				num4 = 0;
				num5 = 0;
			}
			float num7 = 0.3f;
			float num8 = 0.3f;
			float num9 = 0.7f;
			float num10 = 0.7f;
			for (int k = 0; k < num; k++)
			{
				foreach (IntVec3 item in CellRect.CenteredOn(new IntVec3(Rand.RangeInclusive(0, length - 1), 0, Rand.RangeInclusive(0, length2 - 1)), Mathf.Max(Mathf.RoundToInt((float)Rand.RangeInclusive(1, length) * num7), 1), Mathf.Max(Mathf.RoundToInt((float)Rand.RangeInclusive(1, length2) * num7), 1)))
				{
					if (item.x >= 0 && item.x < length && item.z >= 0 && item.z < length2)
					{
						grid[item.x, item.z] = true;
					}
				}
			}
			for (int l = 0; l < num2; l++)
			{
				CellRect cellRect = CellRect.CenteredOn(new IntVec3(Rand.RangeInclusive(0, length - 1), 0, Rand.RangeInclusive(0, length2 - 1)), Mathf.Max(Mathf.RoundToInt((float)Rand.RangeInclusive(1, length) * num8), 1), Mathf.Max(Mathf.RoundToInt((float)Rand.RangeInclusive(1, length2) * num8), 1));
				Rot4 random = Rot4.Random;
				foreach (IntVec3 edgeCell in cellRect.EdgeCells)
				{
					if ((allowEnclosedFalses || !cellRect.IsOnEdge(edgeCell, random) || cellRect.IsOnEdge(edgeCell, random.Rotated(RotationDirection.Clockwise)) || cellRect.IsOnEdge(edgeCell, random.Rotated(RotationDirection.Counterclockwise))) && edgeCell.x >= 0 && edgeCell.x < length && edgeCell.z >= 0 && edgeCell.z < length2)
					{
						grid[edgeCell.x, edgeCell.z] = true;
					}
				}
			}
			for (int m = 0; m < num3; m++)
			{
				IntVec2 intVec = new IntVec2(Rand.RangeInclusive(0, length - 1), Rand.RangeInclusive(0, length2 - 1));
				foreach (IntVec3 item2 in GenRadial.RadialPatternInRadius(Mathf.Max(Mathf.RoundToInt((float)(Mathf.Max(length, length2) / 2) * num9), 1)))
				{
					IntVec3 current3 = item2;
					current3.x += intVec.x;
					current3.z += intVec.z;
					if (current3.x >= 0 && current3.x < length && current3.z >= 0 && current3.z < length2)
					{
						grid[current3.x, current3.z] = true;
					}
				}
			}
			for (int n = 0; n < num6; n++)
			{
				float num11 = Rand.Range(0.7f, 1f);
				IntVec2 intVec2 = new IntVec2(Rand.RangeInclusive(0, length - 1), Rand.RangeInclusive(0, length2 - 1));
				int num12 = Mathf.Max(Mathf.RoundToInt((float)(Mathf.Max(length, length2) / 2) * num10 * num11), 1);
				bool @bool = Rand.Bool;
				tmpCircleCells.Clear();
				tmpCircleCells.AddRange(GenRadial.RadialPatternInRadius(num12));
				foreach (IntVec3 tmpCircleCell in tmpCircleCells)
				{
					if ((allowEnclosedFalses || ((!@bool || tmpCircleCell.x >= 0) && (@bool || tmpCircleCell.z >= 0))) && (!tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x - 1, 0, tmpCircleCell.z - 1)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x - 1, 0, tmpCircleCell.z)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x - 1, 0, tmpCircleCell.z + 1)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x, 0, tmpCircleCell.z - 1)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x, 0, tmpCircleCell.z)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x, 0, tmpCircleCell.z + 1)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x + 1, 0, tmpCircleCell.z - 1)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x + 1, 0, tmpCircleCell.z)) || !tmpCircleCells.Contains(new IntVec3(tmpCircleCell.x + 1, 0, tmpCircleCell.z + 1))))
					{
						IntVec3 intVec3 = tmpCircleCell;
						intVec3.x += intVec2.x;
						intVec3.z += intVec2.z;
						if (intVec3.x >= 0 && intVec3.x < length && intVec3.z >= 0 && intVec3.z < length2)
						{
							grid[intVec3.x, intVec3.z] = true;
						}
					}
				}
			}
			for (int num13 = 0; num13 < num4; num13++)
			{
				bool bool2 = Rand.Bool;
				foreach (IntVec3 item3 in CellRect.CenteredOn(new IntVec3(Rand.RangeInclusive(0, length - 1), 0, Rand.RangeInclusive(0, length2 - 1)), (!bool2) ? 1 : Mathf.RoundToInt(Rand.RangeInclusive(1, length)), bool2 ? 1 : Mathf.RoundToInt(Rand.RangeInclusive(1, length2))))
				{
					if (item3.x >= 0 && item3.x < length && item3.z >= 0 && item3.z < length2)
					{
						grid[item3.x, item3.z] = true;
					}
				}
			}
			for (int num14 = 0; num14 < num5; num14++)
			{
				bool bool3 = Rand.Bool;
				CellRect cellRect2 = CellRect.CenteredOn(new IntVec3(Rand.RangeInclusive(0, length - 1), 0, Rand.RangeInclusive(0, length2 - 1)), Mathf.RoundToInt(Rand.RangeInclusive(1, length)), 1);
				foreach (IntVec3 item4 in cellRect2)
				{
					int num15 = item4.x - cellRect2.minX - cellRect2.Width / 2;
					if (bool3)
					{
						num15 = -num15;
					}
					IntVec3 intVec4 = item4;
					intVec4.z += num15;
					if (intVec4.x >= 0 && intVec4.x < length && intVec4.z >= 0 && intVec4.z < length2)
					{
						grid[intVec4.x, intVec4.z] = true;
					}
				}
			}
			if (preferOutlines)
			{
				for (int num16 = 0; num16 < grid.GetLength(0) - 1; num16++)
				{
					for (int num17 = 0; num17 < grid.GetLength(1) - 1; num17++)
					{
						if (grid[num16, num17] && grid[num16 + 1, num17] && grid[num16, num17 + 1] && grid[num16 + 1, num17 + 1])
						{
							switch (Rand.Range(0, 4))
							{
							case 0:
								grid[num16, num17] = false;
								break;
							case 1:
								grid[num16 + 1, num17] = false;
								break;
							case 2:
								grid[num16, num17 + 1] = false;
								break;
							default:
								grid[num16 + 1, num17 + 1] = false;
								break;
							}
						}
					}
				}
			}
			if (wipedCircleRadiusPct > 0f)
			{
				IntVec2 intVec5 = new IntVec2(length / 2, length2 / 2);
				foreach (IntVec3 item5 in GenRadial.RadialPatternInRadius(Mathf.FloorToInt((float)Mathf.Min(length, length2) * wipedCircleRadiusPct)))
				{
					IntVec3 current7 = item5;
					current7.x += intVec5.x;
					current7.z += intVec5.z;
					if (current7.x >= 0 && current7.x < length && current7.z >= 0 && current7.z < length2)
					{
						grid[current7.x, current7.z] = false;
					}
				}
			}
			if (horizontalSymmetry)
			{
				for (int num18 = grid.GetLength(0) / 2; num18 < grid.GetLength(0); num18++)
				{
					for (int num19 = 0; num19 < grid.GetLength(1); num19++)
					{
						grid[num18, num19] = grid[grid.GetLength(0) - num18 - 1, num19];
					}
				}
			}
			if (!verticalSymmetry)
			{
				return;
			}
			for (int num20 = 0; num20 < grid.GetLength(0); num20++)
			{
				for (int num21 = grid.GetLength(1) / 2; num21 < grid.GetLength(1); num21++)
				{
					grid[num20, num21] = grid[num20, grid.GetLength(1) - num21 - 1];
				}
			}
		}

		private static bool IsValid(bool[,] grid, bool allTruesMustBeConnected, bool allowEnclosedFalses, bool preferOutlines, float wipedCircleRadiusPct)
		{
			int num = 0;
			int upperBound = grid.GetUpperBound(0);
			int upperBound2 = grid.GetUpperBound(1);
			for (int i = grid.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = grid.GetLowerBound(1); j <= upperBound2; j++)
				{
					if (grid[i, j])
					{
						num++;
					}
				}
			}
			if (grid.GetLength(0) >= 3 && grid.GetLength(1) >= 3)
			{
				float num2 = 1f;
				if (wipedCircleRadiusPct > 0f)
				{
					int num3 = Mathf.FloorToInt((float)Mathf.Min(grid.GetLength(0), grid.GetLength(1)) * wipedCircleRadiusPct);
					float num4 = (float)Math.PI * (float)num3 * (float)num3;
					num2 = 1f - Mathf.Clamp01(num4 / (float)(grid.GetLength(0) * grid.GetLength(1)));
				}
				int num5 = grid.GetLength(0) * grid.GetLength(1);
				int num6 = Mathf.FloorToInt((float)num5 * (preferOutlines ? 0.24f : 0.6f) * num2);
				int num7 = Mathf.CeilToInt((float)num5 * (preferOutlines ? 0.53f : 0.85f) * num2);
				if (num < num6 || num > num7)
				{
					return false;
				}
			}
			if (grid.GetLength(0) >= 2 && grid.GetLength(1) >= 2)
			{
				bool flag = false;
				bool flag2 = false;
				for (int k = 0; k < grid.GetLength(0) - 1; k++)
				{
					for (int l = 0; l < grid.GetLength(1) - 1; l++)
					{
						if (grid[k, l] && grid[k + 1, l])
						{
							flag2 = true;
						}
						if (grid[k, l] && grid[k, l + 1])
						{
							flag = true;
						}
						if (flag2 && flag)
						{
							break;
						}
					}
				}
				if (!flag2 || !flag)
				{
					return false;
				}
			}
			if (allTruesMustBeConnected)
			{
				bool[,] array = new bool[grid.GetLength(0), grid.GetLength(1)];
				bool flag3 = false;
				for (int m = 0; m < grid.GetLength(0); m++)
				{
					for (int n = 0; n < grid.GetLength(1); n++)
					{
						if (grid[m, n] && !array[m, n])
						{
							if (flag3)
							{
								return false;
							}
							flag3 = true;
							MarkVisited(m, n, grid, array, traverseFalses: false);
						}
					}
				}
			}
			if (!allowEnclosedFalses)
			{
				bool[,] array2 = new bool[grid.GetLength(0), grid.GetLength(1)];
				for (int num8 = 0; num8 < grid.GetLength(0); num8++)
				{
					if (!grid[num8, 0])
					{
						MarkVisited(num8, 0, grid, array2, traverseFalses: true);
					}
					if (!grid[num8, grid.GetLength(1) - 1])
					{
						MarkVisited(num8, grid.GetLength(1) - 1, grid, array2, traverseFalses: true);
					}
				}
				for (int num9 = 0; num9 < grid.GetLength(1); num9++)
				{
					if (!grid[0, num9])
					{
						MarkVisited(0, num9, grid, array2, traverseFalses: true);
					}
					if (!grid[grid.GetLength(0) - 1, num9])
					{
						MarkVisited(grid.GetLength(0) - 1, num9, grid, array2, traverseFalses: true);
					}
				}
				for (int num10 = 0; num10 < grid.GetLength(0); num10++)
				{
					for (int num11 = 0; num11 < grid.GetLength(1); num11++)
					{
						if (!grid[num10, num11] && !array2[num10, num11])
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private static void MarkVisited(int startX, int startY, bool[,] grid, bool[,] visited, bool traverseFalses)
		{
			if (visited[startX, startY])
			{
				return;
			}
			tmpStack.Clear();
			tmpStack.Push(new Pair<int, int>(startX, startY));
			visited[startX, startY] = true;
			while (tmpStack.Count != 0)
			{
				Pair<int, int> pair = tmpStack.Pop();
				int first = pair.First;
				int second = pair.Second;
				if (first > 0 && grid[first - 1, second] == !traverseFalses && !visited[first - 1, second])
				{
					visited[first - 1, second] = true;
					tmpStack.Push(new Pair<int, int>(first - 1, second));
				}
				if (second > 0 && grid[first, second - 1] == !traverseFalses && !visited[first, second - 1])
				{
					visited[first, second - 1] = true;
					tmpStack.Push(new Pair<int, int>(first, second - 1));
				}
				if (first + 1 < grid.GetLength(0) && grid[first + 1, second] == !traverseFalses && !visited[first + 1, second])
				{
					visited[first + 1, second] = true;
					tmpStack.Push(new Pair<int, int>(first + 1, second));
				}
				if (second + 1 < grid.GetLength(1) && grid[first, second + 1] == !traverseFalses && !visited[first, second + 1])
				{
					visited[first, second + 1] = true;
					tmpStack.Push(new Pair<int, int>(first, second + 1));
				}
			}
		}
	}
}
