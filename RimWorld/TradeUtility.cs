using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class TradeUtility
{
	public const float MinimumBuyPrice = 0.5f;

	public const float MinimumSellPrice = 0.01f;

	public const float PriceFactorBuy_Global = 1.4f;

	public const float PriceFactorSell_Global = 0.6f;

	public static bool EverPlayerSellable(ThingDef def)
	{
		if (!def.tradeability.PlayerCanSell())
		{
			return false;
		}
		if (def.GetStatValueAbstract(StatDefOf.MarketValue) <= 0f)
		{
			return false;
		}
		if (def.category != ThingCategory.Item && def.category != ThingCategory.Pawn && def.category != ThingCategory.Building)
		{
			return false;
		}
		if (def.category == ThingCategory.Building && !def.Minifiable)
		{
			return false;
		}
		return true;
	}

	public static bool PlayerSellableNow(Thing t, ITrader trader)
	{
		t = t.GetInnerIfMinified();
		if (!EverPlayerSellable(t.def))
		{
			return false;
		}
		if (t.IsNotFresh())
		{
			return false;
		}
		if (t is Apparel { WornByCorpse: not false })
		{
			return false;
		}
		if (CompBiocodable.IsBiocoded(t))
		{
			return false;
		}
		if (t is Pawn pawn)
		{
			if ((pawn.GetExtraHostFaction() != null && pawn.GetExtraHostFaction() == trader.Faction) || (pawn.IsQuestLodger() && pawn.GetExtraHomeFaction() == trader.Faction) || (pawn.HomeFaction != null && pawn.HomeFaction == trader.Faction && !pawn.IsSlave && !pawn.IsPrisoner))
			{
				return false;
			}
			if (pawn.RaceProps.Animal && pawn.IsFormingCaravan())
			{
				return false;
			}
		}
		return true;
	}

	public static void SpawnDropPod(IntVec3 dropSpot, Map map, Thing t)
	{
		ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
		activeTransporterInfo.SingleContainedThing = t;
		activeTransporterInfo.leaveSlag = false;
		DropPodUtility.MakeDropPodAt(dropSpot, map, activeTransporterInfo);
	}

	public static IEnumerable<Thing> AllLaunchableThingsForTrade(Map map, ITrader trader = null)
	{
		HashSet<Thing> yieldedThings = new HashSet<Thing>();
		foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
		{
			foreach (IntVec3 tradeableCell in item.TradeableCells)
			{
				List<Thing> thingList = tradeableCell.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing t = thingList[i];
					if (ModsConfig.BiotechActive && t.def == ThingDefOf.GeneBank)
					{
						CompGenepackContainer compGenepackContainer = t.TryGetComp<CompGenepackContainer>();
						if (compGenepackContainer == null)
						{
							continue;
						}
						List<Genepack> containedGenepacks = compGenepackContainer.ContainedGenepacks;
						foreach (Genepack item2 in containedGenepacks)
						{
							if (PlayerSellableNow(t, trader) && !yieldedThings.Contains(item2))
							{
								yieldedThings.Add(item2);
								yield return item2;
							}
						}
					}
					else if (t is Building_Bookcase building_Bookcase)
					{
						foreach (Book heldBook in building_Bookcase.HeldBooks)
						{
							if (PlayerSellableNow(t, trader) && !yieldedThings.Contains(heldBook))
							{
								yieldedThings.Add(heldBook);
								yield return heldBook;
							}
						}
					}
					else if (t is Building_OutfitStand building_OutfitStand)
					{
						foreach (Thing heldItem in building_OutfitStand.HeldItems)
						{
							if (PlayerSellableNow(t, trader) && !yieldedThings.Contains(heldItem))
							{
								yieldedThings.Add(heldItem);
								yield return heldItem;
							}
						}
					}
					else if (t.def.category == ThingCategory.Item && PlayerSellableNow(t, trader) && !yieldedThings.Contains(t))
					{
						yieldedThings.Add(t);
						yield return t;
					}
				}
			}
		}
	}

	public static IEnumerable<Pawn> AllSellableColonyPawns(Map map, bool checkAcceptableTemperatureOfAnimals = true)
	{
		foreach (Pawn item in map.mapPawns.PrisonersOfColonySpawned)
		{
			if (item.guest.PrisonerIsSecure)
			{
				yield return item;
			}
		}
		foreach (Pawn item2 in map.mapPawns.SlavesOfColonySpawned)
		{
			if (item2.guest.SlaveIsSecure)
			{
				yield return item2;
			}
		}
		foreach (Pawn item3 in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (item3.IsAnimal && item3.HostFaction == null && !item3.InMentalState && !item3.Downed && (!checkAcceptableTemperatureOfAnimals || map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(item3.def)))
			{
				yield return item3;
			}
		}
	}

	public static Thing ThingFromStockToMergeWith(ITrader trader, Thing thing)
	{
		if (thing is Pawn)
		{
			return null;
		}
		foreach (Thing good in trader.Goods)
		{
			if (TransferableUtility.TransferAsOne(good, thing, TransferAsOneMode.Normal) && good.CanStackWith(thing) && good.def.stackLimit != 1)
			{
				return good;
			}
		}
		return null;
	}

	public static void LaunchThingsOfType(ThingDef resDef, int debt, Map map, TradeShip trader)
	{
		while (debt > 0)
		{
			Thing thing = null;
			foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
			{
				foreach (IntVec3 tradeableCell in item.TradeableCells)
				{
					foreach (Thing item2 in map.thingGrid.ThingsAt(tradeableCell))
					{
						if (item2.def == resDef)
						{
							thing = item2;
							goto end_IL_0089;
						}
					}
				}
				continue;
				end_IL_0089:
				break;
			}
			if (thing == null)
			{
				Log.Error("Could not find any " + resDef?.ToString() + " to transfer to trader.");
				break;
			}
			int num = Math.Min(debt, thing.stackCount);
			if (trader != null)
			{
				trader.GiveSoldThingToTrader(thing, num, TradeSession.playerNegotiator);
			}
			else
			{
				thing.SplitOff(num).Destroy();
			}
			debt -= num;
		}
	}

	public static void LaunchSilver(Map map, int fee)
	{
		LaunchThingsOfType(ThingDefOf.Silver, fee, map, null);
	}

	public static Map PlayerHomeMapWithMostLaunchableSilver()
	{
		return Find.Maps.Where((Map x) => x.IsPlayerHome).MaxBy((Map x) => (from t in AllLaunchableThingsForTrade(x)
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount));
	}

	public static bool ColonyHasEnoughSilver(Map map, int fee)
	{
		return (from t in AllLaunchableThingsForTrade(map)
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount) >= fee;
	}

	public static void CheckInteractWithTradersTeachOpportunity(Pawn pawn)
	{
		if (!pawn.Dead)
		{
			Lord lord = pawn.GetLord();
			if (lord != null && lord.CurLordToil is LordToil_DefendTraderCaravan)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.InteractingWithTraders, pawn, OpportunityType.Important);
			}
		}
	}

	public static float GetPricePlayerSell(Thing thing, float priceFactorSell_TraderPriceType, float priceFactorSell_HumanPawn, float priceGain_PlayerNegotiator, float priceGain_FactionBase, float priceGain_DrugBonus, float priceGain_AnimalProduceBonus, TradeCurrency currency = TradeCurrency.Silver)
	{
		if (currency == TradeCurrency.Favor)
		{
			return thing.RoyalFavorValue;
		}
		float statValue = thing.GetStatValue(StatDefOf.SellPriceFactor);
		float num = thing.MarketValue * 0.6f * priceFactorSell_TraderPriceType * statValue * priceFactorSell_HumanPawn * (1f - Find.Storyteller.difficulty.tradePriceFactorLoss);
		num *= 1f + priceGain_PlayerNegotiator + priceGain_DrugBonus + priceGain_FactionBase + priceGain_AnimalProduceBonus;
		num = Mathf.Max(num, 0.01f);
		if (num > 99.5f)
		{
			num = Mathf.Round(num);
		}
		return num;
	}

	public static float GetPricePlayerBuy(Thing thing, float priceFactorBuy_TraderPriceType, float priceFactorBuy_JoinAs, float priceGain_PlayerNegotiator, float priceGain_FactionBase)
	{
		float num = thing.MarketValue * 1.4f * priceFactorBuy_TraderPriceType * priceFactorBuy_JoinAs * (1f + Find.Storyteller.difficulty.tradePriceFactorLoss);
		num *= 1f - priceGain_PlayerNegotiator - priceGain_FactionBase;
		num = Mathf.Max(num, 0.5f);
		if (num > 99.5f)
		{
			num = Mathf.Round(num);
		}
		return num;
	}

	public static float TotalMarketValue(IEnumerable<Thing> things)
	{
		if (things == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Thing thing in things)
		{
			num += thing.MarketValue * (float)thing.stackCount;
		}
		return num;
	}

	public static void CheckGiveTraderQuest(Pawn trader)
	{
		if (ModsConfig.OdysseyActive && Rand.Chance(Find.Storyteller.QuestGiverTraderChanceNow))
		{
			trader.mindState.hasQuest = true;
			Find.Storyteller.Notify_QuestGiverTraderSpawned();
		}
	}

	public static void ReceiveQuestFromTrader(Pawn trader, Pawn negotiator)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		Slate slate = new Slate();
		slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(negotiator.Map));
		slate.Set("discoveryMethod", "QuestDiscoveredFromTrader".Translate(trader.Named("TRADER"), negotiator.Named("NEGOTIATOR")));
		List<QuestScriptDef> list = QuestUtility.GetGiverQuests(QuestGiverTag.Traders).ToList();
		if (!list.NullOrEmpty())
		{
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(list.RandomElementByWeight((QuestScriptDef q) => q.rootSelectionWeight), slate);
			Messages.Message("MessageTraderGaveQuest".Translate(quest.name, trader.Named("TRADER"), negotiator.Named("NEGOTIATOR")), MessageTypeDefOf.PositiveEvent);
			if (!quest.hidden && quest.root.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest, slate.Get<string>("discoveryMethod"));
			}
			trader.mindState.hasQuest = false;
		}
	}
}
