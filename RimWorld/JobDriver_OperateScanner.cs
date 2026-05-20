using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_OperateScanner : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		CompScanner scannerComp = job.targetA.Thing.TryGetComp<CompScanner>();
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);
		this.FailOn(() => !scannerComp.CanUseNow);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil work = ToilMaker.MakeToil("MakeNewToils");
		work.tickAction = delegate
		{
			Pawn actor = work.actor;
			scannerComp.Used(actor);
			actor.skills.Learn(SkillDefOf.Intellectual, 0.035f);
			actor.GainComfortFromCellIfPossible(1, chairsOnly: true);
		};
		work.PlaySustainerOrSound(scannerComp.Props.soundWorking);
		work.AddFailCondition(() => !scannerComp.CanUseNow);
		work.defaultCompleteMode = ToilCompleteMode.Never;
		work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		work.activeSkill = () => SkillDefOf.Intellectual;
		yield return work;
	}
}
