using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_OpenStylingStationDialog : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Styling station"))
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_General.Do(delegate
				{
					Find.WindowStack.Add(new Dialog_StylingStation(pawn, job.targetA.Thing));
				});
			}
		}
	}
}
