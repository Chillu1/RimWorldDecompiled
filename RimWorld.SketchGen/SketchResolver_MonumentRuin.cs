using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_MonumentRuin : SketchResolver
	{
		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}

		protected override void ResolveInt(ResolveParams parms)
		{
			ResolveParams parms2 = parms;
			parms2.allowWood = (parms.allowWood ?? false);
			if (parms2.allowedMonumentThings == null)
			{
				parms2.allowedMonumentThings = new ThingFilter();
				parms2.allowedMonumentThings.SetAllowAll(null, includeNonStorable: true);
			}
			if (ModsConfig.RoyaltyActive)
			{
				parms2.allowedMonumentThings.SetAllow(ThingDefOf.Drape, allow: false);
			}
			SketchResolverDefOf.Monument.Resolve(parms2);
			SketchResolverDefOf.DamageBuildings.Resolve(parms);
		}
	}
}
