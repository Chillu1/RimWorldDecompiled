using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_EmptyThingContainer : JobDriver
{
	protected const TargetIndex ContainerInd = TargetIndex.A;

	protected const TargetIndex ContentsInd = TargetIndex.B;

	protected const TargetIndex StoreCellInd = TargetIndex.C;

	private const int OpenTicks = 120;

	protected virtual PathEndMode ContainerPathEndMode
	{
		get
		{
			if (!job.GetTarget(TargetIndex.A).Thing.def.hasInteractionCell)
			{
				return PathEndMode.Touch;
			}
			return PathEndMode.InteractionCell;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		Pawn p = pawn;
		LocalTargetInfo target = job.GetTarget(TargetIndex.A);
		Job obj = job;
		bool errorOnFailed2 = errorOnFailed;
		if (p.Reserve(target, obj, 1, -1, job.def.containerReservationLayer, errorOnFailed2))
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		CompThingContainer comp;
		yield return Toils_Goto.GotoThing(TargetIndex.A, ContainerPathEndMode).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A)
			.FailOn(() => job.GetTarget(TargetIndex.A).Thing.TryGetComp(out comp) && comp.Empty);
		yield return Toils_General.WaitWhileExtractingContents(TargetIndex.A, TargetIndex.B, 120);
		yield return Toils_General.Do(delegate
		{
			if (base.TargetThingA.TryGetInnerInteractableThingOwner().TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near))
			{
				base.TargetThingA.TryGetComp<CompThingContainer>()?.Props.dropEffecterDef?.Spawn(base.TargetThingA, base.Map).Cleanup();
			}
		});
		yield return Toils_Reserve.Reserve(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
		yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: true, tryStoreInSameStorageIfSpotCantHoldWholeStack: true);
	}
}
