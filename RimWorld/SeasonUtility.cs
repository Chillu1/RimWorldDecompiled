using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class SeasonUtility
	{
		private const float HemisphereLerpDistance = 5f;

		private const float SeasonYearPctLerpDistance = 0.085f;

		private static readonly SimpleCurve SeasonalAreaSeasons = new SimpleCurve
		{
			new CurvePoint(-0.0425000042f, 0f),
			new CurvePoint(0.0425000042f, 1f),
			new CurvePoint(0.2075f, 1f),
			new CurvePoint(0.292500019f, 2f),
			new CurvePoint(0.457499981f, 2f),
			new CurvePoint(0.5425f, 3f),
			new CurvePoint(0.7075f, 3f),
			new CurvePoint(0.7925f, 4f),
			new CurvePoint(0.9575f, 4f),
			new CurvePoint(1.0425f, 5f)
		};

		public static Season FirstSeason => Season.Spring;

		public static Season GetReportedSeason(float yearPct, float latitude)
		{
			GetSeason(yearPct, latitude, out float spring, out float summer, out float fall, out float winter, out float permanentSummer, out float permanentWinter);
			if (permanentSummer == 1f)
			{
				return Season.PermanentSummer;
			}
			if (permanentWinter == 1f)
			{
				return Season.PermanentWinter;
			}
			return GenMath.MaxBy(Season.Spring, spring, Season.Summer, summer, Season.Fall, fall, Season.Winter, winter);
		}

		public static Season GetDominantSeason(float yearPct, float latitude)
		{
			GetSeason(yearPct, latitude, out float spring, out float summer, out float fall, out float winter, out float permanentSummer, out float permanentWinter);
			return GenMath.MaxBy(Season.Spring, spring, Season.Summer, summer, Season.Fall, fall, Season.Winter, winter, Season.PermanentSummer, permanentSummer, Season.PermanentWinter, permanentWinter);
		}

		public static void GetSeason(float yearPct, float latitude, out float spring, out float summer, out float fall, out float winter, out float permanentSummer, out float permanentWinter)
		{
			yearPct = Mathf.Clamp01(yearPct);
			LatitudeSectionUtility.GetLatitudeSection(latitude, out float equatorial, out float seasonal, out float polar);
			GetSeasonalAreaSeason(yearPct, out float spring2, out float summer2, out float fall2, out float winter2, northernHemisphere: true);
			GetSeasonalAreaSeason(yearPct, out float spring3, out float summer3, out float fall3, out float winter3, northernHemisphere: false);
			float num = Mathf.InverseLerp(-2.5f, 2.5f, latitude);
			float num2 = num * spring2 + (1f - num) * spring3;
			float num3 = num * summer2 + (1f - num) * summer3;
			float num4 = num * fall2 + (1f - num) * fall3;
			float num5 = num * winter2 + (1f - num) * winter3;
			spring = num2 * seasonal;
			summer = num3 * seasonal;
			fall = num4 * seasonal;
			winter = num5 * seasonal;
			permanentSummer = equatorial;
			permanentWinter = polar;
		}

		private static void GetSeasonalAreaSeason(float yearPct, out float spring, out float summer, out float fall, out float winter, bool northernHemisphere)
		{
			yearPct = Mathf.Clamp01(yearPct);
			float x = northernHemisphere ? yearPct : ((yearPct + 0.5f) % 1f);
			float num = SeasonalAreaSeasons.Evaluate(x);
			if (num <= 1f)
			{
				winter = 1f - num;
				spring = num;
				summer = 0f;
				fall = 0f;
			}
			else if (num <= 2f)
			{
				spring = 1f - (num - 1f);
				summer = num - 1f;
				fall = 0f;
				winter = 0f;
			}
			else if (num <= 3f)
			{
				summer = 1f - (num - 2f);
				fall = num - 2f;
				spring = 0f;
				winter = 0f;
			}
			else if (num <= 4f)
			{
				fall = 1f - (num - 3f);
				winter = num - 3f;
				spring = 0f;
				summer = 0f;
			}
			else
			{
				winter = 1f - (num - 4f);
				spring = num - 4f;
				summer = 0f;
				fall = 0f;
			}
		}

		public static Twelfth GetFirstTwelfth(this Season season, float latitude)
		{
			if (latitude >= 0f)
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.First;
				case Season.Summer:
					return Twelfth.Fourth;
				case Season.Fall:
					return Twelfth.Seventh;
				case Season.Winter:
					return Twelfth.Tenth;
				case Season.PermanentSummer:
					return Twelfth.First;
				case Season.PermanentWinter:
					return Twelfth.First;
				}
			}
			else
			{
				switch (season)
				{
				case Season.Fall:
					return Twelfth.First;
				case Season.Winter:
					return Twelfth.Fourth;
				case Season.Spring:
					return Twelfth.Seventh;
				case Season.Summer:
					return Twelfth.Tenth;
				case Season.PermanentSummer:
					return Twelfth.First;
				case Season.PermanentWinter:
					return Twelfth.First;
				}
			}
			return Twelfth.Undefined;
		}

		public static Twelfth GetMiddleTwelfth(this Season season, float latitude)
		{
			if (latitude >= 0f)
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.Second;
				case Season.Summer:
					return Twelfth.Fifth;
				case Season.Fall:
					return Twelfth.Eighth;
				case Season.Winter:
					return Twelfth.Eleventh;
				case Season.PermanentSummer:
					return Twelfth.Sixth;
				case Season.PermanentWinter:
					return Twelfth.Sixth;
				}
			}
			else
			{
				switch (season)
				{
				case Season.Fall:
					return Twelfth.Second;
				case Season.Winter:
					return Twelfth.Fifth;
				case Season.Spring:
					return Twelfth.Eighth;
				case Season.Summer:
					return Twelfth.Eleventh;
				case Season.PermanentSummer:
					return Twelfth.Sixth;
				case Season.PermanentWinter:
					return Twelfth.Sixth;
				}
			}
			return Twelfth.Undefined;
		}

		public static Season GetPreviousSeason(this Season season)
		{
			switch (season)
			{
			case Season.Undefined:
				return Season.Undefined;
			case Season.Spring:
				return Season.Winter;
			case Season.Summer:
				return Season.Spring;
			case Season.Fall:
				return Season.Summer;
			case Season.Winter:
				return Season.Fall;
			case Season.PermanentSummer:
				return Season.PermanentSummer;
			case Season.PermanentWinter:
				return Season.PermanentWinter;
			default:
				return Season.Undefined;
			}
		}

		public static float GetMiddleYearPct(this Season season, float latitude)
		{
			if (season == Season.Undefined)
			{
				return 0.5f;
			}
			return season.GetMiddleTwelfth(latitude).GetMiddleYearPct();
		}

		public static string Label(this Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return "SeasonSpring".Translate();
			case Season.Summer:
				return "SeasonSummer".Translate();
			case Season.Fall:
				return "SeasonFall".Translate();
			case Season.Winter:
				return "SeasonWinter".Translate();
			case Season.PermanentSummer:
				return "SeasonPermanentSummer".Translate();
			case Season.PermanentWinter:
				return "SeasonPermanentWinter".Translate();
			default:
				return "Unknown season";
			}
		}

		public static string LabelCap(this Season season)
		{
			return season.Label().CapitalizeFirst();
		}

		public static string SeasonsRangeLabel(List<Twelfth> twelfths, Vector2 longLat)
		{
			if (twelfths.Count == 0)
			{
				return "";
			}
			if (twelfths.Count == 12)
			{
				return "WholeYear".Translate();
			}
			string text = "";
			for (int i = 0; i < 12; i++)
			{
				Twelfth twelfth = (Twelfth)i;
				if (twelfths.Contains(twelfth))
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += SeasonsContinuousRangeLabel(twelfths, twelfth, longLat);
				}
			}
			return text;
		}

		private static string SeasonsContinuousRangeLabel(List<Twelfth> twelfths, Twelfth rootTwelfth, Vector2 longLat)
		{
			Twelfth leftMostTwelfth = TwelfthUtility.GetLeftMostTwelfth(twelfths, rootTwelfth);
			Twelfth rightMostTwelfth = TwelfthUtility.GetRightMostTwelfth(twelfths, rootTwelfth);
			for (Twelfth twelfth = leftMostTwelfth; twelfth != rightMostTwelfth; twelfth = TwelfthUtility.TwelfthAfter(twelfth))
			{
				if (!twelfths.Contains(twelfth))
				{
					Log.Error("Twelfths doesn't contain " + twelfth + " (" + leftMostTwelfth + ".." + rightMostTwelfth + ")");
					break;
				}
				twelfths.Remove(twelfth);
			}
			twelfths.Remove(rightMostTwelfth);
			return GenDate.SeasonDateStringAt(leftMostTwelfth, longLat) + " - " + GenDate.SeasonDateStringAt(rightMostTwelfth, longLat);
		}
	}
}
