using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientHatch : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckOdyssey("Ancient hatch"))
		{
			return;
		}
		Sketch sketch = new Sketch();
		CellRect cellRect = new CellRect(0, 0, 7, 7);
		foreach (IntVec3 item in cellRect)
		{
			sketch.AddTerrain(TerrainDefOf.AncientConcrete, item);
		}
		foreach (IntVec3 item2 in cellRect.ContractedBy(1))
		{
			sketch.AddTerrain(TerrainDefOf.AncientTile, item2);
		}
		foreach (IntVec3 corner in cellRect.ContractedBy(1).Corners)
		{
			sketch.AddThing(ThingDefOf.AncientSecurityTurret, corner, Rot4.North);
		}
		sketch.AddThing(ThingDefOf.AncientHatch, cellRect.CenterCell, Rot4.North);
		parms.sketch.Merge(sketch);
	}
}
