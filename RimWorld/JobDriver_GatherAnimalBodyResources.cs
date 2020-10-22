using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_GatherAnimalBodyResources : JobDriver
	{
		private float gatherProgress;

		protected const TargetIndex AnimalInd = TargetIndex.A;

		protected abstract float WorkTotal
		{
			get;
		}

		protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref gatherProgress, "gatherProgress", 0f);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil wait = new Toil();
			wait.initAction = delegate
			{
				Pawn actor2 = wait.actor;
				Pawn pawn2 = (Pawn)job.GetTarget(TargetIndex.A).Thing;
				actor2.pather.StopDead();
				PawnUtility.ForceWait(pawn2, 15000, null, maintainPosture: true);
			};
			wait.tickAction = delegate
			{
				Pawn actor = wait.actor;
				actor.skills.Learn(SkillDefOf.Animals, 0.13f);
				gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed);
				if (gatherProgress >= WorkTotal)
				{
					GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A)).Gathered(pawn);
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			};
			wait.AddFinishAction(delegate
			{
				Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
				if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			});
			wait.FailOnDespawnedOrNull(TargetIndex.A);
			wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			wait.AddEndCondition(() => GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A)).ActiveAndFull ? JobCondition.Ongoing : JobCondition.Incompletable);
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			wait.WithProgressBar(TargetIndex.A, () => gatherProgress / WorkTotal);
			wait.activeSkill = () => SkillDefOf.Animals;
			yield return wait;
		}
	}
}
