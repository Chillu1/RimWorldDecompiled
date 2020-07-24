using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Execute : JobDriver
	{
		protected Pawn Victim => (Pawn)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_Execute jobDriver_Execute = this;
			this.FailOnAggroMentalState(TargetIndex.A);
			yield return Toils_Interpersonal.GotoPrisoner(pawn, Victim, PrisonerInteractionModeDefOf.Execution).FailOn(() => !jobDriver_Execute.Victim.IsPrisonerOfColony || !jobDriver_Execute.Victim.guest.PrisonerIsSecure);
			Toil execute = new Toil();
			execute.initAction = delegate
			{
				ExecutionUtility.DoExecutionByCut(execute.actor, jobDriver_Execute.Victim);
				ThoughtUtility.GiveThoughtsForPawnExecuted(jobDriver_Execute.Victim, PawnExecutionKind.GenericBrutal);
				TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, jobDriver_Execute.pawn, jobDriver_Execute.Victim);
			};
			execute.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return execute;
		}
	}
}
