using System.Xml;
using Verse;

namespace RimWorld
{
	public class XenotypeChance
	{
		public XenotypeDef xenotype;

		public float chance;

		public XenotypeChance()
		{
		}

		public XenotypeChance(XenotypeDef xenotype, float chance)
		{
			this.xenotype = xenotype;
			this.chance = chance;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "xenotype", xmlRoot.Name);
			chance = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
	}
}
