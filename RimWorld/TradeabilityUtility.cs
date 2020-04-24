namespace RimWorld
{
	public static class TradeabilityUtility
	{
		public static bool PlayerCanSell(this Tradeability tradeability)
		{
			if (tradeability != Tradeability.All)
			{
				return tradeability == Tradeability.Sellable;
			}
			return true;
		}

		public static bool TraderCanSell(this Tradeability tradeability)
		{
			if (tradeability != Tradeability.All)
			{
				return tradeability == Tradeability.Buyable;
			}
			return true;
		}
	}
}
