using System;
using System.Globalization;
using UnityEngine;

namespace Verse;

public struct ByteRange : IEquatable<ByteRange>
{
	public byte min;

	public byte max;

	public static ByteRange Zero => new ByteRange(0, 0);

	public static ByteRange One => new ByteRange(1, 1);

	public byte TrueMin => Math.Min(min, max);

	public byte TrueMax => Math.Max(min, max);

	public float Average => ((float)(int)min + (float)(int)max) / 2f;

	public int RandomInRange => Rand.RangeInclusive(min, max);

	public ByteRange(byte min, byte max)
	{
		this.min = min;
		this.max = max;
	}

	public int Lerped(float lerpFactor)
	{
		return min + Mathf.RoundToInt(lerpFactor * (float)(max - min));
	}

	public static ByteRange FromString(string s)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string[] array = s.Split('~');
		if (array.Length == 1)
		{
			byte num = Convert.ToByte(array[0], invariantCulture);
			return new ByteRange(num, num);
		}
		int num2 = ((!array[0].NullOrEmpty()) ? Convert.ToByte(array[0], invariantCulture) : 0);
		byte b = (array[1].NullOrEmpty() ? byte.MaxValue : Convert.ToByte(array[1], invariantCulture));
		return new ByteRange((byte)num2, b);
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
		if (!(obj is ByteRange other))
		{
			return false;
		}
		return Equals(other);
	}

	public bool Equals(ByteRange other)
	{
		if (min == other.min)
		{
			return max == other.max;
		}
		return false;
	}

	public static bool operator ==(ByteRange lhs, ByteRange rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ByteRange lhs, ByteRange rhs)
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
