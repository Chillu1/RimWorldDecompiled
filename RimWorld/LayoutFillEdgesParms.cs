using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutFillEdgesParms
{
	public ThingDef def;

	public ThingDef stuff;

	public int padding;

	public int contractedBy = 1;

	public IntRange countRange = IntRange.Invalid;

	public FloatRange groupsPerTenEdgeCells = new FloatRange(1f, 1f);

	public IntRange groupCountRange = new IntRange(1, 2);

	public RotationDirection rotOffset = RotationDirection.Opposite;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
