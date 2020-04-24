using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_LandInSpecificCell : TransportPodsArrivalAction
	{
		private MapParent mapParent;

		private IntVec3 cell;

		public TransportPodsArrivalAction_LandInSpecificCell()
		{
		}

		public TransportPodsArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell)
		{
			this.mapParent = mapParent;
			this.cell = cell;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_Values.Look(ref cell, "cell");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (mapParent != null && mapParent.Tile != destinationTile)
			{
				return false;
			}
			return CanLandInSpecificCell(pods, mapParent);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
			TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, cell, mapParent.Map);
			Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}

		public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
		{
			if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
			{
				return false;
			}
			if (mapParent.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownDaysLeft().ToString("0.#")));
			}
			return true;
		}
	}
}
