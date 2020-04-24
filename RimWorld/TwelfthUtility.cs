using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class TwelfthUtility
	{
		public static Quadrum GetQuadrum(this Twelfth twelfth)
		{
			switch (twelfth)
			{
			case Twelfth.First:
				return Quadrum.Aprimay;
			case Twelfth.Second:
				return Quadrum.Aprimay;
			case Twelfth.Third:
				return Quadrum.Aprimay;
			case Twelfth.Fourth:
				return Quadrum.Jugust;
			case Twelfth.Fifth:
				return Quadrum.Jugust;
			case Twelfth.Sixth:
				return Quadrum.Jugust;
			case Twelfth.Seventh:
				return Quadrum.Septober;
			case Twelfth.Eighth:
				return Quadrum.Septober;
			case Twelfth.Ninth:
				return Quadrum.Septober;
			case Twelfth.Tenth:
				return Quadrum.Decembary;
			case Twelfth.Eleventh:
				return Quadrum.Decembary;
			case Twelfth.Twelfth:
				return Quadrum.Decembary;
			default:
				return Quadrum.Undefined;
			}
		}

		public static Twelfth PreviousTwelfth(this Twelfth twelfth)
		{
			if (twelfth == Twelfth.Undefined)
			{
				return Twelfth.Undefined;
			}
			int num = (int)(twelfth - 1);
			if (num == -1)
			{
				num = 11;
			}
			return (Twelfth)num;
		}

		public static Twelfth NextTwelfth(this Twelfth twelfth)
		{
			if (twelfth == Twelfth.Undefined)
			{
				return Twelfth.Undefined;
			}
			return (Twelfth)((int)(twelfth + 1) % 12);
		}

		public static float GetMiddleYearPct(this Twelfth twelfth)
		{
			return ((float)(int)twelfth + 0.5f) / 12f;
		}

		public static float GetBeginningYearPct(this Twelfth twelfth)
		{
			return (float)(int)twelfth / 12f;
		}

		public static Twelfth FindStartingWarmTwelfth(int tile)
		{
			Twelfth twelfth = GenTemperature.EarliestTwelfthInAverageTemperatureRange(tile, 16f, 9999f);
			if (twelfth == Twelfth.Undefined)
			{
				twelfth = Season.Summer.GetFirstTwelfth(Find.WorldGrid.LongLatOf(tile).y);
			}
			return twelfth;
		}

		public static Twelfth GetLeftMostTwelfth(List<Twelfth> twelfths, Twelfth rootTwelfth)
		{
			if (twelfths.Count >= 12)
			{
				return Twelfth.Undefined;
			}
			Twelfth result;
			do
			{
				result = rootTwelfth;
				rootTwelfth = TwelfthBefore(rootTwelfth);
			}
			while (twelfths.Contains(rootTwelfth));
			return result;
		}

		public static Twelfth GetRightMostTwelfth(List<Twelfth> twelfths, Twelfth rootTwelfth)
		{
			if (twelfths.Count >= 12)
			{
				return Twelfth.Undefined;
			}
			Twelfth m;
			do
			{
				m = rootTwelfth;
				rootTwelfth = TwelfthAfter(rootTwelfth);
			}
			while (twelfths.Contains(rootTwelfth));
			return TwelfthAfter(m);
		}

		public static Twelfth TwelfthBefore(Twelfth m)
		{
			if (m == Twelfth.First)
			{
				return Twelfth.Twelfth;
			}
			return m - 1;
		}

		public static Twelfth TwelfthAfter(Twelfth m)
		{
			if (m == Twelfth.Twelfth)
			{
				return Twelfth.First;
			}
			return m + 1;
		}
	}
}
