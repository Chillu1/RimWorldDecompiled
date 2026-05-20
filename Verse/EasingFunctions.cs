using Unity.Mathematics;
using UnityEngine;

namespace Verse;

public static class EasingFunctions
{
	public static float EaseInOutQuad(float v)
	{
		if (!((double)v < 0.5))
		{
			return 1f - Mathf.Pow(-2f * v + 2f, 4f) / 2f;
		}
		return 8f * v * v * v * v;
	}

	public static float EaseInOutQuint(float x)
	{
		if (!((double)x < 0.5))
		{
			return 1f - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
		}
		return 16f * x * x * x * x * x;
	}

	public static float EaseInCubic(float x)
	{
		return x * x * x;
	}

	public static float EaseOutCubic(float x)
	{
		return 1f - math.pow(1f - x, 3f);
	}
}
