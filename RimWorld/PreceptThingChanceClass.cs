using System.Xml;
using Verse;

namespace RimWorld
{
	public class PreceptThingChanceClass
	{
		public ThingDef def;

		public float chance;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot);
			chance = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
	}
}
