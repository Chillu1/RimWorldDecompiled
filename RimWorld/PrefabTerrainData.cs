using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class PrefabTerrainData
{
	public TerrainDef def;

	public float chance = 1f;

	public ColorDef color;

	public List<CellRect> rects;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def");
	}
}
