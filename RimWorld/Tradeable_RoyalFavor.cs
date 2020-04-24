using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable_RoyalFavor : Tradeable
	{
		public override bool IsFavor => true;

		public override bool IsCurrency => true;

		public override bool TraderWillTrade => true;

		public override bool IsThing => false;

		public override bool Interactive => false;

		public override Thing AnyThing => null;

		public override string Label => TradeSession.trader.Faction.def.royalFavorLabel;

		public override string TipDescription => "RoyalFavorDescription".Translate(TradeSession.trader.Faction.Named("FACTION"));

		public override int CostToInt(float cost)
		{
			return Mathf.CeilToInt(cost);
		}

		public override void ResolveTrade()
		{
			if (base.ActionToDo == TradeAction.PlayerBuys)
			{
				TradeSession.playerNegotiator.royalty.GainFavor(TradeSession.trader.Faction, base.CountToTransferToSource);
			}
		}

		public override void DrawIcon(Rect iconRect)
		{
			Faction faction = TradeSession.trader.Faction;
			GUI.color = faction.Color;
			Widgets.DrawTextureRotated(iconRect, faction.def.FactionIcon, 0f);
			GUI.color = Color.white;
		}

		public override int CountHeldBy(Transactor trans)
		{
			if (trans == Transactor.Trader)
			{
				return 99999;
			}
			return 0;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}
