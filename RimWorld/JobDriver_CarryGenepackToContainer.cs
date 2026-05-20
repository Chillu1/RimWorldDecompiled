using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_CarryGenepackToContainer : JobDriver
{
	private const TargetIndex GenepackInd = TargetIndex.A;

	private const TargetIndex ContainerInd = TargetIndex.B;

	private const int InsertTicks = 30;

	private Thing Container => job.GetTarget(TargetIndex.B).Thing;

	private CompGenepackContainer ContainerComp => Container.TryGetComp<CompGenepackContainer>();

	private Genepack Genepack => (Genepack)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckBiotech("Genepack"))
		{
			yield break;
		}
		this.FailOn(delegate
		{
			CompGenepackContainer containerComp = ContainerComp;
			if (containerComp == null || containerComp.Full)
			{
				return true;
			}
			return (!containerComp.autoLoad && (!containerComp.leftToLoad.Contains(Genepack) || Genepack.targetContainer != Container)) ? true : false;
		});
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
		yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.Touch);
		Toil toil = Toils_General.Wait(30, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B);
		toil.handlingFacing = true;
		yield return toil;
		yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
		{
			Genepack.def.soundDrop.PlayOneShot(SoundInfo.InMap(Container));
			Genepack.targetContainer = null;
			CompGenepackContainer containerComp = ContainerComp;
			containerComp.leftToLoad.Remove(Genepack);
			MoteMaker.ThrowText(Container.DrawPos, pawn.Map, "InsertedThing".Translate($"{containerComp.innerContainer.Count} / {containerComp.Props.maxCapacity}"));
		});
	}
}
