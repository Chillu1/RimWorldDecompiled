using System.Xml;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class ThingDefCountClass : IExposable
{
	public ThingDef thingDef;

	public int count = 1;

	public Color? color;

	public float? chance;

	public ThingDef stuff;

	public QualityCategory quality = QualityCategory.Normal;

	public string Label => GenLabel.ThingLabel(thingDef, null, count);

	public string LabelCap => Label.CapitalizeFirst(thingDef);

	public string Summary => count + "x " + ((thingDef != null) ? thingDef.label : "null");

	public float DropChance => chance ?? 1f;

	public bool IsChanceBased => chance.HasValue;

	public ThingDefCountClass()
	{
	}

	public ThingDefCountClass(ThingDef thingDef, int count)
	{
		if (count < 0)
		{
			Log.Warning("Tried to set ThingDefCountClass count to " + count + ". thingDef=" + thingDef);
			count = 0;
		}
		this.thingDef = thingDef;
		this.count = count;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref count, "count", 1);
		Scribe_Values.Look(ref quality, "quality", QualityCategory.Awful);
		Scribe_Values.Look(ref color, "color");
		Scribe_Values.Look(ref chance, "chance");
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "thingDef", "count");
	}

	public override string ToString()
	{
		return string.Format("({0}x {1})", count, (thingDef != null) ? thingDef.defName : "null");
	}

	public override int GetHashCode()
	{
		return thingDef.shortHash + count << 16;
	}

	public IngredientCount ToIngredientCount()
	{
		IngredientCount ingredientCount = new IngredientCount();
		ingredientCount.SetBaseCount(count);
		ingredientCount.filter.SetAllow(thingDef, allow: true);
		return ingredientCount;
	}

	public static implicit operator ThingDefCountClass(ThingDefCount t)
	{
		return new ThingDefCountClass(t.ThingDef, t.Count);
	}
}
