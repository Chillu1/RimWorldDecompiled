using System.Xml;
using Verse;

namespace RimWorld;

public class PawnGenOption
{
	public PawnKindDef kind;

	public float selectionWeight;

	public float Cost => kind.combatPower;

	public override string ToString()
	{
		return string.Format("({0} w={1:F2} c={2})", (kind != null) ? kind.ToString() : "null", selectionWeight, (kind != null) ? Cost.ToString("F2") : "null");
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name);
		selectionWeight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}
}
