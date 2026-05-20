using Verse;

namespace RimWorld.Planet;

public abstract class SitePartWorker_DistressCall : SitePartWorker
{
	public override void Init(Site site, SitePart sitePart)
	{
		base.Init(site, sitePart);
		site.customLabel = sitePart.def.label.Formatted(site.Faction.Named("FACTION"));
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		return (string)("Hostiles".Translate() + ": " + "Unknown".Translate().CapitalizeFirst()) + ("\n" + "Contains".Translate() + ": " + "Unknown".Translate().CapitalizeFirst());
	}
}
