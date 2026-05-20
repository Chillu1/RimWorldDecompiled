using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class DrawStyle_Line : DrawStyle
{
	public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
	{
		if (Mathf.Abs(origin.x - target.x) >= Mathf.Abs(origin.z - target.z))
		{
			int z = origin.z;
			if (origin.x > target.x)
			{
				IntVec3 intVec = target;
				IntVec3 intVec2 = origin;
				origin = intVec;
				target = intVec2;
			}
			for (int i = origin.x; i <= target.x; i++)
			{
				buffer.Add(new IntVec3(i, 0, z));
			}
		}
		else
		{
			int x = origin.x;
			if (origin.z > target.z)
			{
				IntVec3 intVec3 = target;
				IntVec3 intVec2 = origin;
				origin = intVec3;
				target = intVec2;
			}
			for (int j = origin.z; j <= target.z; j++)
			{
				buffer.Add(new IntVec3(x, 0, j));
			}
		}
	}
}
