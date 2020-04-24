using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface ITrader
	{
		TraderKindDef TraderKind
		{
			get;
		}

		IEnumerable<Thing> Goods
		{
			get;
		}

		int RandomPriceFactorSeed
		{
			get;
		}

		string TraderName
		{
			get;
		}

		bool CanTradeNow
		{
			get;
		}

		float TradePriceImprovementOffsetForPlayer
		{
			get;
		}

		Faction Faction
		{
			get;
		}

		TradeCurrency TradeCurrency
		{
			get;
		}

		IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator);

		void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator);

		void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator);
	}
}
