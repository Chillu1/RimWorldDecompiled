using System.Xml;
using Verse;

namespace RimWorld;

public class PossessionThingDefCountClass
{
	public ThingDef key;

	public IntRange value = IntRange.One;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "key", xmlRoot.Name);
		if (xmlRoot.HasChildNodes)
		{
			value = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
		}
		else
		{
			value = IntRange.One;
		}
	}
}
