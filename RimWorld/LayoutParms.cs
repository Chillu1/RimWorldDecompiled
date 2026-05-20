using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutParms
{
	public LayoutDef def;

	public float weight = 1f;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "weight");
	}
}
