using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ChangeTreeMode : JobDriver
	{
		private const int WaitTicks = 120;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.WaitWith(TargetIndex.A, 120, useProgressBar: true);
			yield return Toils_General.Do(delegate
			{
				job.targetA.Thing.TryGetComp<CompTreeConnection>().FinalizeMode();
			}).PlaySustainerOrSound(SoundDefOf.DryadCasteSet);
			yield return Toils_General.Wait(60, TargetIndex.A);
		}
	}
}
