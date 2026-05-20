using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientConcretePad : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (parms.rect.HasValue)
		{
			return parms.sketch != null;
		}
		return false;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		Sketch sketch = new Sketch();
		foreach (IntVec3 edgeCell in parms.rect.Value.EdgeCells)
		{
			sketch.AddTerrain(TerrainDefOf.AncientTile, edgeCell);
		}
		foreach (IntVec3 item in parms.rect.Value.ContractedBy(1))
		{
			sketch.AddTerrain(TerrainDefOf.AncientConcrete, item);
		}
		foreach (IntVec3 corner in parms.rect.Value.Corners)
		{
			sketch.AddThing(ThingDefOf.AncientShipBeacon, corner, Rot4.North);
		}
		parms.sketch.Merge(sketch);
	}
}
