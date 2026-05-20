using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class DrawStyle_AngledLine : DrawStyle
{
	private static readonly List<IntVec3> tempLine = new List<IntVec3>();

	public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
	{
		tempLine.Clear();
		DrawPaddedBresenham(origin, target, tempLine);
		int count = tempLine.Count;
		buffer.Clear();
		int num = (count - 1) / 2;
		buffer.Add(tempLine[num]);
		for (int i = 1; i <= num; i++)
		{
			if (num - i >= 0)
			{
				buffer.Add(tempLine[num - i]);
			}
			if (num + i < count)
			{
				buffer.Add(tempLine[num + i]);
			}
		}
		if (count % 2 == 0)
		{
			buffer.Add(tempLine[count - 1]);
		}
	}

	private void DrawPaddedBresenham(IntVec3 origin, IntVec3 target, List<IntVec3> line)
	{
		int num = origin.x;
		int num2 = origin.z;
		int num3 = Mathf.Abs(target.x - origin.x);
		int num4 = Mathf.Abs(target.z - origin.z);
		int num5 = ((origin.x < target.x) ? 1 : (-1));
		int num6 = ((origin.z < target.z) ? 1 : (-1));
		bool flag = num3 >= num4;
		num3 *= 4;
		num4 *= 4;
		int num7 = num3 / 2 - num4 / 2;
		int num8 = Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.z - origin.z) + 1;
		for (int i = 0; i < num8; i++)
		{
			line.Add(new IntVec3(num, origin.y, num2));
			if (num7 > 0 || (num7 == 0 && flag))
			{
				num += num5;
				num7 -= num4;
			}
			else
			{
				num2 += num6;
				num7 += num3;
			}
		}
	}
}
