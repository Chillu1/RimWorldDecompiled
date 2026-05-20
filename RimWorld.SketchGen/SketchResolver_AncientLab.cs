using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientLab : SketchResolver
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
		if (!ModLister.CheckIdeology("Ancient lab"))
		{
			return;
		}
		SketchResolveParams parms2 = parms;
		parms2.thingCentral = ThingDefOf.AncientOperatingTable;
		parms2.requireFloor = true;
		SketchResolverDefOf.AddThingsCentral.Resolve(parms2);
		SketchResolveParams parms3 = parms;
		parms3.wallEdgeThing = ThingDefOf.AncientDisplayBank;
		parms3.requireFloor = true;
		SketchResolverDefOf.AddWallEdgeThings.Resolve(parms3);
		foreach (IntVec3 cell in parms.rect.Value.Cells)
		{
			parms.sketch.AddTerrain(TerrainDefOf.AncientTile, cell);
		}
	}
}
