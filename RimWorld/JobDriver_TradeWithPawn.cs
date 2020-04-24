using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TradeWithPawn : JobDriver
	{
		private Pawn Trader => (Pawn)base.TargetThingA;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Trader, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !Trader.CanTradeNow);
			Toil trade = new Toil();
			trade.initAction = delegate
			{
				Pawn actor = trade.actor;
				if (Trader.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(actor, Trader));
				}
			};
			yield return trade;
		}
	}
}
