using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_GiveToCaravan : TransportPodsArrivalAction
	{
		private Caravan caravan;

		private static List<Thing> tmpContainedThings = new List<Thing>();

		public TransportPodsArrivalAction_GiveToCaravan()
		{
		}

		public TransportPodsArrivalAction_GiveToCaravan(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref caravan, "caravan");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (caravan != null && !Find.WorldGrid.IsNeighborOrSame(caravan.Tile, destinationTile))
			{
				return false;
			}
			return CanGiveTo(pods, caravan);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				tmpContainedThings.Clear();
				tmpContainedThings.AddRange(pods[i].innerContainer);
				for (int j = 0; j < tmpContainedThings.Count; j++)
				{
					pods[i].innerContainer.Remove(tmpContainedThings[j]);
					caravan.AddPawnOrItem(tmpContainedThings[j], addCarriedPawnToWorldPawnsIfAny: true);
				}
			}
			tmpContainedThings.Clear();
			Messages.Message("MessageTransportPodsArrivedAndAddedToCaravan".Translate(caravan.Name), caravan, MessageTypeDefOf.TaskCompletion);
		}

		public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return caravan != null && caravan.Spawned && caravan.IsPlayerControlled;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanGiveTo(pods, caravan), () => new TransportPodsArrivalAction_GiveToCaravan(caravan), "GiveToCaravan".Translate(caravan.Label), representative, caravan.Tile);
		}
	}
}
