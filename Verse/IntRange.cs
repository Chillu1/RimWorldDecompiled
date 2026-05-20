using System;
using System.Globalization;
using UnityEngine;

namespace Verse;

public struct IntRange : IEquatable<IntRange>
{
	public int min;

	public int max;

	public static IntRange Zero => new IntRange(0, 0);

	[Obsolete]
	public static IntRange zero => Zero;

	public static IntRange One => new IntRange(1, 1);

	[Obsolete]
	public static IntRange one => One;

	public static IntRange Invalid => new IntRange(-1, -1);

	[Obsolete]
	public static IntRange invalid => Invalid;

	public int TrueMin => Mathf.Min(min, max);

	public int TrueMax => Mathf.Max(min, max);

	public float Average => ((float)min + (float)max) / 2f;

	public int RandomInRange => Rand.RangeInclusive(min, max);

	public bool IsValid => this != Invalid;

	public bool IsInvalid => this == Invalid;

	public static IntRange Between(int min, int max)
	{
		return new IntRange(min, max);
	}

	public IntRange(int min, int max)
	{
		this.min = min;
		this.max = max;
	}

	public IntRange(int val)
	{
		min = val;
		max = val;
	}

	public int Lerped(float lerpFactor)
	{
		return min + Mathf.RoundToInt(lerpFactor * (float)(max - min));
	}

	public static IntRange FromString(string s)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string[] array = s.Split('~');
		if (array.Length == 1)
		{
			int num = Convert.ToInt32(array[0], invariantCulture);
			return new IntRange(num, num);
		}
		int num2 = (array[0].NullOrEmpty() ? int.MinValue : Convert.ToInt32(array[0], invariantCulture));
		int num3 = (array[1].NullOrEmpty() ? int.MaxValue : Convert.ToInt32(array[1], invariantCulture));
		return new IntRange(num2, num3);
	}

	public override string ToString()
	{
		if (min == max)
		{
			return min.ToString();
		}
		return min + "~" + max;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(min, max);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IntRange other))
		{
			return false;
		}
		return Equals(other);
	}

	public bool Equals(IntRange other)
	{
		if (min == other.min)
		{
			return max == other.max;
		}
		return false;
	}

	public static bool operator ==(IntRange lhs, IntRange rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(IntRange lhs, IntRange rhs)
	{
		return !(lhs == rhs);
	}

	internal bool Includes(int val)
	{
		if (val >= min)
		{
			return val <= max;
		}
		return false;
	}
}
