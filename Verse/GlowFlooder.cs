using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GlowFlooder
	{
		private struct GlowFloodCell
		{
			public int intDist;

			public uint status;
		}

		private class CompareGlowFlooderLightSquares : IComparer<int>
		{
			private GlowFloodCell[] grid;

			public CompareGlowFlooderLightSquares(GlowFloodCell[] grid)
			{
				this.grid = grid;
			}

			public int Compare(int a, int b)
			{
				return grid[a].intDist.CompareTo(grid[b].intDist);
			}
		}

		private Map map;

		private GlowFloodCell[] calcGrid;

		private FastPriorityQueue<int> openSet;

		private uint statusUnseenValue;

		private uint statusOpenValue = 1u;

		private uint statusFinalizedValue = 2u;

		private int mapSizeX;

		private int mapSizeZ;

		private CompGlower glower;

		private CellIndices cellIndices;

		private Color32[] glowGrid;

		private float attenLinearSlope;

		private Thing[] blockers = new Thing[8];

		private static readonly sbyte[,] Directions = new sbyte[8, 2]
		{
			{
				0,
				-1
			},
			{
				1,
				0
			},
			{
				0,
				1
			},
			{
				-1,
				0
			},
			{
				1,
				-1
			},
			{
				1,
				1
			},
			{
				-1,
				1
			},
			{
				-1,
				-1
			}
		};

		public GlowFlooder(Map map)
		{
			this.map = map;
			mapSizeX = map.Size.x;
			mapSizeZ = map.Size.z;
			calcGrid = new GlowFloodCell[mapSizeX * mapSizeZ];
			openSet = new FastPriorityQueue<int>(new CompareGlowFlooderLightSquares(calcGrid));
		}

		public void AddFloodGlowFor(CompGlower theGlower, Color32[] glowGrid)
		{
			cellIndices = map.cellIndices;
			this.glowGrid = glowGrid;
			glower = theGlower;
			attenLinearSlope = -1f / theGlower.Props.glowRadius;
			Building[] innerArray = map.edificeGrid.InnerArray;
			IntVec3 position = theGlower.parent.Position;
			int num = Mathf.RoundToInt(glower.Props.glowRadius * 100f);
			int curIndex = cellIndices.CellToIndex(position);
			int num2 = 0;
			InitStatusesAndPushStartNode(ref curIndex, position);
			while (openSet.Count != 0)
			{
				curIndex = openSet.Pop();
				IntVec3 intVec = cellIndices.IndexToCell(curIndex);
				calcGrid[curIndex].status = statusFinalizedValue;
				SetGlowGridFromDist(curIndex);
				for (int i = 0; i < 8; i++)
				{
					uint num3 = (uint)(intVec.x + Directions[i, 0]);
					uint num4 = (uint)(intVec.z + Directions[i, 1]);
					if (num3 >= mapSizeX || num4 >= mapSizeZ)
					{
						continue;
					}
					int x = (int)num3;
					int z = (int)num4;
					int num5 = cellIndices.CellToIndex(x, z);
					if (calcGrid[num5].status == statusFinalizedValue)
					{
						continue;
					}
					blockers[i] = innerArray[num5];
					if (blockers[i] != null)
					{
						if (blockers[i].def.blockLight)
						{
							continue;
						}
						blockers[i] = null;
					}
					int num6 = (i >= 4) ? 141 : 100;
					int num7 = calcGrid[curIndex].intDist + num6;
					if (num7 > num)
					{
						continue;
					}
					switch (i)
					{
					case 4:
						if (blockers[0] != null && blockers[1] != null)
						{
							continue;
						}
						break;
					case 5:
						if (blockers[1] != null && blockers[2] != null)
						{
							continue;
						}
						break;
					case 6:
						if (blockers[2] != null && blockers[3] != null)
						{
							continue;
						}
						break;
					case 7:
						if (blockers[0] != null && blockers[3] != null)
						{
							continue;
						}
						break;
					}
					if (calcGrid[num5].status <= statusUnseenValue)
					{
						calcGrid[num5].intDist = 999999;
						calcGrid[num5].status = statusOpenValue;
					}
					if (num7 < calcGrid[num5].intDist)
					{
						calcGrid[num5].intDist = num7;
						calcGrid[num5].status = statusOpenValue;
						openSet.Push(num5);
					}
				}
			}
		}

		private void InitStatusesAndPushStartNode(ref int curIndex, IntVec3 start)
		{
			statusUnseenValue += 3u;
			statusOpenValue += 3u;
			statusFinalizedValue += 3u;
			curIndex = cellIndices.CellToIndex(start);
			openSet.Clear();
			calcGrid[curIndex].intDist = 100;
			openSet.Clear();
			openSet.Push(curIndex);
		}

		private void SetGlowGridFromDist(int index)
		{
			float num = (float)calcGrid[index].intDist / 100f;
			ColorInt colorInt = default(ColorInt);
			if (num <= glower.Props.glowRadius)
			{
				float b = 1f / (num * num);
				float b2 = Mathf.Lerp(1f + attenLinearSlope * num, b, 0.4f);
				colorInt = glower.Props.glowColor * b2;
			}
			if (colorInt.r > 0 || colorInt.g > 0 || colorInt.b > 0)
			{
				colorInt.ClampToNonNegative();
				ColorInt colorInt2 = glowGrid[index].AsColorInt();
				colorInt2 += colorInt;
				if (num < glower.Props.overlightRadius)
				{
					colorInt2.a = 1;
				}
				Color32 toColor = colorInt2.ToColor32;
				glowGrid[index] = toColor;
			}
		}
	}
}
