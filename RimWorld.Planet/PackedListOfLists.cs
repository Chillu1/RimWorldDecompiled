using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet
{
	public static class PackedListOfLists
	{
		private static List<int> vertAdjacentTrisCount = new List<int>();

		public static void AddList<T>(List<int> offsets, List<T> values, List<T> listToAdd)
		{
			offsets.Add(values.Count);
			values.AddRange(listToAdd);
		}

		public static void GetList<T>(List<int> offsets, List<T> values, int listIndex, List<T> outList)
		{
			outList.Clear();
			int num = offsets[listIndex];
			int num2 = values.Count;
			if (listIndex + 1 < offsets.Count)
			{
				num2 = offsets[listIndex + 1];
			}
			for (int i = num; i < num2; i++)
			{
				outList.Add(values[i]);
			}
		}

		public static void GetListValuesIndices<T>(List<int> offsets, List<T> values, int listIndex, List<int> outList)
		{
			outList.Clear();
			int num = offsets[listIndex];
			int num2 = values.Count;
			if (listIndex + 1 < offsets.Count)
			{
				num2 = offsets[listIndex + 1];
			}
			for (int i = num; i < num2; i++)
			{
				outList.Add(i);
			}
		}

		public static int GetListCount<T>(List<int> offsets, List<T> values, int listIndex)
		{
			int num = offsets[listIndex];
			int num2 = values.Count;
			if (listIndex + 1 < offsets.Count)
			{
				num2 = offsets[listIndex + 1];
			}
			return num2 - num;
		}

		public static void GenerateVertToTrisPackedList(List<Vector3> verts, List<TriangleIndices> tris, List<int> outOffsets, List<int> outValues)
		{
			outOffsets.Clear();
			outValues.Clear();
			vertAdjacentTrisCount.Clear();
			int i = 0;
			for (int count = verts.Count; i < count; i++)
			{
				vertAdjacentTrisCount.Add(0);
			}
			int j = 0;
			for (int count2 = tris.Count; j < count2; j++)
			{
				TriangleIndices triangleIndices = tris[j];
				vertAdjacentTrisCount[triangleIndices.v1]++;
				vertAdjacentTrisCount[triangleIndices.v2]++;
				vertAdjacentTrisCount[triangleIndices.v3]++;
			}
			int num = 0;
			int k = 0;
			for (int count3 = verts.Count; k < count3; k++)
			{
				outOffsets.Add(num);
				int num2 = vertAdjacentTrisCount[k];
				vertAdjacentTrisCount[k] = 0;
				for (int l = 0; l < num2; l++)
				{
					outValues.Add(-1);
				}
				num += num2;
			}
			int m = 0;
			for (int count4 = tris.Count; m < count4; m++)
			{
				TriangleIndices triangleIndices2 = tris[m];
				outValues[outOffsets[triangleIndices2.v1] + vertAdjacentTrisCount[triangleIndices2.v1]] = m;
				outValues[outOffsets[triangleIndices2.v2] + vertAdjacentTrisCount[triangleIndices2.v2]] = m;
				outValues[outOffsets[triangleIndices2.v3] + vertAdjacentTrisCount[triangleIndices2.v3]] = m;
				vertAdjacentTrisCount[triangleIndices2.v1]++;
				vertAdjacentTrisCount[triangleIndices2.v2]++;
				vertAdjacentTrisCount[triangleIndices2.v3]++;
			}
		}
	}
}
