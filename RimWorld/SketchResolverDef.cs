using RimWorld.SketchGen;
using Verse;

namespace RimWorld
{
	public class SketchResolverDef : Def
	{
		public SketchResolver resolver;

		public bool isRoot;

		public void Resolve(ResolveParams parms)
		{
			resolver.Resolve(parms);
		}

		public bool CanResolve(ResolveParams parms)
		{
			return resolver.CanResolve(parms);
		}
	}
}
