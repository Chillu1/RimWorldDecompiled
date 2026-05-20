using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ReleaseAnimalToWild : JobDriver
{
	private const TargetIndex AnimalInd = TargetIndex.A;

	private const int WaitForTicks = 180;

	protected Pawn Animal => (Pawn)job.targetA.Thing;

	public static bool TryFindClosestOutsideCell(IntVec3 root, Map map, TraverseParms traverseParms, Pawn pawn, out IntVec3 cell)
	{
		cell = default(IntVec3);
		if (CellFinder.TryFindClosestRegionWith(root.GetRegion(map), traverseParms, (Region r) => r.District.TouchesMapEdge, 99999, out var result))
		{
			float num = float.PositiveInfinity;
			foreach (IntVec3 cell2 in result.Cells)
			{
				if (cell2.Standable(map) && (pawn == null || !cell2.IsForbidden(pawn)))
				{
					float num2 = cell2.DistanceToSquared(root);
					if (num2 < num)
					{
						num = num2;
						cell = cell2;
					}
				}
			}
			return num != float.PositiveInfinity;
		}
		return false;
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => Animal.Dead);
		this.FailOn(() => Animal.Faction != Faction.OfPlayer);
		this.FailOn(() => Animal.InAggroMentalState);
		this.FailOn(() => Animal.MapHeld.designationManager.DesignationOn(Animal, DesignationDefOf.ReleaseAnimalToWild) == null);
		yield return Toils_Reserve.Reserve(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
		Toil waitToil = Toils_General.WaitWith(TargetIndex.A, 180).WithProgressBarToilDelay(TargetIndex.A);
		yield return Toils_Jump.JumpIf(waitToil, () => Animal.Position.GetRegion(base.Map).District.TouchesMapEdge);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			if (!TryFindClosestOutsideCell(actor.Position, actor.Map, TraverseParms.For(actor), actor, out var cell))
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				actor.pather.StartPath(cell, PathEndMode.OnCell);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		yield return toil;
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, storageMode: false);
		yield return waitToil;
		yield return Toils_General.Do(delegate
		{
			ReleaseAnimalToWildUtility.DoReleaseAnimal(Animal, pawn);
		});
	}
}
