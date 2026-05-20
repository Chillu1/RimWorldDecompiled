using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Execute : JobDriver
{
	protected Pawn Victim => (Pawn)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnAggroMentalState(TargetIndex.A);
		yield return Toils_Interpersonal.GotoPrisoner(pawn, Victim, PrisonerInteractionModeDefOf.Execution).FailOn(() => !Victim.IsPrisonerOfColony || !Victim.guest.PrisonerIsSecure);
		Toil execute = ToilMaker.MakeToil("MakeNewToils");
		execute.initAction = delegate
		{
			ExecutionUtility.DoExecutionByCut(execute.actor, Victim);
			ThoughtUtility.GiveThoughtsForPawnExecuted(Victim, execute.actor, PawnExecutionKind.GenericBrutal);
			TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, pawn, Victim);
		};
		execute.defaultCompleteMode = ToilCompleteMode.Instant;
		execute.activeSkill = () => SkillDefOf.Melee;
		yield return execute;
	}
}
