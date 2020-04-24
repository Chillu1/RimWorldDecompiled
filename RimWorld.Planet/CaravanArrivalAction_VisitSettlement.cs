using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitSettlement : CaravanArrivalAction
	{
		private Settlement settlement;

		public override string Label => "VisitSettlement".Translate(settlement.Label);

		public override string ReportString => "CaravanVisiting".Translate(settlement.Label);

		public CaravanArrivalAction_VisitSettlement()
		{
		}

		public CaravanArrivalAction_VisitSettlement(Settlement settlement)
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
			return CanVisit(caravan, settlement);
		}

		public override void Arrived(Caravan caravan)
		{
			if (caravan.IsPlayerControlled)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(settlement), "LetterCaravanEnteredMap".Translate(caravan.Label, settlement).CapitalizeFirst(), LetterDefOf.NeutralEvent, caravan);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, Settlement settlement)
		{
			return settlement != null && settlement.Spawned && settlement.Visitable;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, settlement), () => new CaravanArrivalAction_VisitSettlement(settlement), "VisitSettlement".Translate(settlement.Label), caravan, settlement.Tile, settlement);
		}
	}
}
