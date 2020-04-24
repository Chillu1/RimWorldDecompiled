using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RelaxAlone : JobDriver
	{
		private Rot4 faceDir = Rot4.Invalid;

		private const TargetIndex SpotOrBedInd = TargetIndex.A;

		private bool FromBed => job.GetTarget(TargetIndex.A).HasThing;

		public override bool CanBeginNowWhileLyingDown()
		{
			if (FromBed)
			{
				return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
			}
			return false;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (FromBed)
			{
				if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, ((Building_Bed)job.GetTarget(TargetIndex.A).Thing).SleepingSlotsCount, 0, null, errorOnFailed))
				{
					return false;
				}
			}
			else if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil;
			if (FromBed)
			{
				this.KeepLyingDown(TargetIndex.A);
				yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
				yield return Toils_Bed.GotoBed(TargetIndex.A);
				toil = Toils_LayDown.LayDown(TargetIndex.A, hasBed: true, lookForOtherJobs: false);
				toil.AddFailCondition(() => !pawn.Awake());
			}
			else
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
				toil = new Toil();
				toil.initAction = delegate
				{
					faceDir = (job.def.faceDir.IsValid ? job.def.faceDir : Rot4.Random);
				};
				toil.handlingFacing = true;
			}
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = job.def.joyDuration;
			toil.AddPreTickAction(delegate
			{
				if (faceDir.IsValid)
				{
					pawn.rotationTracker.FaceCell(pawn.Position + faceDir.FacingCell);
				}
				pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(pawn);
			});
			yield return toil;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref faceDir, "faceDir");
		}
	}
}
