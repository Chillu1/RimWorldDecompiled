using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutWallAttatchmentParms
{
	public ThingDef def;

	public ThingDef stuff;

	public IntRange countRange = IntRange.Invalid;

	public FloatRange thingsPer10EdgeCells = new FloatRange(1f, 1f);

	public float spawnChancePerPosition = 1f;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
