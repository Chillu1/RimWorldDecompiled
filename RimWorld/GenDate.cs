using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenDate
	{
		public const int TicksPerDay = 60000;

		public const int HoursPerDay = 24;

		public const int DaysPerTwelfth = 5;

		public const int TwelfthsPerYear = 12;

		public const int GameStartHourOfDay = 6;

		public const int TicksPerTwelfth = 300000;

		public const int TicksPerSeason = 900000;

		public const int TicksPerQuadrum = 900000;

		public const int TicksPerYear = 3600000;

		public const int DaysPerYear = 60;

		public const int DaysPerSeason = 15;

		public const int DaysPerQuadrum = 15;

		public const int TicksPerHour = 2500;

		public const float TimeZoneWidth = 15f;

		public const int DefaultStartingYear = 5500;

		private static int TicksGame => Find.TickManager.TicksGame;

		public static int DaysPassed => DaysPassedAt(TicksGame);

		public static float DaysPassedFloat => (float)TicksGame / 60000f;

		public static int TwelfthsPassed => TwelfthsPassedAt(TicksGame);

		public static float TwelfthsPassedFloat => (float)TicksGame / 300000f;

		public static int YearsPassed => YearsPassedAt(TicksGame);

		public static float YearsPassedFloat => (float)TicksGame / 3600000f;

		public static int TickAbsToGame(int absTick)
		{
			return absTick - Find.TickManager.gameStartAbsTick;
		}

		public static int TickGameToAbs(int gameTick)
		{
			return gameTick + Find.TickManager.gameStartAbsTick;
		}

		public static int DaysPassedAt(int gameTicks)
		{
			return Mathf.FloorToInt((float)gameTicks / 60000f);
		}

		public static int TwelfthsPassedAt(int gameTicks)
		{
			return Mathf.FloorToInt((float)gameTicks / 300000f);
		}

		public static int YearsPassedAt(int gameTicks)
		{
			return Mathf.FloorToInt((float)gameTicks / 3600000f);
		}

		private static long LocalTicksOffsetFromLongitude(float longitude)
		{
			return (long)TimeZoneAt(longitude) * 2500L;
		}

		public static int HourOfDay(long absTicks, float longitude)
		{
			return GenMath.PositiveModRemap(absTicks + LocalTicksOffsetFromLongitude(longitude), 2500, 24);
		}

		public static int DayOfTwelfth(long absTicks, float longitude)
		{
			return GenMath.PositiveModRemap(absTicks + LocalTicksOffsetFromLongitude(longitude), 60000, 5);
		}

		public static int DayOfYear(long absTicks, float longitude)
		{
			return GenMath.PositiveModRemap(absTicks + LocalTicksOffsetFromLongitude(longitude), 60000, 60);
		}

		public static Twelfth Twelfth(long absTicks, float longitude)
		{
			return (Twelfth)GenMath.PositiveModRemap(absTicks + LocalTicksOffsetFromLongitude(longitude), 300000, 12);
		}

		public static Season Season(long absTicks, Vector2 longLat)
		{
			return Season(absTicks, longLat.y, longLat.x);
		}

		public static Season Season(long absTicks, float latitude, float longitude)
		{
			return SeasonUtility.GetReportedSeason(YearPercent(absTicks, longitude), latitude);
		}

		public static Quadrum Quadrum(long absTicks, float longitude)
		{
			return Twelfth(absTicks, longitude).GetQuadrum();
		}

		public static int Year(long absTicks, float longitude)
		{
			long num = absTicks + LocalTicksOffsetFromLongitude(longitude);
			return 5500 + Mathf.FloorToInt((float)num / 3600000f);
		}

		public static int DayOfSeason(long absTicks, float longitude)
		{
			return (DayOfYear(absTicks, longitude) - (int)SeasonUtility.FirstSeason.GetFirstTwelfth(0f) * 5) % 15;
		}

		public static int DayOfQuadrum(long absTicks, float longitude)
		{
			return (DayOfYear(absTicks, longitude) - (int)QuadrumUtility.FirstQuadrum.GetFirstTwelfth() * 5) % 15;
		}

		public static int DayTick(long absTicks, float longitude)
		{
			return (int)GenMath.PositiveMod(absTicks + LocalTicksOffsetFromLongitude(longitude), 60000L);
		}

		public static float DayPercent(long absTicks, float longitude)
		{
			int num = DayTick(absTicks, longitude);
			if (num == 0)
			{
				num = 1;
			}
			return (float)num / 60000f;
		}

		public static float YearPercent(long absTicks, float longitude)
		{
			return (float)(int)GenMath.PositiveMod(absTicks + LocalTicksOffsetFromLongitude(longitude), 3600000L) / 3600000f;
		}

		public static int HourInteger(long absTicks, float longitude)
		{
			return GenMath.PositiveModRemap(absTicks + LocalTicksOffsetFromLongitude(longitude), 2500, 24);
		}

		public static float HourFloat(long absTicks, float longitude)
		{
			return DayPercent(absTicks, longitude) * 24f;
		}

		public static string DateFullStringAt(long absTicks, Vector2 location)
		{
			int num = DayOfSeason(absTicks, location.x) + 1;
			string value = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "FullDate".Translate(value, Quadrum(absTicks, location.x).Label(), Year(absTicks, location.x), num);
		}

		public static string DateFullStringWithHourAt(long absTicks, Vector2 location)
		{
			return DateFullStringAt(absTicks, location) + ", " + HourInteger(absTicks, location.x) + "LetterHour".Translate();
		}

		public static string DateReadoutStringAt(long absTicks, Vector2 location)
		{
			int num = DayOfSeason(absTicks, location.x) + 1;
			string value = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "DateReadout".Translate(value, Quadrum(absTicks, location.x).Label(), Year(absTicks, location.x), num);
		}

		public static string DateShortStringAt(long absTicks, Vector2 location)
		{
			int value = DayOfSeason(absTicks, location.x) + 1;
			return "ShortDate".Translate(value, Quadrum(absTicks, location.x).LabelShort(), Year(absTicks, location.x), value);
		}

		public static string SeasonDateStringAt(long absTicks, Vector2 longLat)
		{
			int num = DayOfSeason(absTicks, longLat.x) + 1;
			string value = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "SeasonFullDate".Translate(value, Season(absTicks, longLat).Label(), num);
		}

		public static string SeasonDateStringAt(Twelfth twelfth, Vector2 longLat)
		{
			return SeasonDateStringAt((int)twelfth * 300000 + 1, longLat);
		}

		public static string QuadrumDateStringAt(long absTicks, float longitude)
		{
			int num = DayOfQuadrum(absTicks, longitude) + 1;
			string value = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "SeasonFullDate".Translate(value, Quadrum(absTicks, longitude).Label(), num);
		}

		public static string QuadrumDateStringAt(Quadrum quadrum)
		{
			return QuadrumDateStringAt((int)quadrum * 900000 + 1, 0f);
		}

		public static string QuadrumDateStringAt(Twelfth twelfth)
		{
			return QuadrumDateStringAt((int)twelfth * 300000 + 1, 0f);
		}

		public static float TicksToDays(this int numTicks)
		{
			return (float)numTicks / 60000f;
		}

		public static string ToStringTicksToDays(this int numTicks, string format = "F1")
		{
			string text = numTicks.TicksToDays().ToString(format);
			if (text == "1")
			{
				return "Period1Day".Translate();
			}
			return text + " " + "DaysLower".Translate();
		}

		public static string ToStringTicksToPeriod(this int numTicks, bool allowSeconds = true, bool shortForm = false, bool canUseDecimals = true, bool allowYears = true)
		{
			if (allowSeconds && numTicks < 2500 && (numTicks < 600 || Math.Round((float)numTicks / 2500f, 1) == 0.0))
			{
				int num = Mathf.RoundToInt((float)numTicks / 60f);
				if (shortForm)
				{
					return num + (string)"LetterSecond".Translate();
				}
				if (num == 1)
				{
					return "Period1Second".Translate();
				}
				return "PeriodSeconds".Translate(num);
			}
			if (numTicks < 60000)
			{
				if (shortForm)
				{
					return Mathf.RoundToInt((float)numTicks / 2500f) + (string)"LetterHour".Translate();
				}
				if (numTicks < 2500)
				{
					string text = ((float)numTicks / 2500f).ToString("0.#");
					if (text == "1")
					{
						return "Period1Hour".Translate();
					}
					return "PeriodHours".Translate(text);
				}
				int num2 = Mathf.RoundToInt((float)numTicks / 2500f);
				if (num2 == 1)
				{
					return "Period1Hour".Translate();
				}
				return "PeriodHours".Translate(num2);
			}
			if (numTicks < 3600000 || !allowYears)
			{
				if (shortForm)
				{
					return Mathf.RoundToInt((float)numTicks / 60000f) + (string)"LetterDay".Translate();
				}
				string text2 = (!canUseDecimals) ? Mathf.RoundToInt((float)numTicks / 60000f).ToString() : ((float)numTicks / 60000f).ToStringDecimalIfSmall();
				if (text2 == "1")
				{
					return "Period1Day".Translate();
				}
				return "PeriodDays".Translate(text2);
			}
			if (shortForm)
			{
				return Mathf.RoundToInt((float)numTicks / 3600000f) + (string)"LetterYear".Translate();
			}
			string text3 = (!canUseDecimals) ? Mathf.RoundToInt((float)numTicks / 3600000f).ToString() : ((float)numTicks / 3600000f).ToStringDecimalIfSmall();
			if (text3 == "1")
			{
				return "Period1Year".Translate();
			}
			return "PeriodYears".Translate(text3);
		}

		public static string ToStringTicksToPeriodVerbose(this int numTicks, bool allowHours = true, bool allowQuadrums = true)
		{
			if (numTicks < 0)
			{
				return "0";
			}
			numTicks.TicksToPeriod(out int years, out int quadrums, out int days, out float hoursFloat);
			if (!allowQuadrums)
			{
				days += 15 * quadrums;
				quadrums = 0;
			}
			if (years > 0)
			{
				string text = (years != 1) ? ((string)"PeriodYears".Translate(years)) : ((string)"Period1Year".Translate());
				if (quadrums > 0)
				{
					text += ", ";
					text = ((quadrums != 1) ? ((string)(text + "PeriodQuadrums".Translate(quadrums))) : ((string)(text + "Period1Quadrum".Translate())));
				}
				return text;
			}
			if (quadrums > 0)
			{
				string text2 = (quadrums != 1) ? ((string)"PeriodQuadrums".Translate(quadrums)) : ((string)"Period1Quadrum".Translate());
				if (days > 0)
				{
					text2 += ", ";
					text2 = ((days != 1) ? ((string)(text2 + "PeriodDays".Translate(days))) : ((string)(text2 + "Period1Day".Translate())));
				}
				return text2;
			}
			if (days > 0)
			{
				string text3 = (days != 1) ? ((string)"PeriodDays".Translate(days)) : ((string)"Period1Day".Translate());
				int num = (int)hoursFloat;
				if (allowHours && num > 0)
				{
					text3 += ", ";
					text3 = ((num != 1) ? ((string)(text3 + "PeriodHours".Translate(num))) : ((string)(text3 + "Period1Hour".Translate())));
				}
				return text3;
			}
			if (allowHours)
			{
				if (hoursFloat > 1f)
				{
					int num2 = Mathf.RoundToInt(hoursFloat);
					if (num2 == 1)
					{
						return "Period1Hour".Translate();
					}
					return "PeriodHours".Translate(num2);
				}
				if (Math.Round(hoursFloat, 1) == 1.0)
				{
					return "Period1Hour".Translate();
				}
				return "PeriodHours".Translate(hoursFloat.ToString("0.#"));
			}
			return "PeriodDays".Translate(0);
		}

		public static string ToStringTicksToPeriodVague(this int numTicks, bool vagueMin = true, bool vagueMax = true)
		{
			if (vagueMax && numTicks > 36000000)
			{
				return "OverADecade".Translate();
			}
			if (vagueMin && numTicks < 60000)
			{
				return "LessThanADay".Translate();
			}
			return numTicks.ToStringTicksToPeriod();
		}

		public static void TicksToPeriod(this int numTicks, out int years, out int quadrums, out int days, out float hoursFloat)
		{
			((long)numTicks).TicksToPeriod(out years, out quadrums, out days, out hoursFloat);
		}

		public static void TicksToPeriod(this long numTicks, out int years, out int quadrums, out int days, out float hoursFloat)
		{
			if (numTicks < 0)
			{
				Log.ErrorOnce("Tried to calculate period for negative ticks", 12841103);
			}
			long num = numTicks;
			years = (int)(num / 3600000);
			num -= (long)years * 3600000L;
			quadrums = (int)(num / 900000);
			num -= (long)quadrums * 900000L;
			days = (int)(num / 60000);
			num -= (long)days * 60000L;
			hoursFloat = (float)num / 2500f;
		}

		public static string ToStringApproxAge(this float yearsFloat)
		{
			if (yearsFloat >= 1f)
			{
				return ((int)yearsFloat).ToStringCached();
			}
			Mathf.Min((int)(yearsFloat * 3600000f), 3599999).TicksToPeriod(out int years, out int quadrums, out int days, out float hoursFloat);
			if (years > 0)
			{
				if (years == 1)
				{
					return "Period1Year".Translate();
				}
				return "PeriodYears".Translate(years);
			}
			if (quadrums > 0)
			{
				if (quadrums == 1)
				{
					return "Period1Quadrum".Translate();
				}
				return "PeriodQuadrums".Translate(quadrums);
			}
			if (days > 0)
			{
				if (days == 1)
				{
					return "Period1Day".Translate();
				}
				return "PeriodDays".Translate(days);
			}
			int num = (int)hoursFloat;
			if (num == 1)
			{
				return "Period1Hour".Translate();
			}
			return "PeriodHours".Translate(num);
		}

		public static int TimeZoneAt(float longitude)
		{
			return Mathf.RoundToInt(TimeZoneFloatAt(longitude));
		}

		public static float TimeZoneFloatAt(float longitude)
		{
			return longitude / 15f;
		}
	}
}
