using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenCelestial
	{
		public struct LightInfo
		{
			public Vector2 vector;

			public float intensity;
		}

		public enum LightType
		{
			Shadow,
			LightingSun,
			LightingMoon
		}

		public const float ShadowMaxLengthDay = 15f;

		public const float ShadowMaxLengthNight = 15f;

		private const float ShadowGlowLerpSpan = 0.15f;

		private const float ShadowDayNightThreshold = 0.6f;

		private static SimpleCurve SunPeekAroundDegreesFactorCurve = new SimpleCurve
		{
			new CurvePoint(70f, 1f),
			new CurvePoint(75f, 0.05f)
		};

		private static SimpleCurve SunOffsetFractionFromLatitudeCurve = new SimpleCurve
		{
			new CurvePoint(70f, 0.2f),
			new CurvePoint(75f, 1.5f)
		};

		private static int TicksAbsForSunPosInWorldSpace
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					return GenTicks.TicksAbs;
				}
				int startingTile = Find.GameInitData.startingTile;
				float longitude = (startingTile >= 0) ? Find.WorldGrid.LongLatOf(startingTile).x : 0f;
				return Mathf.RoundToInt(2500f * (12f - GenDate.TimeZoneFloatAt(longitude)));
			}
		}

		public static float CurCelestialSunGlow(Map map)
		{
			return CelestialSunGlow(map, Find.TickManager.TicksAbs);
		}

		public static float CelestialSunGlow(Map map, int ticksAbs)
		{
			Vector2 vector = Find.WorldGrid.LongLatOf(map.Tile);
			return CelestialSunGlowPercent(vector.y, GenDate.DayOfYear(ticksAbs, vector.x), GenDate.DayPercent(ticksAbs, vector.x));
		}

		public static float CurShadowStrength(Map map)
		{
			return Mathf.Clamp01(Mathf.Abs(CurCelestialSunGlow(map) - 0.6f) / 0.15f);
		}

		public static LightInfo GetLightSourceInfo(Map map, LightType type)
		{
			float num = GenLocalDate.DayPercent(map);
			bool flag;
			float intensity;
			switch (type)
			{
			case LightType.Shadow:
				flag = IsDaytime(CurCelestialSunGlow(map));
				intensity = CurShadowStrength(map);
				break;
			case LightType.LightingSun:
				flag = true;
				intensity = Mathf.Clamp01((CurCelestialSunGlow(map) - 0.6f + 0.2f) / 0.15f);
				break;
			case LightType.LightingMoon:
				flag = false;
				intensity = Mathf.Clamp01((0f - (CurCelestialSunGlow(map) - 0.6f - 0.2f)) / 0.15f);
				break;
			default:
				Log.ErrorOnce("Invalid light type requested", 64275614);
				flag = true;
				intensity = 0f;
				break;
			}
			float t;
			float num2;
			float num3;
			if (flag)
			{
				t = num;
				num2 = -1.5f;
				num3 = 15f;
			}
			else
			{
				t = ((!(num > 0.5f)) ? (0.5f + Mathf.InverseLerp(0f, 0.5f, num) * 0.5f) : (Mathf.InverseLerp(0.5f, 1f, num) * 0.5f));
				num2 = -0.9f;
				num3 = 15f;
			}
			float num4 = Mathf.LerpUnclamped(0f - num3, num3, t);
			float y = num2 - 2.5f * (num4 * num4 / 100f);
			LightInfo result = default(LightInfo);
			result.vector = new Vector2(num4, y);
			result.intensity = intensity;
			return result;
		}

		public static Vector3 CurSunPositionInWorldSpace()
		{
			int ticksAbsForSunPosInWorldSpace = TicksAbsForSunPosInWorldSpace;
			return SunPositionUnmodified(GenDate.DayOfYear(ticksAbsForSunPosInWorldSpace, 0f), GenDate.DayPercent(ticksAbsForSunPosInWorldSpace, 0f), new Vector3(0f, 0f, -1f));
		}

		public static bool IsDaytime(float glow)
		{
			return glow > 0.6f;
		}

		private static Vector3 SunPosition(float latitude, int dayOfYear, float dayPercent)
		{
			Vector3 target = SurfaceNormal(latitude);
			Vector3 current = SunPositionUnmodified(dayOfYear, dayPercent, new Vector3(1f, 0f, 0f), latitude);
			float num = SunPeekAroundDegreesFactorCurve.Evaluate(latitude);
			current = Vector3.RotateTowards(current, target, (float)Math.PI * 19f / 180f * num, 9999999f);
			float num2 = Mathf.InverseLerp(60f, 0f, Mathf.Abs(latitude));
			if (num2 > 0f)
			{
				current = Vector3.RotateTowards(current, target, (float)Math.PI * 2f * (17f * num2 / 360f), 9999999f);
			}
			return current.normalized;
		}

		private static Vector3 SunPositionUnmodified(float dayOfYear, float dayPercent, Vector3 initialSunPos, float latitude = 0f)
		{
			Vector3 point = initialSunPos * 100f;
			float num = 0f - Mathf.Cos(dayOfYear / 60f * (float)Math.PI * 2f);
			point.y += num * 100f * SunOffsetFractionFromLatitudeCurve.Evaluate(latitude);
			point = Quaternion.AngleAxis((dayPercent - 0.5f) * 360f, Vector3.up) * point;
			return point.normalized;
		}

		private static float CelestialSunGlowPercent(float latitude, int dayOfYear, float dayPercent)
		{
			Vector3 vector = SurfaceNormal(latitude);
			Vector3 rhs = SunPosition(latitude, dayOfYear, dayPercent);
			float value = Vector3.Dot(vector.normalized, rhs);
			return Mathf.Clamp01(Mathf.InverseLerp(0f, 0.7f, value));
		}

		public static float AverageGlow(float latitude, int dayOfYear)
		{
			float num = 0f;
			for (int i = 0; i < 24; i++)
			{
				num += CelestialSunGlowPercent(latitude, dayOfYear, (float)i / 24f);
			}
			return num / 24f;
		}

		private static Vector3 SurfaceNormal(float latitude)
		{
			Vector3 point = new Vector3(1f, 0f, 0f);
			return Quaternion.AngleAxis(latitude, new Vector3(0f, 0f, 1f)) * point;
		}

		public static void LogSunGlowForYear()
		{
			for (int i = -90; i <= 90; i += 10)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Sun visibility percents for latitude " + i + ", for each hour of each day of the year");
				stringBuilder.AppendLine("---------------------------------------");
				stringBuilder.Append("Day/hr".PadRight(6));
				for (int j = 0; j < 24; j += 2)
				{
					stringBuilder.Append((j + "h").PadRight(6));
				}
				stringBuilder.AppendLine();
				for (int k = 0; k < 60; k += 5)
				{
					stringBuilder.Append(k.ToString().PadRight(6));
					for (int l = 0; l < 24; l += 2)
					{
						stringBuilder.Append(CelestialSunGlowPercent(i, k, (float)l / 24f).ToString("F2").PadRight(6));
					}
					stringBuilder.AppendLine();
				}
				Log.Message(stringBuilder.ToString());
			}
		}

		public static void LogSunAngleForYear()
		{
			for (int i = -90; i <= 90; i += 10)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Sun angles for latitude " + i + ", for each hour of each day of the year");
				stringBuilder.AppendLine("---------------------------------------");
				stringBuilder.Append("Day/hr".PadRight(6));
				for (int j = 0; j < 24; j += 2)
				{
					stringBuilder.Append((j + "h").PadRight(6));
				}
				stringBuilder.AppendLine();
				for (int k = 0; k < 60; k += 5)
				{
					stringBuilder.Append(k.ToString().PadRight(6));
					for (int l = 0; l < 24; l += 2)
					{
						float num = Vector3.Angle(SurfaceNormal(i), SunPositionUnmodified(k, (float)l / 24f, new Vector3(1f, 0f, 0f)));
						stringBuilder.Append((90f - num).ToString("F0").PadRight(6));
					}
					stringBuilder.AppendLine();
				}
				Log.Message(stringBuilder.ToString());
			}
		}
	}
}
