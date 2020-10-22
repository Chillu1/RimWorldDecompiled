using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_VisitSite : TransportPodsArrivalAction
	{
		private Site site;

		private PawnsArrivalModeDef arrivalMode;

		public TransportPodsArrivalAction_VisitSite()
		{
		}

		public TransportPodsArrivalAction_VisitSite(Site site, PawnsArrivalModeDef arrivalMode)
		{
			this.site = site;
			this.arrivalMode = arrivalMode;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref site, "site");
			Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (site != null && site.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(pods, site);
		}

		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !site.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			bool num = !site.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, CaravanArrivalAction_VisitSite.MapSize, null);
			if (num)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
			}
			if (site.Faction != null && site.Faction != Faction.OfPlayer)
			{
				site.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile);
			}
			Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
			arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
		}

		public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Site site)
		{
			if (site == null || !site.Spawned)
			{
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyNonDownedColonist(pods))
			{
				return false;
			}
			if (site.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Site site)
		{
			foreach (FloatMenuOption floatMenuOption in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportPodsArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.EdgeDrop), "DropAtEdge".Translate(), representative, site.Tile))
			{
				yield return floatMenuOption;
			}
			foreach (FloatMenuOption floatMenuOption2 in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportPodsArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.CenterDrop), "DropInCenter".Translate(), representative, site.Tile))
			{
				yield return floatMenuOption2;
			}
		}
	}
}
