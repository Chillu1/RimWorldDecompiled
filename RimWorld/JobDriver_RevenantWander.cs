using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RevenantWander : JobDriver
{
	private const int SmearMTBDays = 2;

	private const int MinSmearInterval = 60000;

	private CompRevenant Comp => pawn.TryGetComp<CompRevenant>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Comp.becomeInvisibleTick = Find.TickManager.TicksGame + 140;
		Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (Find.TickManager.TicksGame > Comp.revenantLastLeftSmear + 60000 && Rand.MTBEventOccurs(2f, 60000f, delta))
			{
				RevenantUtility.CreateRevenantSmear(base.pawn);
			}
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
			if (Rand.MTBEventOccurs(900f, 1f, delta))
			{
				Pawn closestTargetInRadius = RevenantUtility.GetClosestTargetInRadius(base.pawn, 10f);
				if (closestTargetInRadius != null)
				{
					base.pawn.mindState.enemyTarget = closestTargetInRadius;
					Comp.revenantState = RevenantState.Attack;
					EndJobWith(JobCondition.InterruptForced);
				}
			}
		});
		yield return toil;
	}
}
