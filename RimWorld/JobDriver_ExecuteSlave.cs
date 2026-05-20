using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExecuteSlave : JobDriver
	{
		protected Pawn Victim => (Pawn)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Execute slave"))
			{
				this.FailOnAggroMentalState(TargetIndex.A);
				this.FailOnForbidden(TargetIndex.A);
				yield return Toils_Interpersonal.GotoSlave(pawn, Victim).FailOn(() => !Victim.IsSlaveOfColony);
				Toil execute = ToilMaker.MakeToil("MakeNewToils");
				execute.initAction = delegate
				{
					ExecutionUtility.DoExecutionByCut(execute.actor, Victim);
					ThoughtUtility.GiveThoughtsForPawnExecuted(Victim, execute.actor, PawnExecutionKind.GenericBrutal);
				};
				execute.defaultCompleteMode = ToilCompleteMode.Instant;
				execute.activeSkill = () => SkillDefOf.Melee;
				yield return execute;
			}
		}
	}
}
