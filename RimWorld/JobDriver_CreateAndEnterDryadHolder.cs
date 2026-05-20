using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_CreateAndEnterDryadHolder : JobDriver
	{
		private const int TicksToCreate = 200;

		private CompTreeConnection TreeComp => job.targetA.Thing.TryGetComp<CompTreeConnection>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => TreeComp.ShouldReturnToTree(pawn));
			yield return Toils_General.Do(delegate
			{
				if (!CellFinder.TryFindRandomCellNear(job.GetTarget(TargetIndex.A).Cell, pawn.Map, 4, (IntVec3 c) => GauranlenUtility.CocoonAndPodCellValidator(c, pawn.Map), out var result))
				{
					Log.Error("Could not find cell to place dryad holder. Dryad=" + pawn.GetUniqueLoadID());
				}
				job.targetB = result;
			});
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			yield return Toils_General.Wait(200).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B);
			yield return EnterToil();
		}

		public abstract Toil EnterToil();
	}
}
