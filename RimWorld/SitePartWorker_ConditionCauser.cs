using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_ConditionCauser : SitePartWorker
	{
		public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
		{
			int worldRange = sitePart.def.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange;
			return base.GetPostProcessedThreatLabel(site, sitePart) + " (" + "ConditionCauserRadius".Translate(worldRange) + ")";
		}

		public override void Init(Site site, SitePart sitePart)
		{
			sitePart.conditionCauser = ThingMaker.MakeThing(sitePart.def.conditionCauserDef);
			CompCauseGameCondition compCauseGameCondition = sitePart.conditionCauser.TryGetComp<CompCauseGameCondition>();
			compCauseGameCondition.RandomizeSettings();
			compCauseGameCondition.LinkWithSite(sitePart.site);
		}

		public override void SitePartWorkerTick(SitePart sitePart)
		{
			if (!sitePart.conditionCauser.DestroyedOrNull() && !sitePart.conditionCauser.Spawned)
			{
				sitePart.conditionCauser.Tick();
			}
		}

		public override void PostDrawExtraSelectionOverlays(SitePart sitePart)
		{
			base.PostDrawExtraSelectionOverlays(sitePart);
			GenDraw.DrawWorldRadiusRing(sitePart.site.Tile, sitePart.def.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange);
		}

		public override void Notify_SiteMapAboutToBeRemoved(SitePart sitePart)
		{
			base.Notify_SiteMapAboutToBeRemoved(sitePart);
			if (!sitePart.conditionCauser.DestroyedOrNull() && sitePart.conditionCauser.Spawned && sitePart.conditionCauser.Map == sitePart.site.Map)
			{
				sitePart.conditionCauser.DeSpawn();
				sitePart.conditionCauserWasSpawned = false;
			}
		}
	}
}
