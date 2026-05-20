using Verse;

namespace RimWorld.Planet;

public class SitePartWorker_Asteroid : SitePartWorker
{
	public override void Init(Site site, SitePart sitePart)
	{
		base.Init(site, sitePart);
		site.customLabel = sitePart.def.label.Formatted(NamedArgumentUtility.Named(sitePart.parms.preciousLumpResources, "RESOURCE"));
	}
}
