using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayEgg : JobDriver
	{
		private const int LayEgg = 500;

		private const TargetIndex LaySpotInd = TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_General.Wait(500);
			yield return Toils_General.Do(delegate
			{
				GenSpawn.Spawn(pawn.GetComp<CompEggLayer>().ProduceEgg(), pawn.Position, base.Map).SetForbiddenIfOutsideHomeArea();
			});
		}
	}
}
