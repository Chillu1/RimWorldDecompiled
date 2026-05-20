using System;
using System.Globalization;
using UnityEngine;

namespace Verse;

public static class MathEvaluatorCustomFunctions
{
	public class FunctionType
	{
		public string name;

		public int minArgs;

		public int maxArgs;

		public Func<object[], object> func;
	}

	public static readonly FunctionType[] FunctionTypes = new FunctionType[11]
	{
		new FunctionType
		{
			name = "min",
			minArgs = 2,
			maxArgs = 2,
			func = Min
		},
		new FunctionType
		{
			name = "max",
			minArgs = 2,
			maxArgs = 2,
			func = Max
		},
		new FunctionType
		{
			name = "round",
			minArgs = 1,
			maxArgs = 1,
			func = Round
		},
		new FunctionType
		{
			name = "roundToDigits",
			minArgs = 2,
			maxArgs = 2,
			func = RoundToDigits
		},
		new FunctionType
		{
			name = "floor",
			minArgs = 1,
			maxArgs = 1,
			func = Floor
		},
		new FunctionType
		{
			name = "roundRandom",
			minArgs = 1,
			maxArgs = 1,
			func = RoundRandom
		},
		new FunctionType
		{
			name = "randInt",
			minArgs = 2,
			maxArgs = 2,
			func = RandInt
		},
		new FunctionType
		{
			name = "randFloat",
			minArgs = 2,
			maxArgs = 2,
			func = RandFloat
		},
		new FunctionType
		{
			name = "rangeAverage",
			minArgs = 1,
			maxArgs = 1,
			func = RangeAverage
		},
		new FunctionType
		{
			name = "roundToTicksRough",
			minArgs = 1,
			maxArgs = 1,
			func = RoundToTicksRough
		},
		new FunctionType
		{
			name = "lerp",
			minArgs = 3,
			maxArgs = 3,
			func = Lerp
		}
	};

	private static object Min(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Math.Min(Convert.ToDouble(args[0], invariantCulture), Convert.ToDouble(args[1], invariantCulture));
	}

	private static object Max(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Math.Max(Convert.ToDouble(args[0], invariantCulture), Convert.ToDouble(args[1], invariantCulture));
	}

	private static object Round(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Math.Round(Convert.ToDouble(args[0], invariantCulture));
	}

	private static object RoundToDigits(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Math.Round(Convert.ToDouble(args[0], invariantCulture), Convert.ToInt32(args[1], invariantCulture));
	}

	private static object Floor(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Math.Floor(Convert.ToDouble(args[0], invariantCulture));
	}

	private static object RoundRandom(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (double)GenMath.RoundRandom(Convert.ToSingle(args[0], invariantCulture));
	}

	private static object RandInt(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (double)Rand.RangeInclusive(Convert.ToInt32(args[0], invariantCulture), Convert.ToInt32(args[1], invariantCulture));
	}

	private static object RandFloat(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (double)Rand.Range(Convert.ToSingle(args[0], invariantCulture), Convert.ToSingle(args[1], invariantCulture));
	}

	private static object RangeAverage(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (double)FloatRange.FromString(Convert.ToString(args[0], invariantCulture)).Average;
	}

	private static object RoundToTicksRough(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		int num = Convert.ToInt32(args[0], invariantCulture);
		if (num <= 250)
		{
			return 250;
		}
		if (num < 5000)
		{
			return GenMath.RoundTo(num, 250);
		}
		if (num < 60000)
		{
			return GenMath.RoundTo(num, 2500);
		}
		if (num < 120000)
		{
			return GenMath.RoundTo(num, 6000);
		}
		return GenMath.RoundTo(num, 60000);
	}

	public static object Lerp(object[] args)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return Mathf.Lerp(Convert.ToSingle(args[0], invariantCulture), Convert.ToSingle(args[1], invariantCulture), Convert.ToSingle(args[2], invariantCulture));
	}
}
