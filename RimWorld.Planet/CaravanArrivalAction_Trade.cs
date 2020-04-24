using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_Trade : CaravanArrivalAction
	{
		private Settlement settlement;

		public override string Label => "TradeWithSettlement".Translate(settlement.Label);

		public override string ReportString => "CaravanTrading".Translate(settlement.Label);

		public CaravanArrivalAction_Trade()
		{
		}

		public CaravanArrivalAction_Trade(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
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
			return CanTradeWith(caravan, settlement);
		}

		public override void Arrived(Caravan caravan)
		{
			CameraJumper.TryJumpAndSelect(caravan);
			Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, settlement.Faction, settlement.TraderKind);
			Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
		}

		public static FloatMenuAcceptanceReport CanTradeWith(Caravan caravan, Settlement settlement)
		{
			return settlement != null && settlement.Spawned && !settlement.HasMap && settlement.Faction != null && settlement.Faction != Faction.OfPlayer && !settlement.Faction.def.permanentEnemy && !settlement.Faction.HostileTo(Faction.OfPlayer) && settlement.CanTradeNow && HasNegotiator(caravan, settlement);
		}

		private static bool HasNegotiator(Caravan caravan, Settlement settlement)
		{
			Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan, settlement.Faction, settlement.TraderKind);
			if (pawn != null)
			{
				return !pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
			}
			return false;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanTradeWith(caravan, settlement), () => new CaravanArrivalAction_Trade(settlement), "TradeWith".Translate(settlement.Label), caravan, settlement.Tile, settlement);
		}
	}
}
