using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutScatterTerrainParms
{
	public TerrainDef def;

	public IntRange groupCount = IntRange.Invalid;

	public FloatRange groupsPerHundredCells = new FloatRange(0f, 2f);

	public IntRange itemsPerGroup = new IntRange(2, 4);

	public IntRange groupDistRange = new IntRange(2, 5);

	public int minGroups;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
