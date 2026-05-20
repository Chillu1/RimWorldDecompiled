using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_VisitSite : TransportersArrivalAction
{
	private Site site;

	private PawnsArrivalModeDef arrivalMode;

	public override bool GeneratesMap => true;

	public TransportersArrivalAction_VisitSite()
	{
	}

	public TransportersArrivalAction_VisitSite(Site site, PawnsArrivalModeDef arrivalMode)
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

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
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

	public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
	{
		return !site.HasMap;
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);
		bool num = !site.HasMap;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, site.PreferredMapSize, null);
		if (num)
		{
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
		}
		if (site.Faction != null && site.Faction != Faction.OfPlayer && site.MainSitePartDef.considerEnteringAsAttack)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(site.Faction, Faction.OfPlayer.GoodwillToMakeHostile(site.Faction), canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.AttackedSettlement);
		}
		if (transporters.IsShuttle())
		{
			Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}
		else
		{
			Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}
		arrivalMode.Worker.TravellingTransportersArrived(transporters, orGenerateMap);
	}

	public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Site site)
	{
		if (site == null || !site.Spawned)
		{
			return false;
		}
		if (!TransportersArrivalActionUtility.AnyNonDownedColonist(pods))
		{
			return false;
		}
		if (site.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Site site)
	{
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportersArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.EdgeDrop), "DropAtEdge".Translate(), launchAction, site.Tile, UIConfirmationCallback))
		{
			yield return floatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportersArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.CenterDrop), "DropInCenter".Translate(), launchAction, site.Tile, UIConfirmationCallback))
		{
			yield return floatMenuOption2;
		}
		void UIConfirmationCallback(Action action)
		{
			if (ModsConfig.OdysseyActive && site.Tile.LayerDef == PlanetLayerDefOf.Orbit)
			{
				TaggedString text = "OrbitalWarning".Translate();
				text += string.Format("\n\n{0}", "LaunchToConfirmation".Translate());
				Find.WindowStack.Add(new Dialog_MessageBox(text, null, action, "Cancel".Translate(), delegate
				{
				}, null, buttonADestructive: true));
			}
			else
			{
				action();
			}
		}
	}
}
