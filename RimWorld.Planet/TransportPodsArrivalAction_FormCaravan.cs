using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_FormCaravan : TransportPodsArrivalAction
	{
		private string arrivalMessageKey = "MessageTransportPodsArrived";

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<Thing> tmpContainedThings = new List<Thing>();

		public TransportPodsArrivalAction_FormCaravan()
		{
		}

		public TransportPodsArrivalAction_FormCaravan(string arrivalMessageKey)
		{
			this.arrivalMessageKey = arrivalMessageKey;
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			return CanFormCaravanAt(pods, destinationTile);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			tmpPawns.Clear();
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner innerContainer = pods[i].innerContainer;
				for (int num = innerContainer.Count - 1; num >= 0; num--)
				{
					Pawn pawn = innerContainer[num] as Pawn;
					if (pawn != null)
					{
						tmpPawns.Add(pawn);
						innerContainer.Remove(pawn);
					}
				}
			}
			if (!GenWorldClosest.TryFindClosestPassableTile(tile, out var foundTile))
			{
				foundTile = tile;
			}
			Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, foundTile, addToWorldPawnsIfNotAlready: true);
			for (int j = 0; j < pods.Count; j++)
			{
				tmpContainedThings.Clear();
				tmpContainedThings.AddRange(pods[j].innerContainer);
				for (int k = 0; k < tmpContainedThings.Count; k++)
				{
					pods[j].innerContainer.Remove(tmpContainedThings[k]);
					CaravanInventoryUtility.GiveThing(caravan, tmpContainedThings[k]);
				}
			}
			tmpPawns.Clear();
			tmpContainedThings.Clear();
			Messages.Message(arrivalMessageKey.Translate(), caravan, MessageTypeDefOf.TaskCompletion);
		}

		public static bool CanFormCaravanAt(IEnumerable<IThingHolder> pods, int tile)
		{
			if (TransportPodsArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer))
			{
				return !Find.World.Impassable(tile);
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref arrivalMessageKey, "arrivalMessageKey", "MessageTransportPodsArrived");
		}
	}
}
