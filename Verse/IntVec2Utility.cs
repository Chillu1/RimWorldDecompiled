using UnityEngine;

namespace Verse;

public static class IntVec2Utility
{
	public static IntVec2 ToIntVec2(this Vector3 v)
	{
		return new IntVec2((int)v.x, (int)v.z);
	}

	public static IntVec2 ToIntVec2(this Vector2 v)
	{
		return new IntVec2((int)v.x, (int)v.y);
	}
}
