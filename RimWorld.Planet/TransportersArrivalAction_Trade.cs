using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_Trade : TransportersArrivalAction_VisitSettlement
{
	public TransportersArrivalAction_Trade()
	{
	}

	public TransportersArrivalAction_Trade(Settlement settlement, string arrivalMessageKey)
		: base(settlement, arrivalMessageKey)
	{
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		return CanTradeWith(pods, settlement);
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		Pawn pawn = null;
		for (int i = 0; i < transporters.Count; i++)
		{
			if (pawn != null)
			{
				break;
			}
			foreach (Thing item in (IEnumerable<Thing>)transporters[i].GetDirectlyHeldThings())
			{
				if (item is Pawn pawn2)
				{
					pawn = pawn2;
					break;
				}
			}
		}
		base.Arrived(transporters, tile);
		if (pawn != null)
		{
			Caravan caravan = pawn.GetCaravan();
			if (caravan != null && CaravanArrivalAction_Trade.HasNegotiator(caravan, settlement))
			{
				CameraJumper.TryJumpAndSelect(caravan);
				Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, settlement.Faction, settlement.TraderKind);
				Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement));
			}
		}
	}

	public static FloatMenuAcceptanceReport CanTradeWith(IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		if (!TransportersArrivalAction_VisitSettlement.CanVisit(pods, settlement))
		{
			return false;
		}
		if (settlement.Faction == null || settlement.Faction == Faction.OfPlayer)
		{
			return false;
		}
		bool flag = false;
		foreach (IThingHolder pod in pods)
		{
			ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
			if (pod is CompTransporter compTransporter && CaravanShuttleUtility.IsCaravanShuttle(compTransporter))
			{
				directlyHeldThings = compTransporter.parent.GetCaravan().GetDirectlyHeldThings();
			}
			foreach (Thing item in (IEnumerable<Thing>)directlyHeldThings)
			{
				if (item is Pawn pawn && pawn.RaceProps.Humanlike && pawn.CanTradeWith(settlement.Faction, settlement.TraderKind).Accepted)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		return flag && !settlement.HasMap && !settlement.Faction.def.permanentEnemy && !settlement.Faction.HostileTo(Faction.OfPlayer) && settlement.CanTradeNow;
	}
}
