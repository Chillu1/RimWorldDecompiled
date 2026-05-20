using Verse;

namespace RimWorld.Planet;

public class PocketMapParent : MapParent
{
	public Map sourceMap;

	public MapGeneratorDef mapGenerator;

	public override MapGeneratorDef MapGeneratorDef => mapGenerator;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref sourceMap, "sourceMap");
		Scribe_Defs.Look(ref mapGenerator, "mapGenerator");
	}
}
