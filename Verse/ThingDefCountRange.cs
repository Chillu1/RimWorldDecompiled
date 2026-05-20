using System;

namespace Verse;

public struct ThingDefCountRange : IEquatable<ThingDefCountRange>, IExposable
{
	private ThingDef thingDef;

	private IntRange countRange;

	public ThingDef ThingDef => thingDef;

	public IntRange CountRange => countRange;

	public int Min => countRange.min;

	public int Max => countRange.max;

	public int TrueMin => countRange.TrueMin;

	public int TrueMax => countRange.TrueMax;

	public ThingDefCountRange(ThingDef thingDef, int min, int max)
		: this(thingDef, new IntRange(min, max))
	{
	}

	public ThingDefCountRange(ThingDef thingDef, IntRange countRange)
	{
		this.thingDef = thingDef;
		this.countRange = countRange;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Values.Look(ref countRange, "countRange");
	}

	public ThingDefCountRange WithCountRange(IntRange newCountRange)
	{
		return new ThingDefCountRange(thingDef, newCountRange);
	}

	public ThingDefCountRange WithCountRange(int newMin, int newMax)
	{
		return new ThingDefCountRange(thingDef, newMin, newMax);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThingDefCountRange))
		{
			return false;
		}
		return Equals((ThingDefCountRange)obj);
	}

	public bool Equals(ThingDefCountRange other)
	{
		return this == other;
	}

	public static bool operator ==(ThingDefCountRange a, ThingDefCountRange b)
	{
		if (a.thingDef == b.thingDef)
		{
			return a.countRange == b.countRange;
		}
		return false;
	}

	public static bool operator !=(ThingDefCountRange a, ThingDefCountRange b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(countRange.GetHashCode(), thingDef);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "(", null, null, null, null };
		IntRange intRange = countRange;
		obj[1] = intRange.ToString();
		obj[2] = "x ";
		obj[3] = ((thingDef != null) ? thingDef.defName : "null");
		obj[4] = ")";
		return string.Concat(obj);
	}

	public static implicit operator ThingDefCountRange(ThingDefCountRangeClass t)
	{
		return new ThingDefCountRange(t.thingDef, t.countRange);
	}

	public static explicit operator ThingDefCountRange(ThingDefCount t)
	{
		return new ThingDefCountRange(t.ThingDef, t.Count, t.Count);
	}

	public static explicit operator ThingDefCountRange(ThingDefCountClass t)
	{
		return new ThingDefCountRange(t.thingDef, t.count, t.count);
	}
}
