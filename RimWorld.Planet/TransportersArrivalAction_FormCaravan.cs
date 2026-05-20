using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_FormCaravan : TransportersArrivalAction
{
	private string arrivalMessageKey = "MessageTransportPodsArrived";

	private static readonly List<Pawn> tmpPawns = new List<Pawn>();

	private static readonly List<Thing> tmpContainedThings = new List<Thing>();

	public override bool GeneratesMap => false;

	public TransportersArrivalAction_FormCaravan()
	{
	}

	public TransportersArrivalAction_FormCaravan(string arrivalMessageKey)
	{
		this.arrivalMessageKey = arrivalMessageKey;
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		return CanFormCaravanAt(pods, destinationTile);
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		tmpPawns.Clear();
		for (int i = 0; i < transporters.Count; i++)
		{
			ThingOwner innerContainer = transporters[i].innerContainer;
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				if (innerContainer[num] is Pawn item)
				{
					tmpPawns.Add(item);
					innerContainer.Remove(item);
				}
			}
		}
		if (!GenWorldClosest.TryFindClosestPassableTile(tile, out var foundTile))
		{
			foundTile = tile;
		}
		Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, foundTile, addToWorldPawnsIfNotAlready: true);
		if (transporters.IsShuttle())
		{
			CaravanInventoryUtility.GiveThing(caravan, transporters[0].RemoveShuttle());
		}
		for (int j = 0; j < transporters.Count; j++)
		{
			tmpContainedThings.Clear();
			tmpContainedThings.AddRange(transporters[j].innerContainer);
			for (int k = 0; k < tmpContainedThings.Count; k++)
			{
				transporters[j].innerContainer.Remove(tmpContainedThings[k]);
				CaravanInventoryUtility.GiveThing(caravan, tmpContainedThings[k]);
			}
		}
		tmpPawns.Clear();
		tmpContainedThings.Clear();
		Messages.Message(arrivalMessageKey.Translate(), caravan, MessageTypeDefOf.TaskCompletion);
		Find.WorldObjects.WorldObjectAt<PeaceTalks>(tile)?.Notify_CaravanArrived(caravan);
	}

	public static bool CanFormCaravanAt(IEnumerable<IThingHolder> pods, PlanetTile tile)
	{
		if (TransportersArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer) && !Find.World.Impassable(tile))
		{
			return tile.LayerDef.canFormCaravans;
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref arrivalMessageKey, "arrivalMessageKey", "MessageTransportPodsArrived");
	}
}
