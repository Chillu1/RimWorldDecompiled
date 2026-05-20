using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_StudyItem : JobDriver
{
	private const int JobEndInterval = 4000;

	protected const TargetIndex ThingToStudyIndex = TargetIndex.A;

	protected const TargetIndex ResearchBenchInd = TargetIndex.B;

	protected const TargetIndex HaulCell = TargetIndex.C;

	protected Thing ThingToStudy => base.TargetThingA;

	protected bool CanStudyInPlace => ThingToStudy.TryGetComp<CompAnalyzable>()?.Props.canStudyInPlace ?? false;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(ThingToStudy, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (!CanStudyInPlace)
		{
			if (!pawn.Reserve(base.TargetB, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (!pawn.Reserve(base.TargetC, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		bool requiresResearchBench = !CanStudyInPlace;
		if (requiresResearchBench)
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			this.FailOnBurningImmobile(TargetIndex.B);
			yield return Toils_General.DoAtomic(delegate
			{
				job.count = 1;
			});
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		if (requiresResearchBench)
		{
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: false);
		}
		foreach (Toil studyToil in GetStudyToils())
		{
			yield return studyToil;
		}
	}

	protected virtual IEnumerable<Toil> GetStudyToils()
	{
		Toil study = ToilMaker.MakeToil("GetStudyToils");
		CompStudiable comp = ThingToStudy.TryGetComp<CompStudiable>();
		study.tickIntervalAction = delegate(int delta)
		{
			Pawn actor = study.actor;
			float num = 0.08f;
			if (!actor.WorkTypeIsDisabled(WorkTypeDefOf.Research))
			{
				num = actor.GetStatValue(StatDefOf.ResearchSpeed);
			}
			num *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor) * (float)delta;
			comp.Study(actor, num);
			if (comp.Completed)
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		};
		study.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		study.WithProgressBar(TargetIndex.A, () => comp.ProgressPercent);
		study.defaultCompleteMode = ToilCompleteMode.Delay;
		study.defaultDuration = 4000;
		study.activeSkill = () => SkillDefOf.Intellectual;
		yield return study;
	}
}
