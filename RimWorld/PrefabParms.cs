using System.Xml;
using Verse;

namespace RimWorld;

public class PrefabParms
{
	public PrefabDef def;

	public float weight = 1f;

	public IntRange minMaxRange = IntRange.Invalid;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "weight");
	}
}
