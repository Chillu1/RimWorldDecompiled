using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Dialog_Trade : Window
{
	private bool giftsOnly;

	private Vector2 scrollPosition = Vector2.zero;

	private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	public static float lastCurrencyFlashTime = -100f;

	private List<Tradeable> cachedTradeables;

	private Tradeable cachedCurrencyTradeable;

	private TransferableSorterDef sorter1;

	private TransferableSorterDef sorter2;

	private bool playerIsCaravan;

	private bool isShuttleCaravan;

	private List<Thing> playerCaravanAllPawnsAndItems;

	private bool massUsageDirty = true;

	private float cachedMassUsage;

	private bool massCapacityDirty = true;

	private float cachedMassCapacity;

	private string cachedMassCapacityExplanation;

	private bool tilesPerDayDirty = true;

	private float cachedTilesPerDay;

	private string cachedTilesPerDayExplanation;

	private bool daysWorthOfFoodDirty = true;

	private (float days, float tillRot) cachedDaysWorthOfFood;

	private bool foragedFoodPerDayDirty = true;

	private (ThingDef food, float perDay) cachedForagedFoodPerDay;

	private string cachedForagedFoodPerDayExplanation;

	private bool visibilityDirty = true;

	private float cachedVisibility;

	private string cachedVisibilityExplanation;

	private const float TitleAreaHeight = 45f;

	private const float TopAreaHeight = 58f;

	private const float ColumnWidth = 120f;

	private const float FirstCommodityY = 6f;

	private const float RowInterval = 30f;

	private const float SpaceBetweenTraderNameAndTraderKind = 27f;

	private const float ShowSellableItemsIconSize = 32f;

	private const float GiftModeIconSize = 32f;

	private const float TradeModeIconSize = 32f;

	protected static readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

	protected static readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

	private static readonly Texture2D ShowSellableItemsIcon = ContentFinder<Texture2D>.Get("UI/Commands/SellableItems");

	private static readonly Texture2D GiftModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/GiftMode");

	private static readonly Texture2D TradeModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/TradeMode");

	public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

	private PlanetTile Tile => TradeSession.playerNegotiator.Tile;

	private BiomeDef Biome => Find.WorldGrid[Tile].PrimaryBiome;

	private float MassUsage
	{
		get
		{
			if (massUsageDirty)
			{
				massUsageDirty = false;
				TradeSession.deal.UpdateCurrencyCount();
				if (cachedCurrencyTradeable != null)
				{
					cachedTradeables.Add(cachedCurrencyTradeable);
				}
				Building_PassengerShuttle shuttle = TradeSession.playerNegotiator.GetCaravan().Shuttle;
				if (shuttle != null)
				{
					cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, IgnorePawnsInventoryMode.Ignore, includePawnsMass: true);
					cachedMassUsage -= shuttle.GetStatValue(StatDefOf.Mass);
				}
				else
				{
					cachedMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, IgnorePawnsInventoryMode.Ignore);
				}
				if (cachedCurrencyTradeable != null)
				{
					cachedTradeables.RemoveLast();
				}
			}
			return cachedMassUsage;
		}
	}

	private float MassCapacity
	{
		get
		{
			if (massCapacityDirty)
			{
				massCapacityDirty = false;
				Building_PassengerShuttle shuttle = TradeSession.playerNegotiator.GetCaravan().Shuttle;
				if (shuttle != null)
				{
					cachedMassCapacity = shuttle.TransporterComp.MassCapacity;
					return cachedMassCapacity;
				}
				TradeSession.deal.UpdateCurrencyCount();
				if (cachedCurrencyTradeable != null)
				{
					cachedTradeables.Add(cachedCurrencyTradeable);
				}
				StringBuilder stringBuilder = new StringBuilder();
				cachedMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, stringBuilder);
				cachedMassCapacityExplanation = stringBuilder.ToString();
				if (cachedCurrencyTradeable != null)
				{
					cachedTradeables.RemoveLast();
				}
			}
			return cachedMassCapacity;
		}
	}

	private float TilesPerDay
	{
		get
		{
			if (tilesPerDayDirty)
			{
				tilesPerDayDirty = false;
				TradeSession.deal.UpdateCurrencyCount();
				Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
				if (caravan.Shuttle != null)
				{
					cachedTilesPerDayExplanation = "CaravanMovementSpeedShuttle".Translate();
					return 0f;
				}
				StringBuilder stringBuilder = new StringBuilder();
				cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, MassUsage, MassCapacity, Tile, caravan.pather.Moving ? caravan.pather.nextTile : PlanetTile.Invalid, TradeSession.playerNegotiator.GetCaravan().Shuttle != null, stringBuilder);
				cachedTilesPerDayExplanation = stringBuilder.ToString();
			}
			return cachedTilesPerDay;
		}
	}

	private (float days, float tillRot) DaysWorthOfFood
	{
		get
		{
			if (daysWorthOfFoodDirty)
			{
				daysWorthOfFoodDirty = false;
				TradeSession.deal.UpdateCurrencyCount();
				float item = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Tile, IgnorePawnsInventoryMode.Ignore, Faction.OfPlayer);
				cachedDaysWorthOfFood = (days: item, tillRot: DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Tile, IgnorePawnsInventoryMode.Ignore));
			}
			return cachedDaysWorthOfFood;
		}
	}

	private (ThingDef food, float perDay) ForagedFoodPerDay
	{
		get
		{
			if (foragedFoodPerDayDirty)
			{
				foragedFoodPerDayDirty = false;
				TradeSession.deal.UpdateCurrencyCount();
				StringBuilder stringBuilder = new StringBuilder();
				cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, Biome, Faction.OfPlayer, stringBuilder);
				cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
			}
			return cachedForagedFoodPerDay;
		}
	}

	private float Visibility
	{
		get
		{
			if (visibilityDirty)
			{
				visibilityDirty = false;
				TradeSession.deal.UpdateCurrencyCount();
				StringBuilder stringBuilder = new StringBuilder();
				cachedVisibility = CaravanVisibilityCalculator.VisibilityLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, cachedTradeables, stringBuilder);
				cachedVisibilityExplanation = stringBuilder.ToString();
			}
			return cachedVisibility;
		}
	}

	public override QuickSearchWidget CommonSearchWidget => quickSearchWidget;

	public Dialog_Trade(Pawn playerNegotiator, ITrader trader, bool giftsOnly = false)
	{
		this.giftsOnly = giftsOnly;
		TradeSession.SetupWith(trader, playerNegotiator, giftsOnly);
		SetupPlayerCaravanVariables();
		forcePause = true;
		absorbInputAroundWindow = true;
		soundAppear = SoundDefOf.CommsWindow_Open;
		soundClose = SoundDefOf.CommsWindow_Close;
		if (trader is PassingShip)
		{
			soundAmbient = SoundDefOf.RadioComms_Ambience;
		}
		commonSearchWidgetOffset.x += 18f;
		commonSearchWidgetOffset.y -= 18f;
		sorter1 = TransferableSorterDefOf.Category;
		sorter2 = TransferableSorterDefOf.MarketValue;
	}

	public override void PreOpen()
	{
		base.PreOpen();
		quickSearchWidget.Reset();
	}

	public override void PostOpen()
	{
		base.PostOpen();
		if (!giftsOnly && !playerIsCaravan)
		{
			Pawn playerNegotiator = TradeSession.playerNegotiator;
			float level = playerNegotiator.health.capacities.GetLevel(PawnCapacityDefOf.Talking);
			float level2 = playerNegotiator.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
			if (level < 0.95f || level2 < 0.95f)
			{
				TaggedString text = ((!(level < 0.95f)) ? "NegotiatorHearingImpaired".Translate(playerNegotiator.LabelShort, playerNegotiator) : "NegotiatorTalkingImpaired".Translate(playerNegotiator.LabelShort, playerNegotiator));
				text += "\n\n" + "NegotiatorCapacityImpaired".Translate();
				Find.WindowStack.Add(new Dialog_MessageBox(text));
			}
		}
		CacheTradeables();
	}

	private void CacheTradeables()
	{
		cachedCurrencyTradeable = TradeSession.deal.AllTradeables.FirstOrDefault((Tradeable x) => x.IsCurrency && (TradeSession.TradeCurrency != TradeCurrency.Favor || x.IsFavor));
		cachedTradeables = (from tr in TradeSession.deal.AllTradeables
			where !tr.IsCurrency && (tr.TraderWillTrade || !TradeSession.trader.TraderKind.hideThingsNotWillingToTrade)
			where quickSearchWidget.filter.Matches(tr.Label)
			orderby (!tr.TraderWillTrade) ? (-1) : 0 descending
			select tr).ThenBy((Tradeable tr) => tr, sorter1.Comparer).ThenBy((Tradeable tr) => tr, sorter2.Comparer).ThenBy((Tradeable tr) => TransferableUIUtility.DefaultListOrderPriority(tr))
			.ThenBy((Tradeable tr) => tr.ThingDef.label)
			.ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out var qc) ? ((int)qc) : (-1))
			.ThenBy((Tradeable tr) => tr.AnyThing.HitPoints)
			.ToList();
		quickSearchWidget.noResultsMatched = !cachedTradeables.Any();
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (playerIsCaravan)
		{
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, cachedMassCapacityExplanation, TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation), null, Tile, null, -9999f, new Rect(12f, 0f, inRect.width - 24f, 40f));
			inRect.yMin += 52f;
		}
		TradeSession.deal.UpdateCurrencyCount();
		Widgets.BeginGroup(inRect);
		inRect = inRect.AtZero();
		TransferableUIUtility.DoTransferableSorters(sorter1, sorter2, delegate(TransferableSorterDef x)
		{
			sorter1 = x;
			CacheTradeables();
		}, delegate(TransferableSorterDef x)
		{
			sorter2 = x;
			CacheTradeables();
		});
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.Label(new Rect(0f, 27f, inRect.width / 2f, inRect.height / 2f), "NegotiatorTradeDialogInfo".Translate(TradeSession.playerNegotiator.Name.ToStringFull, TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement).ToStringPercent()));
		float num = inRect.width - 590f;
		Rect rect = new Rect(num, 0f, inRect.width - num, 58f);
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Medium;
		Rect rect2 = new Rect(0f, 0f, rect.width / 2f, rect.height);
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.Label(rect2, Faction.OfPlayer.Name.Truncate(rect2.width));
		Rect rect3 = new Rect(rect.width / 2f, 0f, rect.width / 2f, rect.height);
		Text.Anchor = TextAnchor.UpperRight;
		string text = TradeSession.trader.TraderName;
		if (Text.CalcSize(text).x > rect3.width)
		{
			Text.Font = GameFont.Small;
			text = text.Truncate(rect3.width);
		}
		Widgets.Label(rect3, text);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperRight;
		Widgets.Label(new Rect(rect.width / 2f, 27f, rect.width / 2f, rect.height / 2f), TradeSession.trader.TraderKind.LabelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		if (!TradeSession.giftMode)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.6f);
			Text.Font = GameFont.Tiny;
			Rect rect4 = new Rect(rect.width / 2f - 100f - 30f, 0f, 200f, rect.height);
			Text.Anchor = TextAnchor.LowerCenter;
			Widgets.Label(rect4, "PositiveBuysNegativeSells".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
		Widgets.EndGroup();
		float num2 = 0f;
		if (cachedCurrencyTradeable != null)
		{
			float num3 = inRect.width - 16f;
			Rect rect5 = new Rect(0f, 58f, num3, 30f);
			int countToTransfer = cachedCurrencyTradeable.CountToTransfer;
			TradeUI.DrawTradeableRow(rect5, cachedCurrencyTradeable, 1);
			if (countToTransfer != cachedCurrencyTradeable.CountToTransfer)
			{
				CountToTransferChanged();
			}
			GUI.color = Color.gray;
			Widgets.DrawLineHorizontal(0f, 87f, num3);
			GUI.color = Color.white;
			num2 = 30f;
		}
		Rect mainRect = new Rect(0f, 58f + num2, inRect.width, inRect.height - 58f - 38f - num2 - 20f);
		FillMainRect(mainRect);
		Text.Font = GameFont.Small;
		Rect rect6 = new Rect(inRect.width / 2f - AcceptButtonSize.x / 2f, inRect.height - 55f, AcceptButtonSize.x, AcceptButtonSize.y);
		if (Widgets.ButtonText(rect6, TradeSession.giftMode ? ("OfferGifts".Translate() + " (" + FactionGiftUtility.GetGoodwillChange(TradeSession.deal.AllTradeables, TradeSession.trader.Faction).ToStringWithSign() + ")") : "AcceptButton".Translate()))
		{
			Action action = delegate
			{
				if (TradeSession.deal.TryExecute(out var actuallyTraded))
				{
					if (actuallyTraded)
					{
						SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
						TradeSession.playerNegotiator.GetCaravan()?.RecacheInventory();
						Close(doCloseSound: false);
					}
					else
					{
						Close();
					}
				}
			};
			if (TradeSession.deal.DoesTraderHaveEnoughSilver())
			{
				action();
			}
			else
			{
				FlashSilver();
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmTraderShortFunds".Translate(), action));
			}
			Event.current.Use();
		}
		if (Widgets.ButtonText(new Rect(rect6.x - 10f - OtherBottomButtonSize.x, rect6.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "ResetButton".Translate()))
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			TradeSession.deal.Reset();
			CacheTradeables();
			CountToTransferChanged();
		}
		if (Widgets.ButtonText(new Rect(rect6.xMax + 10f, rect6.y, OtherBottomButtonSize.x, OtherBottomButtonSize.y), "CancelButton".Translate()))
		{
			Close();
			Event.current.Use();
		}
		float y = OtherBottomButtonSize.y;
		Rect rect7 = new Rect(inRect.width - y, rect6.y, y, y);
		if (Widgets.ButtonImageWithBG(rect7, ShowSellableItemsIcon, new Vector2(32f, 32f)))
		{
			Find.WindowStack.Add(new Dialog_SellableItems(TradeSession.trader));
		}
		TooltipHandler.TipRegionByKey(rect7, "CommandShowSellableItemsDesc");
		Faction faction = TradeSession.trader.Faction;
		if (faction != null && !giftsOnly && !faction.def.permanentEnemy && TradeSession.trader.TradeCurrency != TradeCurrency.Favor)
		{
			Rect rect8 = new Rect(rect7.x - y - 4f, rect6.y, y, y);
			if (TradeSession.giftMode)
			{
				if (Widgets.ButtonImageWithBG(rect8, TradeModeIcon, new Vector2(32f, 32f)))
				{
					TradeSession.giftMode = false;
					TradeSession.deal.Reset();
					CacheTradeables();
					CountToTransferChanged();
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect8, "TradeModeTip");
			}
			else
			{
				if (Widgets.ButtonImageWithBG(rect8, GiftModeIcon, new Vector2(32f, 32f)))
				{
					TradeSession.giftMode = true;
					TradeSession.deal.Reset();
					CacheTradeables();
					CountToTransferChanged();
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect8, "GiftModeTip", faction.Name);
			}
		}
		Widgets.EndGroup();
	}

	public override void Close(bool doCloseSound = true)
	{
		DragSliderManager.ForceStop();
		base.Close(doCloseSound);
		if (TradeSession.trader is Pawn pawn && pawn.mindState.hasQuest)
		{
			TradeUtility.ReceiveQuestFromTrader(pawn, TradeSession.playerNegotiator);
		}
	}

	private void FillMainRect(Rect mainRect)
	{
		Text.Font = GameFont.Small;
		float height = 6f + (float)cachedTradeables.Count * 30f;
		Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
		Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
		float num = 6f;
		float num2 = scrollPosition.y - 30f;
		float num3 = scrollPosition.y + mainRect.height;
		int num4 = 0;
		for (int i = 0; i < cachedTradeables.Count; i++)
		{
			if (num > num2 && num < num3)
			{
				Rect rect = new Rect(0f, num, viewRect.width, 30f);
				int countToTransfer = cachedTradeables[i].CountToTransfer;
				TradeUI.DrawTradeableRow(rect, cachedTradeables[i], num4);
				if (countToTransfer != cachedTradeables[i].CountToTransfer)
				{
					CountToTransferChanged();
				}
			}
			num += 30f;
			num4++;
		}
		Widgets.EndScrollView();
	}

	public void FlashSilver()
	{
		lastCurrencyFlashTime = Time.time;
	}

	public override bool CausesMessageBackground()
	{
		return true;
	}

	private void SetupPlayerCaravanVariables()
	{
		Caravan caravan = TradeSession.playerNegotiator.GetCaravan();
		if (caravan != null)
		{
			playerIsCaravan = true;
			playerCaravanAllPawnsAndItems = new List<Thing>();
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				playerCaravanAllPawnsAndItems.Add(pawnsListForReading[i]);
			}
			playerCaravanAllPawnsAndItems.AddRange(CaravanInventoryUtility.AllInventoryItems(caravan));
			caravan.Notify_StartedTrading();
		}
		else
		{
			playerIsCaravan = false;
		}
	}

	private void CountToTransferChanged()
	{
		massUsageDirty = true;
		massCapacityDirty = true;
		tilesPerDayDirty = true;
		daysWorthOfFoodDirty = true;
		foragedFoodPerDayDirty = true;
		visibilityDirty = true;
	}

	public override void Notify_CommonSearchChanged()
	{
		CacheTradeables();
	}
}
