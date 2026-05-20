namespace RimWorld.Planet;

public class SitePartWorker_UnknownThreatMarker : SitePartWorker
{
	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		if (site.MainSitePartDef == SitePartDefOf.PreciousLump)
		{
			return null;
		}
		return base.GetPostProcessedThreatLabel(site, sitePart);
	}
}
