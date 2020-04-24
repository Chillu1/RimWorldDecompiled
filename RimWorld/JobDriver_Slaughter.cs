using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Slaughter : JobDriver
	{
		public const int SlaughterDuration = 180;

		protected Pawn Victim => (Pawn)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Slaughter);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.WaitWith(TargetIndex.A, 180, useProgressBar: true);
			yield return Toils_General.Do(delegate
			{
				ExecutionUtility.DoExecutionByCut(pawn, Victim);
				pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
				if (pawn.InMentalState)
				{
					pawn.MentalState.Notify_SlaughteredAnimal();
				}
			});
		}
	}
}
