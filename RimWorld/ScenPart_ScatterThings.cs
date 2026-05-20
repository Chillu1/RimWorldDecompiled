using Verse;

namespace RimWorld;

public abstract class ScenPart_ScatterThings : ScenPart_ThingCount
{
	public bool allowRoofed = true;

	protected abstract bool NearPlayerStart { get; }

	public override void GenerateIntoMap(Map map)
	{
		if (Find.GameInitData != null)
		{
			GenStep_ScatterThings genStep_ScatterThings = new GenStep_ScatterThings();
			genStep_ScatterThings.nearPlayerStart = NearPlayerStart;
			genStep_ScatterThings.allowFoggedPositions = !NearPlayerStart;
			genStep_ScatterThings.thingDef = thingDef;
			genStep_ScatterThings.stuff = stuff;
			genStep_ScatterThings.count = count;
			genStep_ScatterThings.spotMustBeStandable = true;
			genStep_ScatterThings.minSpacing = 5f;
			genStep_ScatterThings.quality = quality;
			genStep_ScatterThings.clusterSize = ((thingDef.category == ThingCategory.Building) ? 1 : 4);
			genStep_ScatterThings.allowRoofed = allowRoofed;
			genStep_ScatterThings.minify = true;
			genStep_ScatterThings.Generate(map, default(GenStepParams));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref allowRoofed, "allowRoofed", defaultValue: false);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ (allowRoofed ? 1 : 0);
	}
}
