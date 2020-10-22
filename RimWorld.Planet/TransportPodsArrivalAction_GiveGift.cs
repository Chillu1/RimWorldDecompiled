using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class TransportPodsArrivalAction_GiveGift : TransportPodsArrivalAction
	{
		private Settlement settlement;

		public TransportPodsArrivalAction_GiveGift()
		{
		}

		public TransportPodsArrivalAction_GiveGift(Settlement settlement)
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
			return CanGiveGiftTo(pods, settlement);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				for (int j = 0; j < pods[i].innerContainer.Count; j++)
				{
					Pawn pawn = pods[i].innerContainer[j] as Pawn;
					if (pawn == null)
					{
						continue;
					}
					if (pawn.RaceProps.Humanlike)
					{
						if (pawn.FactionOrExtraMiniOrHomeFaction == settlement.Faction)
						{
							GenGuest.AddHealthyPrisonerReleasedThoughts(pawn);
						}
						else
						{
							GenGuest.AddPrisonerSoldThoughts(pawn);
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
			}
			FactionGiftUtility.GiveGift(pods, settlement);
		}

		public static FloatMenuAcceptanceReport CanGiveGiftTo(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			foreach (IThingHolder pod in pods)
			{
				ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
				for (int i = 0; i < directlyHeldThings.Count; i++)
				{
					Pawn p;
					if ((p = directlyHeldThings[i] as Pawn) != null && p.IsQuestLodger())
					{
						return false;
					}
				}
			}
			return settlement != null && settlement.Spawned && settlement.Faction != null && settlement.Faction != Faction.OfPlayer && !settlement.Faction.def.permanentEnemy && !settlement.HasMap;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement.Faction == Faction.OfPlayer)
			{
				return Enumerable.Empty<FloatMenuOption>();
			}
			return TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => CanGiveGiftTo(pods, settlement), () => new TransportPodsArrivalAction_GiveGift(settlement), "GiveGiftViaTransportPods".Translate(settlement.Faction.Name, FactionGiftUtility.GetGoodwillChange(pods, settlement).ToStringWithSign()), representative, settlement.Tile, delegate(Action action)
			{
				TradeRequestComp tradeReqComp = settlement.GetComponent<TradeRequestComp>();
				if (tradeReqComp.ActiveRequest && pods.Any((IThingHolder p) => p.GetDirectlyHeldThings().Contains(tradeReqComp.requestThingDef)))
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
}
