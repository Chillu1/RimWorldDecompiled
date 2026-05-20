using RimWorld.Planet;

namespace RimWorld
{
	public class SitePartWorker_AncientComplex : SitePartWorker
	{
		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			if (site.MainSitePartDef == def)
			{
				return null;
			}
			return base.GetPostProcessedThreatLabel(site, sitePart);
		}
	}
}
