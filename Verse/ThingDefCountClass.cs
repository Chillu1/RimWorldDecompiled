using System.Xml;
using RimWorld;

namespace Verse
{
	public sealed class ThingDefCountClass : IExposable
	{
		public ThingDef thingDef;

		public int count;

		public string Label => GenLabel.ThingLabel(thingDef, null, count);

		public string LabelCap => Label.CapitalizeFirst(thingDef);

		public string Summary => count + "x " + ((thingDef != null) ? thingDef.label : "null");

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
			Scribe_Values.Look(ref count, "count", 1);
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingDefCountClass: " + xmlRoot.OuterXml);
				return;
			}
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
			count = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}

		public override string ToString()
		{
			return "(" + count + "x " + ((thingDef != null) ? thingDef.defName : "null") + ")";
		}

		public override int GetHashCode()
		{
			return thingDef.shortHash + count << 16;
		}

		public static implicit operator ThingDefCountClass(ThingDefCount t)
		{
			return new ThingDefCountClass(t.ThingDef, t.Count);
		}
	}
}
