using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientBandNodeRoom : SketchResolver
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
		if (ModLister.CheckBiotech("Ancient band node room"))
		{
			SketchResolveParams parms2 = parms;
			parms2.wallEdgeThing = ThingDefOf.AncientBandNode;
			SketchResolverDefOf.AddWallEdgeThings.Resolve(parms2);
		}
	}
}
