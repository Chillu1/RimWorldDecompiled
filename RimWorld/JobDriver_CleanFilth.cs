using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_CleanFilth : JobDriver
	{
		private float cleaningWorkDone;

		private float totalCleaningWorkDone;

		private float totalCleaningWorkRequired;

		private const TargetIndex FilthInd = TargetIndex.A;

		private Filth Filth => (Filth)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_CleanFilth jobDriver_CleanFilth = this;
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue).JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
			Toil clean = new Toil();
			clean.initAction = delegate
			{
				jobDriver_CleanFilth.cleaningWorkDone = 0f;
				jobDriver_CleanFilth.totalCleaningWorkDone = 0f;
				jobDriver_CleanFilth.totalCleaningWorkRequired = jobDriver_CleanFilth.Filth.def.filth.cleaningWorkToReduceThickness * (float)jobDriver_CleanFilth.Filth.thickness;
			};
			clean.tickAction = delegate
			{
				Filth filth = jobDriver_CleanFilth.Filth;
				jobDriver_CleanFilth.cleaningWorkDone += 1f;
				jobDriver_CleanFilth.totalCleaningWorkDone += 1f;
				if (jobDriver_CleanFilth.cleaningWorkDone > filth.def.filth.cleaningWorkToReduceThickness)
				{
					filth.ThinFilth();
					jobDriver_CleanFilth.cleaningWorkDone = 0f;
					if (filth.Destroyed)
					{
						clean.actor.records.Increment(RecordDefOf.MessesCleaned);
						jobDriver_CleanFilth.ReadyForNextToil();
					}
				}
			};
			clean.defaultCompleteMode = ToilCompleteMode.Never;
			clean.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
			clean.WithProgressBar(TargetIndex.A, () => jobDriver_CleanFilth.totalCleaningWorkDone / jobDriver_CleanFilth.totalCleaningWorkRequired, interpolateBetweenActorAndTarget: true);
			clean.PlaySustainerOrSound(delegate
			{
				ThingDef def = jobDriver_CleanFilth.Filth.def;
				return (!def.filth.cleaningSound.NullOrUndefined()) ? def.filth.cleaningSound : SoundDefOf.Interact_CleanFilth;
			});
			clean.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
			clean.JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
			yield return clean;
			yield return Toils_Jump.Jump(initExtractTargetFromQueue);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref cleaningWorkDone, "cleaningWorkDone", 0f);
			Scribe_Values.Look(ref totalCleaningWorkDone, "totalCleaningWorkDone", 0f);
			Scribe_Values.Look(ref totalCleaningWorkRequired, "totalCleaningWorkRequired", 0f);
		}
	}
}
