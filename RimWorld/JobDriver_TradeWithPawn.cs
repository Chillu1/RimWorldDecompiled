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
			JobDriver_TradeWithPawn jobDriver_TradeWithPawn = this;
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !jobDriver_TradeWithPawn.Trader.CanTradeNow);
			Toil trade = new Toil();
			trade.initAction = delegate
			{
				Pawn actor = trade.actor;
				if (jobDriver_TradeWithPawn.Trader.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(actor, jobDriver_TradeWithPawn.Trader));
				}
			};
			yield return trade;
		}
	}
}
