using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_GiveGift : TransportersArrivalAction
{
	private Settlement settlement;

	public override bool GeneratesMap => false;

	public TransportersArrivalAction_GiveGift()
	{
	}

	public TransportersArrivalAction_GiveGift(Settlement settlement)
	{
		this.settlement = settlement;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref settlement, "settlement");
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
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
		return CanGiveGiftTo(pods, settlement);
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		for (int i = 0; i < transporters.Count; i++)
		{
			for (int j = 0; j < transporters[i].innerContainer.Count; j++)
			{
				if (transporters[i].innerContainer[j] is Pawn pawn)
				{
					if (pawn.RaceProps.Humanlike)
					{
						Pawn result;
						if (pawn.HomeFaction == settlement.Faction)
						{
							GenGuest.AddHealthyPrisonerReleasedThoughts(pawn);
						}
						else if (PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.TryRandomElement(out result))
						{
							Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SoldSlave, result.Named(HistoryEventArgsNames.Doer)));
						}
					}
					else if (pawn.RaceProps.Animal && pawn.relations != null)
					{
						Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond);
						if (firstDirectRelationPawn != null && firstDirectRelationPawn.needs.mood != null)
						{
							pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Bond, firstDirectRelationPawn);
							firstDirectRelationPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SoldMyBondedAnimalMood);
						}
					}
				}
				transporters[i].innerContainer[j].Notify_AbandonedAtTile(tile);
			}
		}
		FactionGiftUtility.GiveGift(transporters, settlement);
	}

	public static FloatMenuAcceptanceReport CanGiveGiftTo(IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		foreach (IThingHolder pod in pods)
		{
			ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
			for (int i = 0; i < directlyHeldThings.Count; i++)
			{
				if (directlyHeldThings[i] is Pawn p && p.IsQuestLodger())
				{
					return false;
				}
			}
		}
		return settlement != null && settlement.Spawned && settlement.Faction != null && settlement.Faction != Faction.OfPlayer && !settlement.Faction.def.permanentEnemy && !settlement.HasMap;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		if (settlement.Faction == Faction.OfPlayer)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}
		return TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanGiveGiftTo(pods, settlement), () => new TransportersArrivalAction_GiveGift(settlement), "GiveGiftViaTransportPods".Translate(settlement.Faction.Name, FactionGiftUtility.GetGoodwillChange(pods, settlement).ToStringWithSign()), launchAction, settlement.Tile, delegate(Action action)
		{
			TradeRequestComp tradeReqComp = settlement.GetComponent<TradeRequestComp>();
			if (tradeReqComp != null && tradeReqComp.ActiveRequest && pods.Any((IThingHolder p) => p.GetDirectlyHeldThings().Contains(tradeReqComp.requestThingDef)))
			{
				Find.WindowStack.Add(new Dialog_MessageBox("GiveGiftViaTransportPodsTradeRequestWarning".Translate(), "Yes".Translate(), delegate
				{
					action();
				}, "No".Translate()));
			}
			else
			{
				action();
			}
		});
	}
}
