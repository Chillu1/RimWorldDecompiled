using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_OfferGifts : CaravanArrivalAction
	{
		private Settlement settlement;

		public override string Label => "OfferGifts".Translate();

		public override string ReportString => "CaravanOfferingGifts".Translate(settlement.Label);

		public CaravanArrivalAction_OfferGifts()
		{
		}

		public CaravanArrivalAction_OfferGifts(Settlement settlement)
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
			return CanOfferGiftsTo(caravan, settlement);
		}

		public override void Arrived(Caravan caravan)
		{
			CameraJumper.TryJumpAndSelect(caravan);
			Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
			Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement, giftsOnly: true));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
		}

		public static FloatMenuAcceptanceReport CanOfferGiftsTo(Caravan caravan, Settlement settlement)
		{
			return settlement != null && settlement.Spawned && !settlement.HasMap && settlement.Faction != null && settlement.Faction != Faction.OfPlayer && !settlement.Faction.def.permanentEnemy && settlement.Faction.HostileTo(Faction.OfPlayer) && settlement.CanTradeNow && HasNegotiator(caravan);
		}

		private static bool HasNegotiator(Caravan caravan)
		{
			Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan);
			if (pawn != null)
			{
				return !pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
			}
			return false;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanOfferGiftsTo(caravan, settlement), () => new CaravanArrivalAction_OfferGifts(settlement), "OfferGifts".Translate(), caravan, settlement.Tile, settlement);
		}
	}
}
