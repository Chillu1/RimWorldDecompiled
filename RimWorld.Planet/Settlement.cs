using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Settlement : MapParent, ITrader, ITraderRestockingInfoProvider
	{
		public Settlement_TraderTracker trader;

		public List<Pawn> previouslyGeneratedInhabitants = new List<Pawn>();

		private string nameInt;

		public bool namedByPlayer;

		private Material cachedMat;

		public static readonly Texture2D ShowSellableItemsCommand = ContentFinder<Texture2D>.Get("UI/Commands/SellableItems");

		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

		public static readonly Texture2D AttackCommand = ContentFinder<Texture2D>.Get("UI/Commands/AttackSettlement");

		public string Name
		{
			get
			{
				return nameInt;
			}
			set
			{
				nameInt = value;
			}
		}

		public override Texture2D ExpandingIcon => base.Faction.def.FactionIcon;

		public override string Label
		{
			get
			{
				if (nameInt == null)
				{
					return base.Label;
				}
				return nameInt;
			}
		}

		public override bool HasName => !nameInt.NullOrEmpty();

		protected override bool UseGenericEnterMapFloatMenuOption => !Attackable;

		public virtual bool Visitable
		{
			get
			{
				if (base.Faction != Faction.OfPlayer)
				{
					if (base.Faction != null)
					{
						return !base.Faction.HostileTo(Faction.OfPlayer);
					}
					return true;
				}
				return false;
			}
		}

		public virtual bool Attackable => base.Faction != Faction.OfPlayer;

		public override bool ShowRelatedQuests => base.Faction != Faction.OfPlayer;

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
				}
				return cachedMat;
			}
		}

		public override MapGeneratorDef MapGeneratorDef
		{
			get
			{
				if (base.Faction == Faction.OfPlayer)
				{
					return MapGeneratorDefOf.Base_Player;
				}
				return MapGeneratorDefOf.Base_Faction;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.TraderKind;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.StockListForReading;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				if (trader == null)
				{
					return 0;
				}
				return trader.RandomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.TraderName;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				if (trader == null)
				{
					return false;
				}
				return trader.CanTradeNow;
			}
		}

		public float TradePriceImprovementOffsetForPlayer
		{
			get
			{
				if (trader == null)
				{
					return 0f;
				}
				return trader.TradePriceImprovementOffsetForPlayer;
			}
		}

		public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

		public bool EverVisited => trader.EverVisited;

		public bool RestockedSinceLastVisit => trader.RestockedSinceLastVisit;

		public int NextRestockTick => trader.NextRestockTick;

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			if (trader == null)
			{
				return null;
			}
			return trader.ColonyThingsWillingToBuy(playerNegotiator);
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
		}

		public Settlement()
		{
			trader = new Settlement_TraderTracker(this);
		}

		public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			foreach (IncidentTargetTagDef item in base.IncidentTargetTags())
			{
				yield return item;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				yield return IncidentTargetTagDefOf.Map_PlayerHome;
			}
			else
			{
				yield return IncidentTargetTagDefOf.Map_Misc;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref previouslyGeneratedInhabitants, "previouslyGeneratedInhabitants", LookMode.Reference);
			Scribe_Deep.Look(ref trader, "trader", this);
			Scribe_Values.Look(ref nameInt, "nameInt");
			Scribe_Values.Look(ref namedByPlayer, "namedByPlayer", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				previouslyGeneratedInhabitants.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			if (!base.Map.IsPlayerHome)
			{
				GetComponent<TimedDetectionRaids>().StartDetectionCountdown(240000);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (trader != null)
			{
				trader.TraderTrackerTick();
			}
			SettlementDefeatUtility.CheckDefeated(this);
		}

		public override void Notify_MyMapRemoved(Map map)
		{
			base.Notify_MyMapRemoved(map);
			for (int num = previouslyGeneratedInhabitants.Count - 1; num >= 0; num--)
			{
				Pawn pawn = previouslyGeneratedInhabitants[num];
				if (pawn.DestroyedOrNull() || !pawn.IsWorldPawn())
				{
					previouslyGeneratedInhabitants.RemoveAt(num);
				}
			}
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			if (!base.Map.IsPlayerHome)
			{
				return !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
			}
			return false;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (trader != null)
			{
				trader.TryDestroyStock();
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Faction != Faction.OfPlayer)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += base.Faction.PlayerRelationKind.GetLabel();
				if (!base.Faction.Hidden)
				{
					text = text + " (" + base.Faction.PlayerGoodwill.ToStringWithSign() + ")";
				}
				RoyalTitleDef royalTitleDef = TraderKind?.TitleRequiredToTrade;
				if (royalTitleDef != null)
				{
					text += "\n" + "RequiresTradePermission".Translate(royalTitleDef.GetLabelCapForBothGenders());
				}
			}
			return text;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (TraderKind != null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandShowSellableItems".Translate();
				command_Action.defaultDesc = "CommandShowSellableItemsDesc".Translate();
				command_Action.icon = ShowSellableItemsCommand;
				command_Action.action = delegate
				{
					Find.WindowStack.Add(new Dialog_SellableItems(this));
					RoyalTitleDef titleRequiredToTrade = TraderKind.TitleRequiredToTrade;
					if (titleRequiredToTrade != null)
					{
						TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradingRequiresPermit, titleRequiredToTrade.GetLabelCapForBothGenders());
					}
				};
				yield return command_Action;
			}
			if (base.Faction != Faction.OfPlayer && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.FormCaravan))
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "CommandFormCaravan".Translate();
				command_Action2.defaultDesc = "CommandFormCaravanDesc".Translate();
				command_Action2.icon = FormCaravanCommand;
				command_Action2.action = delegate
				{
					Find.Tutor.learningReadout.TryActivateConcept(ConceptDefOf.FormCaravan);
					Messages.Message("MessageSelectOwnBaseToFormCaravan".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				};
				yield return command_Action2;
			}
		}

		public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			if (CanTradeNow && CaravanVisitUtility.SettlementVisitedNow(caravan) == this)
			{
				yield return CaravanVisitUtility.TradeCommand(caravan, base.Faction, TraderKind);
			}
			if ((bool)CaravanArrivalAction_OfferGifts.CanOfferGiftsTo(caravan, this))
			{
				yield return FactionGiftUtility.OfferGiftsCommand(caravan, this);
			}
			foreach (Gizmo caravanGizmo in base.GetCaravanGizmos(caravan))
			{
				yield return caravanGizmo;
			}
			if (Attackable)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.icon = AttackCommand;
				command_Action.defaultLabel = "CommandAttackSettlement".Translate();
				command_Action.defaultDesc = "CommandAttackSettlementDesc".Translate();
				command_Action.action = delegate
				{
					SettlementUtility.Attack(caravan, this);
				};
				yield return command_Action;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
			{
				yield return floatMenuOption;
			}
			if (CaravanVisitUtility.SettlementVisitedNow(caravan) != this)
			{
				foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitSettlement.GetFloatMenuOptions(caravan, this))
				{
					yield return floatMenuOption2;
				}
			}
			foreach (FloatMenuOption floatMenuOption3 in CaravanArrivalAction_Trade.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption3;
			}
			foreach (FloatMenuOption floatMenuOption4 in CaravanArrivalAction_OfferGifts.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption4;
			}
			foreach (FloatMenuOption floatMenuOption5 in CaravanArrivalAction_AttackSettlement.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption5;
			}
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			foreach (FloatMenuOption transportPodsFloatMenuOption in base.GetTransportPodsFloatMenuOptions(pods, representative))
			{
				yield return transportPodsFloatMenuOption;
			}
			foreach (FloatMenuOption floatMenuOption in TransportPodsArrivalAction_VisitSettlement.GetFloatMenuOptions(representative, pods, this))
			{
				yield return floatMenuOption;
			}
			foreach (FloatMenuOption floatMenuOption2 in TransportPodsArrivalAction_GiveGift.GetFloatMenuOptions(representative, pods, this))
			{
				yield return floatMenuOption2;
			}
			if (base.HasMap)
			{
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption3 in TransportPodsArrivalAction_AttackSettlement.GetFloatMenuOptions(representative, pods, this))
			{
				yield return floatMenuOption3;
			}
		}

		public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
		{
			foreach (FloatMenuOption shuttleFloatMenuOption in base.GetShuttleFloatMenuOptions(pods, launchAction))
			{
				yield return shuttleFloatMenuOption;
			}
			if ((bool)TransportPodsArrivalAction_GiveGift.CanGiveGiftTo(pods, this))
			{
				yield return new FloatMenuOption("GiveGiftViaTransportPods".Translate(base.Faction.Name, FactionGiftUtility.GetGoodwillChange(pods, this).ToStringWithSign()), delegate
				{
					TradeRequestComp tradeReqComp = GetComponent<TradeRequestComp>();
					if (tradeReqComp.ActiveRequest && pods.Any((IThingHolder p) => p.GetDirectlyHeldThings().Contains(tradeReqComp.requestThingDef)))
					{
						Find.WindowStack.Add(new Dialog_MessageBox("GiveGiftViaTransportPodsTradeRequestWarning".Translate(), "Yes".Translate(), delegate
						{
							launchAction(base.Tile, new TransportPodsArrivalAction_GiveGift(this));
						}, "No".Translate()));
					}
					else
					{
						launchAction(base.Tile, new TransportPodsArrivalAction_GiveGift(this));
					}
				});
			}
			if (base.HasMap)
			{
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption in TransportPodsArrivalActionUtility.GetFloatMenuOptions(() => TransportPodsArrivalAction_AttackSettlement.CanAttack(pods, this), () => new TransportPodsArrivalAction_Shuttle(this), "AttackShuttle".Translate(Label), launchAction, base.Tile))
			{
				yield return floatMenuOption;
			}
		}

		public override void GetChildHolders(List<IThingHolder> outChildren)
		{
			base.GetChildHolders(outChildren);
			if (trader != null)
			{
				outChildren.Add(trader);
			}
		}
	}
}
