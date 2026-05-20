using System;
using RimWorld;

namespace Verse;

public struct ThingDefCount : IEquatable<ThingDefCount>, IExposable
{
	private ThingDef thingDef;

	private int count;

	public ThingDef ThingDef => thingDef;

	public int Count => count;

	public string Label => GenLabel.ThingLabel(thingDef, null, count);

	public string LabelCap => Label.CapitalizeFirst(thingDef);

	public ThingDefCount(ThingDef thingDef, int count)
	{
		if (count < 0)
		{
			Log.Warning(string.Format("Tried to set {0} count to {1}. thingDef={2}", "ThingDefCount", count, thingDef));
			count = 0;
		}
		this.thingDef = thingDef;
		this.count = count;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Values.Look(ref count, "count", 1);
	}

	public ThingDefCount WithCount(int newCount)
	{
		return new ThingDefCount(thingDef, newCount);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThingDefCount))
		{
			return false;
		}
		return Equals((ThingDefCount)obj);
	}

	public bool Equals(ThingDefCount other)
	{
		return this == other;
	}

	public static bool operator ==(ThingDefCount a, ThingDefCount b)
	{
		if (a.thingDef == b.thingDef)
		{
			return a.count == b.count;
		}
		return false;
	}

	public static bool operator !=(ThingDefCount a, ThingDefCount b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(count, thingDef);
	}

	public override string ToString()
	{
		return string.Format("({0}x {1})", count, (thingDef != null) ? thingDef.defName : "null");
	}

	public static implicit operator ThingDefCount(ThingDefCountClass t)
	{
		if (t == null)
		{
			return new ThingDefCount(null, 0);
		}
		return new ThingDefCount(t.thingDef, t.count);
	}
}
