using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RevenantSleep : JobDriver
{
	private CompRevenant Comp => pawn.TryGetComp<CompRevenant>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			base.Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
			pawn.pather?.StopDead();
		});
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (Find.TickManager.TicksGame >= Comp.nextHypnosis && Rand.MTBEventOccurs(2500f, 1f, delta))
			{
				Pawn pawn = RevenantUtility.ScanForTarget(base.pawn);
				if (pawn != null)
				{
					base.pawn.mindState.enemyTarget = pawn;
					Comp.revenantState = RevenantState.Attack;
					EndJobWith(JobCondition.InterruptForced);
				}
			}
		});
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		yield return toil;
	}
}
