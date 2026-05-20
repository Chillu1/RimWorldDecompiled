using System;
using UnityEngine;
using Verse;

namespace LudeonTK;

public static class UIScaling
{
	public static Rect AdjustRectToUIScaling(Rect rect)
	{
		Rect result = rect;
		result.xMin = AdjustCoordToUIScalingFloor(rect.xMin);
		result.yMin = AdjustCoordToUIScalingFloor(rect.yMin);
		result.xMax = AdjustCoordToUIScalingCeil(rect.xMax);
		result.yMax = AdjustCoordToUIScalingCeil(rect.yMax);
		return result;
	}

	public static float AdjustCoordToUIScalingFloor(float coord)
	{
		double num = Prefs.UIScale * coord;
		float num2 = (float)(num - Math.Floor(num)) / Prefs.UIScale;
		return coord - num2;
	}

	public static float AdjustCoordToUIScalingCeil(float coord)
	{
		double num = Prefs.UIScale * coord;
		float num2 = (float)(num - Math.Ceiling(num)) / Prefs.UIScale;
		return coord - num2;
	}
}
