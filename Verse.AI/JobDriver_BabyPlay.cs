using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public abstract class JobDriver_BabyPlay : JobDriver
	{
		public enum StartingConditions
		{
			Invalid,
			GotoBaby,
			PickupBaby
		}

		protected const float BabyPlayMemoryNeedIncrease = 0.6f;

		protected float roomPlayGainFactor = -1f;

		protected float initialPlayPercentage = -1f;

		protected bool finishedSetup;

		protected const TargetIndex BabyInd = TargetIndex.A;

		protected Pawn Baby => (Pawn)base.TargetThingA;

		protected abstract StartingConditions StartingCondition { get; }

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		public Toil SetPlayPercentage()
		{
			return Toils_General.Do(delegate
			{
				if (initialPlayPercentage == -1f)
				{
					initialPlayPercentage = Baby.needs.play?.CurLevelPercentage ?? 1f;
				}
			});
		}

		public void AddPlayThoughtIfAppropriate()
		{
			if (!Baby.DestroyedOrNull())
			{
				float num = Baby.needs.play?.CurLevelPercentage ?? (-1f);
				if (initialPlayPercentage >= 0f && num - initialPlayPercentage > 0.6f)
				{
					Baby.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.PlayedWithMe, pawn);
				}
			}
		}

		protected abstract IEnumerable<Toil> Play();

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			this.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			AddFailCondition(() => !ChildcareUtility.CanSuckle(Baby, out var _));
			AddFinishAction(delegate
			{
				AddPlayThoughtIfAppropriate();
			});
			SetFinalizerJob((JobCondition condition) => (!finishedSetup) ? null : ChildcareUtility.MakeBringBabyToSafetyJob(pawn, Baby));
			foreach (Toil item in CreateStartingCondition())
			{
				yield return item;
			}
			yield return SetPlayPercentage();
			yield return Toils_General.DoAtomic(delegate
			{
				finishedSetup = true;
			});
			foreach (Toil item2 in Play())
			{
				yield return item2;
			}
			foreach (Toil item3 in JobDriver_PickupToHold.Toils(this))
			{
				yield return item3;
			}
		}

		private IEnumerable<Toil> CreateStartingCondition()
		{
			if (StartingCondition == StartingConditions.PickupBaby)
			{
				foreach (Toil item in JobDriver_PickupToHold.Toils(this, TargetIndex.A, subtractNumTakenFromJobCount: false))
				{
					yield return item;
				}
				yield break;
			}
			if (StartingCondition == StartingConditions.GotoBaby)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
				yield break;
			}
			throw new NotImplementedException(StartingCondition.ToString());
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref initialPlayPercentage, "initialPlayPercentage", 0f);
			Scribe_Values.Look(ref roomPlayGainFactor, "roomPlayGainFactor", 0f);
			Scribe_Values.Look(ref finishedSetup, "finishedSetup", defaultValue: false);
		}
	}
}
