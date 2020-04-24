using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanArrivalAction_VisitPeaceTalks : CaravanArrivalAction
	{
		private PeaceTalks peaceTalks;

		public override string Label => "VisitPeaceTalks".Translate(peaceTalks.Label);

		public override string ReportString => "CaravanVisiting".Translate(peaceTalks.Label);

		public CaravanArrivalAction_VisitPeaceTalks()
		{
		}

		public CaravanArrivalAction_VisitPeaceTalks(PeaceTalks peaceTalks)
		{
			this.peaceTalks = peaceTalks;
		}

		public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (peaceTalks != null && peaceTalks.Tile != destinationTile)
			{
				return false;
			}
			return CanVisit(caravan, peaceTalks);
		}

		public override void Arrived(Caravan caravan)
		{
			peaceTalks.Notify_CaravanArrived(caravan);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref peaceTalks, "peaceTalks");
		}

		public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, PeaceTalks peaceTalks)
		{
			return peaceTalks?.Spawned ?? false;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, PeaceTalks peaceTalks)
		{
			return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, peaceTalks), () => new CaravanArrivalAction_VisitPeaceTalks(peaceTalks), "VisitPeaceTalks".Translate(peaceTalks.Label), caravan, peaceTalks.Tile, peaceTalks);
		}
	}
}
