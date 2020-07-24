using System;
using System.Globalization;
using UnityEngine;

namespace Verse
{
	public struct IntRange : IEquatable<IntRange>
	{
		public int min;

		public int max;

		public static IntRange zero => new IntRange(0, 0);

		public static IntRange one => new IntRange(1, 1);

		public int TrueMin => Mathf.Min(min, max);

		public int TrueMax => Mathf.Max(min, max);

		public float Average => ((float)min + (float)max) / 2f;

		public int RandomInRange => Rand.RangeInclusive(min, max);

		public IntRange(int min, int max)
		{
			this.min = min;
			this.max = max;
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
			return new IntRange(Convert.ToInt32(array[0], invariantCulture), Convert.ToInt32(array[1], invariantCulture));
		}

		public override string ToString()
		{
			return min + "~" + max;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(min, max);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IntRange))
			{
				return false;
			}
			return Equals((IntRange)obj);
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
}
