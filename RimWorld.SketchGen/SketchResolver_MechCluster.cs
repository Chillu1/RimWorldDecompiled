namespace RimWorld.SketchGen;

public class SketchResolver_MechCluster : SketchResolver
{
	protected override void ResolveInt(SketchResolveParams parms)
	{
		MechClusterGenerator.ResolveSketch(parms);
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}
}
