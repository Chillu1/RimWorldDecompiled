using Verse;

namespace RimWorld
{
	public static class TradeSession
	{
		public static ITrader trader;

		public static Pawn playerNegotiator;

		public static TradeDeal deal;

		public static bool giftMode;

		public static bool Active => trader != null;

		public static TradeCurrency TradeCurrency => trader.TradeCurrency;

		public static void SetupWith(ITrader newTrader, Pawn newPlayerNegotiator, bool giftMode)
		{
			if (!newTrader.CanTradeNow)
			{
				Log.Warning("Called SetupWith with a trader not willing to trade now.");
			}
			trader = newTrader;
			playerNegotiator = newPlayerNegotiator;
			TradeSession.giftMode = giftMode;
			deal = new TradeDeal();
			if (!giftMode && deal.cannotSellReasons.Count > 0)
			{
				Messages.Message("MessageCannotSellItemsReason".Translate() + deal.cannotSellReasons.ToCommaList(useAnd: true), MessageTypeDefOf.NegativeEvent, historical: false);
			}
		}

		public static void Close()
		{
			trader = null;
		}
	}
}
