using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_InvestigateMonolith : JobDriver
{
	private const int InvestigateSeconds = 5;

	private Building_VoidMonolith VoidMonolith => base.TargetThingA as Building_VoidMonolith;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(VoidMonolith, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (!pawn.ReserveSittableOrSpot(VoidMonolith.InteractionCell, job, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (job.targetB == null)
		{
			job.targetB = base.TargetThingA.SpawnedParentOrMe;
		}
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		int ticks = Mathf.RoundToInt(300f);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: true, maintainPosture: true, maintainSleep: false, TargetIndex.A);
		toil.WithEffect(EffecterDefOf.MonolithStage1, () => base.TargetA);
		toil.PlaySustainerOrSound(SoundDefOf.VoidMonolith_InspectLoop);
		toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
		{
			Find.TickManager.slower.SignalForceNormalSpeed();
		});
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			VoidMonolith.Investigate(pawn);
		});
	}
}
