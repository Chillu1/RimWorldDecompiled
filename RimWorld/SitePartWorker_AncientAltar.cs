using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_AncientAltar : SitePartWorker
	{
		public override void Init(Site site, SitePart sitePart)
		{
			base.Init(site, sitePart);
			sitePart.relicThing = sitePart.parms.relicThing;
		}

		public bool ShouldKeepMapForRelic(SitePart sitePart)
		{
			if (sitePart.relicThing != null && !sitePart.relicThing.DestroyedOrNull())
			{
				return sitePart.relicThing.MapHeld == sitePart.site.Map;
			}
			return false;
		}

		public override void Notify_SiteMapAboutToBeRemoved(SitePart sitePart)
		{
			base.Notify_SiteMapAboutToBeRemoved(sitePart);
			if (ShouldKeepMapForRelic(sitePart))
			{
				sitePart.relicThing.DeSpawnOrDeselect();
				if (sitePart.relicThing.holdingOwner != null)
				{
					sitePart.relicThing.holdingOwner.Remove(sitePart.relicThing);
				}
				sitePart.relicWasSpawned = false;
			}
			else if (!sitePart.parms.relicLostSignal.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(sitePart.parms.relicLostSignal));
			}
		}

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
