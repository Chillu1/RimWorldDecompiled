using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitSickPawn : JobDriver
	{
		private const TargetIndex PatientInd = TargetIndex.A;

		private const TargetIndex ChairInd = TargetIndex.B;

		private Pawn Patient => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private Thing Chair => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(Patient, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (Chair != null && !pawn.Reserve(Chair, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => !Patient.InBed() || !Patient.Awake());
			if (Chair != null)
			{
				this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			}
			if (Chair != null)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			}
			else
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			}
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				Patient.needs.joy.GainJoy(job.def.joyGainRate * 0.000144f, job.def.joyKind);
				if (pawn.IsHashIntervalTick(320))
				{
					InteractionDef intDef = (Rand.Value < 0.8f) ? InteractionDefOf.Chitchat : InteractionDefOf.DeepTalk;
					pawn.interactions.TryInteractWith(Patient, intDef);
				}
				pawn.rotationTracker.FaceCell(Patient.Position);
				pawn.GainComfortFromCellIfPossible();
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None);
				if (pawn.needs.joy.CurLevelPercentage > 0.9999f && Patient.needs.joy.CurLevelPercentage > 0.9999f)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			};
			toil.handlingFacing = true;
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = job.def.joyDuration;
			yield return toil;
		}
	}
}
