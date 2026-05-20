using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_InteractThing : JobDriver
	{
		private const TargetIndex InteractableIndex = TargetIndex.A;

		private const TargetIndex OptionalItemIndex = TargetIndex.B;

		private const TargetIndex SpawnedOrParentIndex = TargetIndex.C;

		private Thing InteractableThing => job.GetTarget(TargetIndex.A).Thing;

		private Thing OptionalThing => job.GetTarget(TargetIndex.B).Thing;

		private bool HasOptionalThing => OptionalThing != null;

		private CompInteractable Interactable
		{
			get
			{
				if (job.interactableIndex == -1)
				{
					return job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompInteractable>();
				}
				if (!(job.GetTarget(TargetIndex.A).Thing is ThingWithComps thingWithComps))
				{
					return null;
				}
				return thingWithComps.GetComps<CompInteractable>().ToList()[job.interactableIndex];
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (base.TargetThingB != null)
			{
				pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			}
			if (base.TargetThingB != null && !pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (job.GetTarget(TargetIndex.B).HasThing)
			{
				Toil opt = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch, canGotoSpawnedParent: true).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B)
					.FailOn(() => !Interactable.CanInteract(pawn));
				yield return opt;
				yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, canTakeFromInventory: true);
				yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(opt, TargetIndex.B);
			}
			if (job.GetTarget(TargetIndex.A).Thing.SpawnedParentOrMe != null)
			{
				job.SetTarget(TargetIndex.C, job.GetTarget(TargetIndex.A).Thing.SpawnedParentOrMe);
			}
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.C).FailOnSomeonePhysicallyInteracting(TargetIndex.C)
				.FailOn(() => !Interactable.CanInteract(pawn));
			if (Interactable.TicksToActivate != 0)
			{
				int num = Interactable.TicksToActivate;
				if (Interactable.Props.activateStat != null && pawn.GetStatValue(Interactable.Props.activateStat) > 0f)
				{
					num = Mathf.RoundToInt((float)num / pawn.GetStatValue(Interactable.Props.activateStat));
				}
				int remainingTicks = Mathf.RoundToInt((float)num * (1f - Interactable.progress));
				yield return WaitForActivate(remainingTicks, num);
			}
			yield return Toils_General.Do(delegate
			{
				if (HasOptionalThing)
				{
					if (!pawn.IsCarryingThing(OptionalThing))
					{
						Log.ErrorOnce("Interact driver required item (" + OptionalThing.Label + ") but was not carrying it.", 77979685);
						return;
					}
					pawn.carryTracker.DestroyCarriedThing();
				}
				Interactable.Interact(pawn);
			});
		}

		public override string GetReport()
		{
			if (base.CurToilIndex != 1 || ticksLeftThisToil <= 0)
			{
				if (string.IsNullOrEmpty(Interactable.Props.activatingStringPending))
				{
					return base.GetReport();
				}
				return Interactable.Props.activatingStringPending.Formatted(Interactable.parent.LabelShort);
			}
			int ticksToActivate = Interactable.TicksToActivate;
			int num = ((ticksLeftThisToil < 0) ? ticksToActivate : ticksLeftThisToil);
			float num2 = (float)(ticksToActivate - (ticksToActivate - num)) / 60f;
			if (string.IsNullOrEmpty(Interactable.Props.activatingString))
			{
				return base.GetReport();
			}
			return Interactable.Props.activatingString.Formatted(Interactable.parent.LabelShort, num2);
		}

		private Toil WaitForActivate(int remainingTicks, int totalTicks)
		{
			Toil toil = ToilMaker.MakeToil("WaitForActivate").FailOn(() => !Interactable.CanInteract(pawn));
			if (Interactable.Props.maintainProgress)
			{
				toil.WithProgressBar(TargetIndex.A, () => Interactable.progress);
			}
			else
			{
				toil.WithProgressBarToilDelay(TargetIndex.A, remainingTicks);
			}
			Toil toil2 = toil;
			toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
			{
				Interactable.Notify_InteractionStarted();
				toil.actor.pather.StopDead();
			});
			Toil toil3 = toil;
			toil3.tickIntervalAction = (Action<int>)Delegate.Combine(toil3.tickIntervalAction, (Action<int>)delegate
			{
				pawn.rotationTracker.FaceTarget(base.TargetA);
			});
			if (Interactable.Props.maintainProgress)
			{
				Toil toil4 = toil;
				toil4.tickIntervalAction = (Action<int>)Delegate.Combine(toil4.tickIntervalAction, (Action<int>)delegate
				{
					Interactable.progress = 1f - (float)ticksLeftThisToil / (float)totalTicks;
				});
			}
			if (Interactable.Props.forceNormalSpeedOnInteracting)
			{
				Toil toil5 = toil;
				toil5.tickAction = (Action)Delegate.Combine(toil5.tickAction, (Action)delegate
				{
					Find.TickManager.slower.SignalForceNormalSpeed();
				});
			}
			toil.AddFinishAction(delegate
			{
				Interactable.Notify_InteractionEnded();
			});
			toil.handlingFacing = true;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultDuration = remainingTicks;
			return toil;
		}
	}
}
