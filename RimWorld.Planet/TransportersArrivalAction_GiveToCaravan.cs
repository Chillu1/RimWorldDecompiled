using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_GiveToCaravan : TransportersArrivalAction
{
	private Caravan caravan;

	private static readonly List<Thing> tmpContainedThings = new List<Thing>();

	public override bool GeneratesMap => false;

	public TransportersArrivalAction_GiveToCaravan()
	{
	}

	public TransportersArrivalAction_GiveToCaravan(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref caravan, "caravan");
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
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

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		for (int i = 0; i < transporters.Count; i++)
		{
			tmpContainedThings.Clear();
			tmpContainedThings.AddRange(transporters[i].innerContainer);
			for (int j = 0; j < tmpContainedThings.Count; j++)
			{
				transporters[i].innerContainer.Remove(tmpContainedThings[j]);
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

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Caravan caravan)
	{
		return TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanGiveTo(pods, caravan), () => new TransportersArrivalAction_GiveToCaravan(caravan), "GiveToCaravan".Translate(caravan.Label), launchAction, caravan.Tile);
	}
}
