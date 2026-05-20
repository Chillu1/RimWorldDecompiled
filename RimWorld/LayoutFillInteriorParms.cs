using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutFillInteriorParms
{
	public ThingDef def;

	public ThingDef stuff;

	public Rot4? fixedRot;

	public bool alignWithRect;

	public int contractedBy = 2;

	public bool snapToGrid;

	public IntRange countRange = IntRange.Invalid;

	public FloatRange thingsPerHundredCells = new FloatRange(2f, 3f);

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
