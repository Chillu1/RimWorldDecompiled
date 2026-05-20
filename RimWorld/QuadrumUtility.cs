using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class QuadrumUtility
{
	private static readonly List<Quadrum> quadrumList = new List<Quadrum>
	{
		Quadrum.Aprimay,
		Quadrum.Decembary,
		Quadrum.Jugust,
		Quadrum.Septober
	};

	private static readonly List<Quadrum> quadrumListInChronologicanOrder = new List<Quadrum>
	{
		Quadrum.Aprimay,
		Quadrum.Jugust,
		Quadrum.Septober,
		Quadrum.Decembary
	};

	public static Quadrum FirstQuadrum => Quadrum.Aprimay;

	public static List<Quadrum> Quadrums => quadrumList;

	public static List<Quadrum> QuadrumsInChronologicalOrder => quadrumListInChronologicanOrder;

	public static Twelfth GetFirstTwelfth(this Quadrum quadrum)
	{
		return quadrum switch
		{
			Quadrum.Aprimay => Twelfth.First, 
			Quadrum.Jugust => Twelfth.Fourth, 
			Quadrum.Septober => Twelfth.Seventh, 
			Quadrum.Decembary => Twelfth.Tenth, 
			_ => Twelfth.Undefined, 
		};
	}

	public static Twelfth GetMiddleTwelfth(this Quadrum quadrum)
	{
		return quadrum switch
		{
			Quadrum.Aprimay => Twelfth.Second, 
			Quadrum.Jugust => Twelfth.Fifth, 
			Quadrum.Septober => Twelfth.Eighth, 
			Quadrum.Decembary => Twelfth.Eleventh, 
			_ => Twelfth.Undefined, 
		};
	}

	public static float GetMiddleYearPct(this Quadrum quadrum)
	{
		return quadrum.GetMiddleTwelfth().GetMiddleYearPct();
	}

	public static string Label(this Quadrum quadrum)
	{
		return quadrum switch
		{
			Quadrum.Aprimay => "QuadrumAprimay".Translate(), 
			Quadrum.Jugust => "QuadrumJugust".Translate(), 
			Quadrum.Septober => "QuadrumSeptober".Translate(), 
			Quadrum.Decembary => "QuadrumDecembary".Translate(), 
			_ => "Unknown quadrum", 
		};
	}

	public static string LabelShort(this Quadrum quadrum)
	{
		return quadrum switch
		{
			Quadrum.Aprimay => "QuadrumAprimay_Short".Translate(), 
			Quadrum.Jugust => "QuadrumJugust_Short".Translate(), 
			Quadrum.Septober => "QuadrumSeptober_Short".Translate(), 
			Quadrum.Decembary => "QuadrumDecembary_Short".Translate(), 
			_ => "Unknown quadrum", 
		};
	}

	public static Season GetSeason(this Quadrum q, Vector2 location)
	{
		return q.GetSeason(location.y);
	}

	public static Season GetSeason(this Quadrum q, float latitude)
	{
		return SeasonUtility.GetReportedSeason(q.GetMiddleYearPct(), latitude);
	}

	public static string QuadrumsRangeLabel(List<Twelfth> twelfths)
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
				text += QuadrumsContinuousRangeLabel(twelfths, twelfth);
			}
		}
		return text;
	}

	private static string QuadrumsContinuousRangeLabel(List<Twelfth> twelfths, Twelfth rootTwelfth)
	{
		Twelfth leftMostTwelfth = TwelfthUtility.GetLeftMostTwelfth(twelfths, rootTwelfth);
		Twelfth rightMostTwelfth = TwelfthUtility.GetRightMostTwelfth(twelfths, rootTwelfth);
		for (Twelfth twelfth = leftMostTwelfth; twelfth != rightMostTwelfth; twelfth = TwelfthUtility.TwelfthAfter(twelfth))
		{
			if (!twelfths.Contains(twelfth))
			{
				Log.Error("Twelfths doesn't contain " + twelfth.ToString() + " (" + leftMostTwelfth.ToString() + ".." + rightMostTwelfth.ToString() + ")");
				break;
			}
			twelfths.Remove(twelfth);
		}
		twelfths.Remove(rightMostTwelfth);
		return GenDate.QuadrumDateStringAt(leftMostTwelfth) + " - " + GenDate.QuadrumDateStringAt(rightMostTwelfth);
	}
}
