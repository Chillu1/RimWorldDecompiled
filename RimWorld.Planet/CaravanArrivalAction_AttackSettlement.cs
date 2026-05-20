using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class CaravanArrivalAction_AttackSettlement : CaravanArrivalAction
{
	private Settlement settlement;

	public override string Label => "AttackSettlement".Translate(settlement.Label);

	public override string ReportString => "CaravanAttacking".Translate(settlement.Label);

	public CaravanArrivalAction_AttackSettlement()
	{
	}

	public CaravanArrivalAction_AttackSettlement(Settlement settlement)
	{
		this.settlement = settlement;
	}

	public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (settlement != null && settlement.Tile != destinationTile)
		{
			return false;
		}
		return CanAttack(caravan, settlement);
	}

	public override void Arrived(Caravan caravan)
	{
		SettlementUtility.Attack(caravan, settlement);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref settlement, "settlement");
	}

	public static FloatMenuAcceptanceReport CanAttack(Caravan caravan, Settlement settlement)
	{
		if (settlement == null || !settlement.Spawned || !settlement.Attackable)
		{
			return false;
		}
		if (settlement.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement)
	{
		return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(caravan, settlement), () => new CaravanArrivalAction_AttackSettlement(settlement), "AttackSettlement".Translate(settlement.Label), caravan, settlement.Tile, settlement, settlement.Faction.AllyOrNeutralTo(Faction.OfPlayer) ? ((Action<Action>)delegate(Action action)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmAttackFriendlyFaction".Translate(settlement.LabelCap, settlement.Faction.Name), delegate
			{
				action();
			}));
		}) : null);
	}
}
