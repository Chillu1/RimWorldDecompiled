using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutPartParms
{
	public RoomPartDef def;

	public float chance = 1f;

	public IntRange threatPointsRange = IntRange.Invalid;

	public IntRange countRange = IntRange.Invalid;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "chance");
	}
}
