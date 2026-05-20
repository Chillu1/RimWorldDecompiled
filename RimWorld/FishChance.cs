using System.Xml;
using Verse;

namespace RimWorld;

public class FishChance
{
	public ThingDef fishDef;

	public float chance = 1f;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "fishDef", xmlRoot);
		chance = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}
}
