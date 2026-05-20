using System.Xml;
using Verse;

namespace RimWorld;

public class LayoutPrefabParms
{
	public PrefabDef def;

	public IntRange countRange = IntRange.Invalid;

	public IntRange minMaxRange = new IntRange(-1, int.MaxValue);

	public bool ensureNoBlocks = true;

	public FloatRange countPerHundredCells = new FloatRange(1f, 2f);

	public FloatRange countPerTenEdgeCells = new FloatRange(1f, 1f);

	public RotationDirection rotOffset;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
