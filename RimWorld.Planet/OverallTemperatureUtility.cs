using System;
using Verse;

namespace RimWorld.Planet;

public static class OverallTemperatureUtility
{
	private static int cachedEnumValuesCount = -1;

	private static readonly SimpleCurve Curve_VeryCold = new SimpleCurve
	{
		new CurvePoint(-9999f, -9999f),
		new CurvePoint(-50f, -75f),
		new CurvePoint(-40f, -60f),
		new CurvePoint(0f, -35f),
		new CurvePoint(20f, -28f),
		new CurvePoint(25f, -18f),
		new CurvePoint(30f, -8.5f),
		new CurvePoint(50f, -7f)
	};

	private static readonly SimpleCurve Curve_Cold = new SimpleCurve
	{
		new CurvePoint(-9999f, -9999f),
		new CurvePoint(-50f, -70f),
		new CurvePoint(-25f, -40f),
		new CurvePoint(-20f, -25f),
		new CurvePoint(-13f, -15f),
		new CurvePoint(0f, -12f),
		new CurvePoint(30f, -3f),
		new CurvePoint(60f, 25f)
	};

	private static readonly SimpleCurve Curve_LittleBitColder = new SimpleCurve
	{
		new CurvePoint(-9999f, -9999f),
		new CurvePoint(-20f, -22f),
		new CurvePoint(-15f, -15f),
		new CurvePoint(-5f, -13f),
		new CurvePoint(40f, 30f),
		new CurvePoint(9999f, 9999f)
	};

	private static readonly SimpleCurve Curve_LittleBitWarmer = new SimpleCurve
	{
		new CurvePoint(-9999f, -9999f),
		new CurvePoint(-45f, -35f),
		new CurvePoint(40f, 50f),
		new CurvePoint(120f, 120f),
		new CurvePoint(9999f, 9999f)
	};

	private static readonly SimpleCurve Curve_Hot = new SimpleCurve
	{
		new CurvePoint(-45f, -22f),
		new CurvePoint(-25f, -12f),
		new CurvePoint(-22f, 2f),
		new CurvePoint(-10f, 25f),
		new CurvePoint(40f, 57f),
		new CurvePoint(120f, 120f),
		new CurvePoint(9999f, 9999f)
	};

	private static readonly SimpleCurve Curve_VeryHot = new SimpleCurve
	{
		new CurvePoint(-45f, 25f),
		new CurvePoint(0f, 40f),
		new CurvePoint(33f, 80f),
		new CurvePoint(40f, 88f),
		new CurvePoint(120f, 120f),
		new CurvePoint(9999f, 9999f)
	};

	public static int EnumValuesCount
	{
		get
		{
			if (cachedEnumValuesCount < 0)
			{
				cachedEnumValuesCount = Enum.GetNames(typeof(OverallTemperature)).Length;
			}
			return cachedEnumValuesCount;
		}
	}

	public static SimpleCurve GetTemperatureCurve(this OverallTemperature overallTemperature)
	{
		return overallTemperature switch
		{
			OverallTemperature.VeryCold => Curve_VeryCold, 
			OverallTemperature.Cold => Curve_Cold, 
			OverallTemperature.LittleBitColder => Curve_LittleBitColder, 
			OverallTemperature.LittleBitWarmer => Curve_LittleBitWarmer, 
			OverallTemperature.Hot => Curve_Hot, 
			OverallTemperature.VeryHot => Curve_VeryHot, 
			_ => null, 
		};
	}
}
