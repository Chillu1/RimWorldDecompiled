using System;
using Verse;

namespace RimWorld;

public struct ThingStuffPairWithQuality : IEquatable<ThingStuffPairWithQuality>, IExposable
{
	public ThingDef thing;

	public ThingDef stuff;

	public QualityCategory? quality;

	public QualityCategory Quality => quality ?? QualityCategory.Normal;

	public ThingStuffPairWithQuality(ThingDef thing, ThingDef stuff, QualityCategory quality)
	{
		this.thing = thing;
		this.stuff = stuff;
		this.quality = quality;
		if (quality != QualityCategory.Normal && !thing.HasComp(typeof(CompQuality)))
		{
			Log.Warning("Created ThingStuffPairWithQuality with quality" + quality.ToString() + " but " + thing?.ToString() + " doesn't have CompQuality.");
			quality = QualityCategory.Normal;
		}
		if (stuff != null && !thing.MadeFromStuff)
		{
			Log.Warning("Created ThingStuffPairWithQuality with stuff " + stuff?.ToString() + " but " + thing?.ToString() + " is not made from stuff.");
			stuff = null;
		}
	}

	public float GetStatValue(StatDef stat)
	{
		return stat.Worker.GetValue(StatRequest.For(thing, stuff, Quality));
	}

	public static bool operator ==(ThingStuffPairWithQuality a, ThingStuffPairWithQuality b)
	{
		if (a.thing == b.thing && a.stuff == b.stuff)
		{
			return a.quality == b.quality;
		}
		return false;
	}

	public static bool operator !=(ThingStuffPairWithQuality a, ThingStuffPairWithQuality b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ThingStuffPairWithQuality))
		{
			return false;
		}
		return Equals((ThingStuffPairWithQuality)obj);
	}

	public bool Equals(ThingStuffPairWithQuality other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(0, thing), stuff), quality);
	}

	public static explicit operator ThingStuffPairWithQuality(ThingStuffPair p)
	{
		return new ThingStuffPairWithQuality(p.thing, p.stuff, QualityCategory.Normal);
	}

	public Thing MakeThing(bool forceQuality = false)
	{
		Thing result = ThingMaker.MakeThing(thing, stuff);
		if (!forceQuality && result.HasComp<CompUniqueWeapon>())
		{
			return result;
		}
		result.TryGetComp<CompQuality>()?.SetQuality(Quality, ArtGenerationContext.Outsider);
		return result;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thing, "thing");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref quality, "quality");
	}
}
