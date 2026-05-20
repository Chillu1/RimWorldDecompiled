using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientLandingPad : SketchResolver
{
	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckIdeology("Ancient landing pad"))
		{
			return;
		}
		Sketch sketch = new Sketch();
		IntVec2 intVec = parms.landingPadSize ?? new IntVec2(12, 12);
		CellRect cellRect = new CellRect(0, 0, intVec.x, intVec.z);
		foreach (IntVec3 item in cellRect)
		{
			sketch.AddTerrain(TerrainDefOf.AncientConcrete, item);
		}
		foreach (IntVec3 corner in cellRect.Corners)
		{
			sketch.AddThing(ThingDefOf.AncientShipBeacon, corner, Rot4.North);
		}
		parms.sketch.Merge(sketch);
		SketchResolveParams parms2 = parms;
		parms2.destroyChanceExp = 5f;
		SketchResolverDefOf.DamageBuildings.Resolve(parms2);
	}
}
