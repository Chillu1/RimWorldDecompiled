using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ClearSnow : JobDriver
	{
		private float workDone;

		private const float ClearWorkPerSnowDepth = 50f;

		private float TotalNeededWork => 50f * base.Map.snowGrid.GetDepth(base.TargetLocA);

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil clearToil = new Toil();
			clearToil.tickAction = delegate
			{
				float statValue = clearToil.actor.GetStatValue(StatDefOf.GeneralLaborSpeed);
				workDone += statValue;
				if (workDone >= TotalNeededWork)
				{
					base.Map.snowGrid.SetDepth(base.TargetLocA, 0f);
					ReadyForNextToil();
				}
			};
			clearToil.defaultCompleteMode = ToilCompleteMode.Never;
			clearToil.WithEffect(EffecterDefOf.ClearSnow, TargetIndex.A);
			clearToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
			clearToil.WithProgressBar(TargetIndex.A, () => workDone / TotalNeededWork, interpolateBetweenActorAndTarget: true);
			clearToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return clearToil;
		}
	}
}
