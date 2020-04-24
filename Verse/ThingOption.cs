using System.Xml;

namespace Verse
{
	public sealed class ThingOption
	{
		public ThingDef thingDef;

		public float weight = 1f;

		public ThingOption()
		{
		}

		public ThingOption(ThingDef thingDef, float weight)
		{
			this.thingDef = thingDef;
			this.weight = weight;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingOption: " + xmlRoot.OuterXml);
				return;
			}
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
			weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}

		public override string ToString()
		{
			return "(" + ((thingDef != null) ? thingDef.defName : "null") + ", weight=" + weight.ToString("0.##") + ")";
		}
	}
}
