using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExecuteGuiltyColonist : JobDriver
	{
		protected Pawn Victim => (Pawn)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnAggroMentalState(TargetIndex.A);
			yield return Toils_Interpersonal.GotoGuiltyColonist(pawn, Victim).FailOn(() => !Victim.IsColonist || !Victim.guilt.IsGuilty || !Victim.guilt.awaitingExecution);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				Pawn victim = Victim;
				ExecutionUtility.DoExecutionByCut(pawn, victim);
				ThoughtUtility.GiveThoughtsForPawnExecuted(victim, pawn, PawnExecutionKind.GenericBrutal);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.activeSkill = () => SkillDefOf.Melee;
			yield return toil;
		}
	}
}
