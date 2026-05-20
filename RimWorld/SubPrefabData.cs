using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class SubPrefabData
{
	public PrefabDef def;

	public float chance = 1f;

	public IntVec3 position;

	public List<IntVec3> positions;

	public RotationDirection relativeRotation;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "position");
	}
}
