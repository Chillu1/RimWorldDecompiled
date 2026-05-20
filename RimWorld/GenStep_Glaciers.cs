using Verse;

namespace RimWorld;

public class GenStep_Glaciers : GenStep_ScatterLumpsMineable
{
	public override void Generate(Map map, GenStepParams parms)
	{
		forcedDefToScatter = ThingDefOf.SolidIce;
		base.Generate(map, parms);
	}
}
