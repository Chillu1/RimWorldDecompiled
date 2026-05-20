using System.Xml;
using Verse;

namespace RimWorld;

public class PawnKindDefWeight
{
	public PawnKindDef kindDef;

	public float weight = 1f;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "kindDef", "weight");
	}
}
