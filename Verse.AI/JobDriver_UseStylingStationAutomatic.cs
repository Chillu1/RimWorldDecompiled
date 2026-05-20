using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_UseStylingStationAutomatic : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Styling station"))
			{
				this.FailOn(() => !pawn.style.LookChangeDesired);
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_StyleChange.SetupLookChangeData();
				yield return Toils_StyleChange.DoLookChange(TargetIndex.A, pawn);
				yield return Toils_StyleChange.FinalizeLookChange();
			}
		}
	}
}
