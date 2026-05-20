using System;
using Verse;

namespace RimWorld;

public struct QualityRange : IEquatable<QualityRange>
{
	public QualityCategory min;

	public QualityCategory max;

	public static QualityRange All => new QualityRange(QualityCategory.Awful, QualityCategory.Legendary);

	public QualityRange(QualityCategory min, QualityCategory max)
	{
		this.min = min;
		this.max = max;
	}

	public bool Includes(QualityCategory p)
	{
		if ((int)p >= (int)min)
		{
			return (int)p <= (int)max;
		}
		return false;
	}

	public static bool operator ==(QualityRange a, QualityRange b)
	{
		if (a.min == b.min)
		{
			return a.max == b.max;
		}
		return false;
	}

	public static bool operator !=(QualityRange a, QualityRange b)
	{
		return !(a == b);
	}

	public static QualityRange FromString(string s)
	{
		string[] array = s.Split('~');
		return new QualityRange(ParseHelper.FromString<QualityCategory>(array[0]), ParseHelper.FromString<QualityCategory>(array[1]));
	}

	public override string ToString()
	{
		return min.ToString() + "~" + max;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineStruct(min.GetHashCode(), max);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is QualityRange qualityRange))
		{
			return false;
		}
		if (qualityRange.min == min)
		{
			return qualityRange.max == max;
		}
		return false;
	}

	public bool Equals(QualityRange other)
	{
		if (other.min == min)
		{
			return other.max == max;
		}
		return false;
	}
}
