using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_LandInSpecificCell : TransportPodsArrivalAction
	{
		private MapParent mapParent;

		private IntVec3 cell;

		private bool landInShuttle;

		public TransportPodsArrivalAction_LandInSpecificCell()
		{
		}

		public TransportPodsArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell)
		{
			this.mapParent = mapParent;
			this.cell = cell;
		}

		public TransportPodsArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell, bool landInShuttle)
		{
			this.mapParent = mapParent;
			this.cell = cell;
			this.landInShuttle = landInShuttle;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_Values.Look(ref cell, "cell");
			Scribe_Values.Look(ref landInShuttle, "landInShuttle", defaultValue: false);
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
			if (landInShuttle)
			{
				TransportPodsArrivalActionUtility.DropShuttle_NewTemp(pods, mapParent.Map, cell);
				Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
			}
			else
			{
				TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, cell, mapParent.Map);
				Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
			}
		}

		public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
		{
			if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
			{
				return false;
			}
			if (mapParent.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
			}
			return true;
		}
	}
}
