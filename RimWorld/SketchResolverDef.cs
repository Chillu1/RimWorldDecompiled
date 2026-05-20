using RimWorld.SketchGen;
using Verse;

namespace RimWorld;

public class SketchResolverDef : Def
{
	public SketchResolver resolver;

	public bool isRoot;

	public void Resolve(SketchResolveParams parms)
	{
		resolver.Resolve(parms);
	}

	public bool CanResolve(SketchResolveParams parms)
	{
		return resolver.CanResolve(parms);
	}
}
