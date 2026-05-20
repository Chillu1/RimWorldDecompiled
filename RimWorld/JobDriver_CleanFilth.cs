using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

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
		Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
		yield return initExtractTargetFromQueue;
		yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue).JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
		Toil clean = ToilMaker.MakeToil("MakeNewToils");
		clean.initAction = delegate
		{
			cleaningWorkDone = 0f;
			totalCleaningWorkDone = 0f;
			totalCleaningWorkRequired = Filth.def.filth.cleaningWorkToReduceThickness * (float)Filth.thickness;
		};
		clean.tickIntervalAction = delegate(int delta)
		{
			Filth filth = Filth;
			float statValueAbstract = filth.Position.GetTerrain(filth.Map).GetStatValueAbstract(StatDefOf.CleaningTimeFactor);
			float num = pawn.GetStatValue(StatDefOf.CleaningSpeed) * (float)delta;
			if (statValueAbstract != 0f)
			{
				num /= statValueAbstract;
			}
			cleaningWorkDone += num;
			totalCleaningWorkDone += num;
			if (cleaningWorkDone > filth.def.filth.cleaningWorkToReduceThickness)
			{
				filth.ThinFilth();
				cleaningWorkDone = 0f;
				if (filth.Destroyed)
				{
					clean.actor.records.Increment(RecordDefOf.MessesCleaned);
					ReadyForNextToil();
				}
			}
		};
		clean.defaultCompleteMode = ToilCompleteMode.Never;
		clean.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
		clean.WithProgressBar(TargetIndex.A, () => totalCleaningWorkDone / totalCleaningWorkRequired, interpolateBetweenActorAndTarget: true);
		clean.PlaySustainerOrSound(delegate
		{
			ThingDef def = Filth.def;
			return (!def.filth.cleaningSound.NullOrUndefined()) ? def.filth.cleaningSound : SoundDefOf.Interact_CleanFilth;
		});
		clean.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
		clean.JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
		clean.JumpIf(() => clean.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing?.Destroyed ?? false, initExtractTargetFromQueue);
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
