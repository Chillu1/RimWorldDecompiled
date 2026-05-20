using System.Xml;

namespace Verse;

public sealed class ThingDefCountRangeClass : IExposable
{
	public ThingDef thingDef;

	public IntRange countRange;

	public int Min => countRange.min;

	public int Max => countRange.max;

	public int TrueMin => countRange.TrueMin;

	public int TrueMax => countRange.TrueMax;

	public ThingDefCountRangeClass()
	{
	}

	public ThingDefCountRangeClass(ThingDef thingDef, int min, int max)
		: this(thingDef, new IntRange(min, max))
	{
	}

	public ThingDefCountRangeClass(ThingDef thingDef, IntRange countRange)
	{
		this.thingDef = thingDef;
		this.countRange = countRange;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Values.Look(ref countRange, "countRange");
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.ChildNodes.Count != 1)
		{
			Log.Error("Misconfigured ThingDefCountRangeClass: " + xmlRoot.OuterXml);
			return;
		}
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
		countRange = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
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

	public static implicit operator ThingDefCountRangeClass(ThingDefCountRange t)
	{
		return new ThingDefCountRangeClass(t.ThingDef, t.CountRange);
	}

	public static explicit operator ThingDefCountRangeClass(ThingDefCount t)
	{
		return new ThingDefCountRangeClass(t.ThingDef, t.Count, t.Count);
	}

	public static explicit operator ThingDefCountRangeClass(ThingDefCountClass t)
	{
		return new ThingDefCountRangeClass(t.thingDef, t.count, t.count);
	}
}
