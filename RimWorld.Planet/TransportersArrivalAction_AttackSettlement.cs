using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_AttackSettlement : TransportersArrivalAction
{
	private Settlement settlement;

	private PawnsArrivalModeDef arrivalMode;

	public override bool GeneratesMap => true;

	public TransportersArrivalAction_AttackSettlement()
	{
	}

	public TransportersArrivalAction_AttackSettlement(Settlement settlement, PawnsArrivalModeDef arrivalMode)
	{
		this.settlement = settlement;
		this.arrivalMode = arrivalMode;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref settlement, "settlement");
		Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (settlement != null && settlement.Tile != destinationTile)
		{
			return false;
		}
		return CanAttack(pods, settlement);
	}

	public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
	{
		return !settlement.HasMap;
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);
		bool num = !settlement.HasMap;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
		TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
		TaggedString letterText = "LetterTransportPodsLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
		SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
		if (num)
		{
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
		}
		Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget, settlement.Faction);
		arrivalMode.Worker.TravellingTransportersArrived(transporters, orGenerateMap);
	}

	public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		if (settlement == null || !settlement.Spawned || !settlement.Attackable)
		{
			return false;
		}
		if (!TransportersArrivalActionUtility.AnyNonDownedColonist(pods))
		{
			return false;
		}
		if (settlement.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailReasonAndMessage("EnterCooldownBlocksEntering".Translate(), "MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportersArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop), "AttackAndDropAtEdge".Translate(settlement.Label), launchAction, settlement.Tile))
		{
			yield return floatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportersArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop), "AttackAndDropInCenter".Translate(settlement.Label), launchAction, settlement.Tile))
		{
			yield return floatMenuOption2;
		}
	}
}
