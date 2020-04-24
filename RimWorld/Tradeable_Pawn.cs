using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable_Pawn : Tradeable
	{
		public override Window NewInfoDialog => new Dialog_InfoCard(AnyPawn);

		public override string Label
		{
			get
			{
				string text = base.Label;
				if (AnyPawn.Name != null && !AnyPawn.Name.Numerical)
				{
					text = text + ", " + AnyPawn.def.label;
				}
				return text + " (" + AnyPawn.GetGenderLabel() + ", " + Mathf.FloorToInt(AnyPawn.ageTracker.AgeBiologicalYearsFloat).ToString() + ")";
			}
		}

		public override string TipDescription
		{
			get
			{
				if (!HasAnyThing)
				{
					return "";
				}
				return AnyPawn.MainDesc(writeFaction: true) + "\n\n" + AnyPawn.def.description;
			}
		}

		private Pawn AnyPawn => (Pawn)AnyThing;

		public override void ResolveTrade()
		{
			if (base.ActionToDo == TradeAction.PlayerSells)
			{
				List<Pawn> list = thingsColony.Take(base.CountToTransferToDestination).Cast<Pawn>().ToList();
				for (int i = 0; i < list.Count; i++)
				{
					TradeSession.trader.GiveSoldThingToTrader(list[i], 1, TradeSession.playerNegotiator);
				}
			}
			else if (base.ActionToDo == TradeAction.PlayerBuys)
			{
				List<Pawn> list2 = thingsTrader.Take(base.CountToTransferToSource).Cast<Pawn>().ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					TradeSession.trader.GiveSoldThingToPlayer(list2[j], 1, TradeSession.playerNegotiator);
				}
			}
		}
	}
}
