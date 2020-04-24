using System;
using System.Globalization;
using UnityEngine;

namespace Verse
{
	public struct FloatRange : IEquatable<FloatRange>
	{
		public float min;

		public float max;

		public static FloatRange Zero => new FloatRange(0f, 0f);

		public static FloatRange One => new FloatRange(1f, 1f);

		public static FloatRange ZeroToOne => new FloatRange(0f, 1f);

		public float Average => (min + max) / 2f;

		public float RandomInRange => Rand.Range(min, max);

		public float TrueMin => Mathf.Min(min, max);

		public float TrueMax => Mathf.Max(min, max);

		public float Span => TrueMax - TrueMin;

		public FloatRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float ClampToRange(float value)
		{
			return Mathf.Clamp(value, min, max);
		}

		public float RandomInRangeSeeded(int seed)
		{
			return Rand.RangeSeeded(min, max, seed);
		}

		public float LerpThroughRange(float lerpPct)
		{
			return Mathf.Lerp(min, max, lerpPct);
		}

		public float InverseLerpThroughRange(float f)
		{
			return Mathf.InverseLerp(min, max, f);
		}

		public bool Includes(float f)
		{
			if (f >= min)
			{
				return f <= max;
			}
			return false;
		}

		public bool IncludesEpsilon(float f)
		{
			if (f >= min - 1E-05f)
			{
				return f <= max + 1E-05f;
			}
			return false;
		}

		public FloatRange ExpandedBy(float f)
		{
			return new FloatRange(min - f, max + f);
		}

		public static bool operator ==(FloatRange a, FloatRange b)
		{
			if (a.min == b.min)
			{
				return a.max == b.max;
			}
			return false;
		}

		public static bool operator !=(FloatRange a, FloatRange b)
		{
			if (a.min == b.min)
			{
				return a.max != b.max;
			}
			return true;
		}

		public static FloatRange operator *(FloatRange r, float val)
		{
			return new FloatRange(r.min * val, r.max * val);
		}

		public static FloatRange operator *(float val, FloatRange r)
		{
			return new FloatRange(r.min * val, r.max * val);
		}

		public static FloatRange FromString(string s)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			string[] array = s.Split('~');
			if (array.Length == 1)
			{
				float num = Convert.ToSingle(array[0], invariantCulture);
				return new FloatRange(num, num);
			}
			return new FloatRange(Convert.ToSingle(array[0], invariantCulture), Convert.ToSingle(array[1], invariantCulture));
		}

		public override string ToString()
		{
			return min + "~" + max;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(min.GetHashCode(), max);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is FloatRange))
			{
				return false;
			}
			return Equals((FloatRange)obj);
		}

		public bool Equals(FloatRange other)
		{
			if (other.min == min)
			{
				return other.max == max;
			}
			return false;
		}
	}
}
