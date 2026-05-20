using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_EjectFuel : JobDriver
{
	public const int EjectTicks = 120;

	private CompRefuelable compRefuelable;

	private Thing Target => job.targetA.Thing;

	private CompRefuelable Refuelable => compRefuelable ?? (compRefuelable = job.targetA.Thing.TryGetComp<CompRefuelable>());

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (!Refuelable.CanEjectFuel())
			{
				base.Map.designationManager.DesignationOn(job.targetA.Thing, DesignationDefOf.EjectFuel)?.Delete();
			}
		};
		yield return toil.FailOnDespawnedOrNull(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.EjectFuel).FailOnDespawnedOrNull(TargetIndex.A);
		Toil toil2 = Toils_General.Wait(120, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A)
			.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		BuildingProperties building = Target.def.building;
		if (building != null && building.openingStartedSound != null)
		{
			toil2.PlaySoundAtStart(Target.def.building.openingStartedSound);
		}
		yield return toil2;
		yield return Toils_General.Do(delegate
		{
			base.Map.designationManager.DesignationOn(job.targetA.Thing, DesignationDefOf.EjectFuel)?.Delete();
			Refuelable.EjectFuel();
		});
	}
}
