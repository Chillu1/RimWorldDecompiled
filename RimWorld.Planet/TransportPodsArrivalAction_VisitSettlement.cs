using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_VisitSettlement : TransportPodsArrivalAction_FormCaravan
	{
		private Settlement settlement;

		public TransportPodsArrivalAction_VisitSettlement()
		{
		}

		public TransportPodsArrivalAction_VisitSettlement(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref settlement, "settlement");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
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
			return CanVisit(pods, settlement);
		}

		public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Visitable)
			{
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			return TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, settlement), () => new TransportPodsArrivalAction_VisitSettlement(settlement), "VisitSettlement".Translate(settlement.Label), representative, settlement.Tile);
		}
	}
}
