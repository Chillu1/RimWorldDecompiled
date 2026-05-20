using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_ApplyTechprint : JobDriver
{
	private const TargetIndex ResearchBenchInd = TargetIndex.A;

	private const TargetIndex TechprintInd = TargetIndex.B;

	private const TargetIndex HaulCell = TargetIndex.C;

	private const int Duration = 600;

	protected Building_ResearchBench ResearchBench => (Building_ResearchBench)job.GetTarget(TargetIndex.A).Thing;

	protected Thing Techprint => job.GetTarget(TargetIndex.B).Thing;

	protected CompTechprint TechprintComp => Techprint.TryGetComp<CompTechprint>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(ResearchBench, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(Techprint, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);
		yield return Toils_General.DoAtomic(delegate
		{
			job.count = 1;
		});
		yield return Toils_Reserve.Reserve(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: false);
		yield return Toils_General.Wait(600).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
			.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
			.WithProgressBarToilDelay(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			Find.ResearchManager.ApplyTechprint(TechprintComp.Props.project, pawn);
			Techprint.Destroy();
			SoundDefOf.TechprintApplied.PlayOneShotOnCamera();
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}
}
