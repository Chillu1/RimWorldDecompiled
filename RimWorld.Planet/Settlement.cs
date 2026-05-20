using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class Settlement : MapParent, ITrader, ITraderRestockingInfoProvider, INameableWorldObject
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

	public override string Label => nameInt ?? base.Label;

	public override bool HasName => !nameInt.NullOrEmpty();

	protected override bool UseGenericEnterMapFloatMenuOption => !Attackable;

	public virtual bool Visitable
	{
		get
		{
			if (base.Faction != Faction.OfPlayer && (base.Faction == null || !base.Faction.HostileTo(Faction.OfPlayer)))
			{
				return !base.Tile.LayerDef.isSpace;
			}
			return false;
		}
	}

	public virtual bool Attackable => base.Faction != Faction.OfPlayer;

	public override bool ShowRelatedQuests => base.Faction != Faction.OfPlayer;

	public override bool GravShipCanLandOn => base.Faction != Faction.OfPlayer;

	public override AcceptanceReport CanBeSettled => false;

	public override Material Material
	{
		get
		{
			if (cachedMat == null)
			{
				cachedMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, 3550);
			}
			return cachedMat;
		}
	}

	public override MapGeneratorDef MapGeneratorDef
	{
		get
		{
			if (def.mapGenerator != null)
			{
				return def.mapGenerator;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				return MapGeneratorDefOf.Base_Player;
			}
			return MapGeneratorDefOf.Base_Faction;
		}
	}

	public TraderKindDef TraderKind => trader?.TraderKind;

	public IEnumerable<Thing> Goods => trader?.StockListForReading;

	public int RandomPriceFactorSeed => trader?.RandomPriceFactorSeed ?? 0;

	public string TraderName => trader?.TraderName;

	public bool CanTradeNow
	{
		get
		{
			if (trader != null)
			{
				return trader.CanTradeNow;
			}
			return false;
		}
	}

	public float TradePriceImprovementOffsetForPlayer => trader?.TradePriceImprovementOffsetForPlayer ?? 0f;

	public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

	public bool EverVisited => trader.EverVisited;

	public bool RestockedSinceLastVisit => trader.RestockedSinceLastVisit;

	public int NextRestockTick => trader.NextRestockTick;

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		return trader?.ColonyThingsWillingToBuy(playerNegotiator);
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
		if (base.Faction == null || base.Faction == Faction.OfPlayer || SettlementDefeatUtility.IsDefeated(base.Map, base.Faction))
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
			if (base.Faction == null)
			{
				Log.Warning("Settlement '" + Name + "' had null faction on load - destroying.");
				Destroy();
			}
		}
	}

	public override void PostMapGenerate()
	{
		base.PostMapGenerate();
		if (!base.Map.IsPlayerHome && TryGetComponent<TimedDetectionRaids>(out var comp))
		{
			comp.StartDetectionCountdown(240000);
		}
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
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
		if (base.Map.AnyBuildingBlockingMapRemoval)
		{
			return false;
		}
		if (base.Map.IsPlayerHome || base.Map.mapPawns.AnyPawnBlockingMapRemoval)
		{
			return false;
		}
		if (TransporterUtility.IncomingTransporterPreventingMapRemoval(base.Map))
		{
			return false;
		}
		return true;
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
			text += base.Faction.PlayerRelationKind.GetLabelCap();
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
		else
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "Settled".Translate() + ": " + GenDate.DateShortStringAt(GenDate.TickGameToAbs(creationGameTicks), Find.WorldGrid.LongLatOf(base.Tile));
			text += " (";
			text += "TimeAgo".Translate((Find.TickManager.TicksGame - creationGameTicks).ToStringTicksToPeriodVague());
			text += ")";
		}
		return text;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (TraderKind != null && !base.Faction.def.permanentEnemy)
		{
			yield return new Command_Action
			{
				defaultLabel = "CommandShowSellableItems".Translate(),
				defaultDesc = "CommandShowSellableItemsDesc".Translate(),
				icon = ShowSellableItemsCommand,
				action = delegate
				{
					Find.WindowStack.Add(new Dialog_SellableItems(this));
					RoyalTitleDef titleRequiredToTrade = TraderKind.TitleRequiredToTrade;
					if (titleRequiredToTrade != null)
					{
						TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradingRequiresPermit, titleRequiredToTrade.GetLabelCapForBothGenders());
					}
				}
			};
		}
		if (base.Faction != Faction.OfPlayer && !PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.FormCaravan) && base.Tile.LayerDef.canFormCaravans)
		{
			yield return new Command_Action
			{
				defaultLabel = "CommandFormCaravan".Translate(),
				defaultDesc = "CommandFormCaravanDesc".Translate(),
				icon = FormCaravanCommand,
				action = delegate
				{
					Find.Tutor.learningReadout.TryActivateConcept(ConceptDefOf.FormCaravan);
					Messages.Message("MessageSelectOwnBaseToFormCaravan".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
			};
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
			yield return new Command_Action
			{
				icon = AttackCommand,
				defaultLabel = "CommandAttackSettlement".Translate(),
				defaultDesc = "CommandAttackSettlementDesc".Translate(),
				action = delegate
				{
					SettlementUtility.Attack(caravan, this);
				}
			};
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

	public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
		{
			yield return transportersFloatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSettlement.GetFloatMenuOptions(launchAction, pods, this, isShuttle: false))
		{
			yield return floatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalAction_GiveGift.GetFloatMenuOptions(launchAction, pods, this))
		{
			yield return floatMenuOption2;
		}
		if (base.HasMap)
		{
			yield break;
		}
		foreach (FloatMenuOption floatMenuOption3 in TransportersArrivalAction_AttackSettlement.GetFloatMenuOptions(launchAction, pods, this))
		{
			yield return floatMenuOption3;
		}
	}

	public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		foreach (FloatMenuOption shuttleFloatMenuOption in base.GetShuttleFloatMenuOptions(pods, launchAction))
		{
			yield return shuttleFloatMenuOption;
		}
		if ((bool)TransportersArrivalAction_VisitSettlement.CanVisit(pods, this))
		{
			yield return new FloatMenuOption("VisitSettlement".Translate(Label), delegate
			{
				launchAction(base.Tile, new TransportersArrivalAction_VisitSettlement(this, "MessageShuttleArrived"));
			});
		}
		if ((bool)TransportersArrivalAction_Trade.CanTradeWith(pods, this))
		{
			yield return new FloatMenuOption("TradeWithSettlement".Translate(Label), delegate
			{
				launchAction(base.Tile, new TransportersArrivalAction_Trade(this, "MessageShuttleArrived"));
			});
		}
		if (base.HasMap)
		{
			yield break;
		}
		IThingHolder thingHolder = pods.FirstOrDefault();
		CompTransporter firstPod = thingHolder as CompTransporter;
		if (firstPod == null || firstPod.Shuttle.shipParent == null)
		{
			yield break;
		}
		TaggedString message = (base.Faction.HostileTo(Faction.OfPlayer) ? "ConfirmLandOnHostileFactionBase".Translate(base.Faction) : "ConfirmLandOnNeutralFactionBase".Translate(base.Faction));
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(() => TransportersArrivalAction_AttackSettlement.CanAttack(pods, this), () => new TransportersArrivalAction_TransportShip(this, firstPod.Shuttle.shipParent), "AttackShuttle".Translate(Label), delegate(PlanetTile t, TransportersArrivalAction s)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(message, delegate
			{
				launchAction(t, s);
			}));
		}, base.Tile))
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

	public override void Abandon(bool wasGravshipLaunch)
	{
		base.Abandon(wasGravshipLaunch);
		if (!wasGravshipLaunch && base.Tile.LayerDef == PlanetLayerDefOf.Surface)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedSettlement);
			worldObject.Tile = base.Tile;
			worldObject.SetFaction(base.Faction);
			Find.WorldObjects.Add(worldObject);
		}
	}
}
