using System.Xml;
using Verse;

namespace RimWorld;

public class ComplexThreat
{
	public ComplexThreatDef def;

	public float chancePerComplex = 1f;

	public int maxPerComplex = int.MaxValue;

	public int maxPerRoom = 1;

	public float selectionWeight = 1f;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "selectionWeight");
	}
}
