namespace RimWorld.SketchGen
{
	public class SketchResolver_MechCluster : SketchResolver
	{
		protected override void ResolveInt(ResolveParams parms)
		{
			MechClusterGenerator.ResolveSketch(parms);
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}
	}
}
