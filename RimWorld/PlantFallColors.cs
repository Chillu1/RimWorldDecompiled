using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PlantFallColors
{
	[TweakValue("Graphics", 0f, 1f)]
	private static float FallColorBegin = 0.55f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallColorEnd = 0.45f;

	[TweakValue("Graphics", 0f, 30f)]
	private static float FallSlopeComponent = 15f;

	[TweakValue("Graphics", 0f, 100f)]
	private static bool FallIntensityOverride = false;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallIntensity = 0f;

	[TweakValue("Graphics", 0f, 100f)]
	private static bool FallGlobalControls = false;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallSrcR = 0.3803f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallSrcG = 0.4352f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallSrcB = 0.1451f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallDstR = 0.4392f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallDstG = 0.3254f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallDstB = 0.1765f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallRangeBegin = 0.02f;

	[TweakValue("Graphics", 0f, 1f)]
	private static float FallRangeEnd = 0.1f;

	private static readonly int FallSrc = Shader.PropertyToID("_FallSrc");

	private static readonly int FallDst = Shader.PropertyToID("_FallDst");

	private static readonly int FallRange = Shader.PropertyToID("_FallRange");

	private static readonly int Controls = Shader.PropertyToID("_FallGlobalControls");

	public static float GetFallColorFactor(float latitude, int dayOfYear)
	{
		float a = GenCelestial.AverageGlow(latitude, dayOfYear);
		float b = GenCelestial.AverageGlow(latitude, dayOfYear + 1);
		float x = Mathf.LerpUnclamped(a, b, FallSlopeComponent);
		return GenMath.LerpDoubleClamped(FallColorBegin, FallColorEnd, 0f, 1f, x);
	}

	public static void SetFallShaderGlobals(Map map)
	{
		if (FallIntensityOverride)
		{
			Shader.SetGlobalFloat(ShaderPropertyIDs.FallIntensity, FallIntensity);
		}
		else
		{
			Vector2 vector = Find.WorldGrid.LongLatOf(map.Tile);
			Shader.SetGlobalFloat(ShaderPropertyIDs.FallIntensity, GetFallColorFactor(vector.y, GenLocalDate.DayOfYear(map)));
		}
		Shader.SetGlobalInt(Controls, FallGlobalControls ? 1 : 0);
		if (FallGlobalControls)
		{
			Shader.SetGlobalVector(FallSrc, new Vector3(FallSrcR, FallSrcG, FallSrcB));
			Shader.SetGlobalVector(FallDst, new Vector3(FallDstR, FallDstG, FallDstB));
			Shader.SetGlobalVector(FallRange, new Vector3(FallRangeBegin, FallRangeEnd));
		}
	}
}
