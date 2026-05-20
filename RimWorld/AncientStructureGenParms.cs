using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimWorld;

public class AncientStructureGenParms
{
	public class ScatteredThing
	{
		public ThingDef thingDef;

		public float chancePer100Cells = 1f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "thingDef", "chancePer100Cells");
		}
	}

	public LayoutDef structureLayoutDef;

	public IntRange structureSizeRange = IntRange.Zero;

	public List<ScatteredPrefabs> scatteredPrefabs = new List<ScatteredPrefabs>();

	public ThingDef perimeterWallDef;

	public int perimeterExpandBy;

	public float perimeterWallChance = 1f;

	public TerrainDef perimeterTerrainDef;

	public List<LayoutScatterTerrainParms> perimeterScatterTerrain = new List<LayoutScatterTerrainParms>();

	public List<ScatteredThing> perimeterScatteredThings = new List<ScatteredThing>();
}
