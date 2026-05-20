using System.Xml;
using Verse;

namespace RimWorld;

public class PawnKindDefCount
{
	public PawnKindDef kindDef;

	public int count;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "kindDef", "count");
	}
}
