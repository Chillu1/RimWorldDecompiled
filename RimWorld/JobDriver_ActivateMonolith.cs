using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ActivateMonolith : JobDriver
{
	private const int ActivateSeconds = 10;

	private const TargetIndex MonolithIndex = TargetIndex.A;

	private const TargetIndex ActivationItemIndex = TargetIndex.B;

	private Building_VoidMonolith VoidMonolith => job.GetTarget(TargetIndex.A).Thing as Building_VoidMonolith;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (base.TargetThingC != null)
		{
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		}
		if (base.TargetThingB != null && !pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
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
		this.FailOn(() => !VoidMonolith.CanActivate(out var _, out var _));
		if (Find.Anomaly.NextLevelDef == MonolithLevelDefOf.VoidAwakened)
		{
			yield return Toils_General.Do(delegate
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("VoidAwakeningConfirmationText".Translate(), delegate
				{
				}, delegate
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}));
			});
		}
		if (base.TargetThingB != null)
		{
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch, canGotoSpawnedParent: true).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return getToHaulTarget;
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, canTakeFromInventory: true);
			yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.B);
		}
		if (job.targetC == null)
		{
			job.targetC = base.TargetThingA.SpawnedParentOrMe;
		}
		yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.C).FailOnSomeonePhysicallyInteracting(TargetIndex.C);
		int ticks = Mathf.RoundToInt(600f);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: true, maintainPosture: true, maintainSleep: false, TargetIndex.A);
		toil.WithEffect(EffecterDefOf.MonolithStage2, () => base.TargetA);
		toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
		{
			Find.TickManager.slower.SignalForceNormalSpeed();
		});
		if (Find.Anomaly.LevelDef.activateSound != null)
		{
			toil.PlaySustainerOrSound(Find.Anomaly.LevelDef.activateSound);
		}
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			if (base.TargetThingB != null)
			{
				pawn.carryTracker.DestroyCarriedThing();
			}
			VoidMonolith.Activate(pawn);
		});
		yield return Toils_General.Wait(360, TargetIndex.A);
	}
}
