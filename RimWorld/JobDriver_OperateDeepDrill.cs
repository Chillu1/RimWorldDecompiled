using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_OperateDeepDrill : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
			this.FailOn(() => !job.targetA.Thing.TryGetComp<CompDeepDrill>().CanDrillNow());
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil work = new Toil();
			work.tickAction = delegate
			{
				Pawn actor = work.actor;
				((Building)actor.CurJob.targetA.Thing).GetComp<CompDeepDrill>().DrillWorkDone(actor);
				actor.skills.Learn(SkillDefOf.Mining, 0.065f);
			};
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			work.activeSkill = (() => SkillDefOf.Mining);
			yield return work;
		}
	}
}
