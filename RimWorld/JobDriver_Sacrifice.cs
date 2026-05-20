using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_Sacrifice : JobDriver
{
	private const TargetIndex VictimIndex = TargetIndex.A;

	private const TargetIndex StandingIndex = TargetIndex.B;

	protected Pawn Victim => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Sacrifice"))
		{
			yield break;
		}
		this.FailOnDestroyedOrNull(TargetIndex.A);
		Pawn victim = Victim;
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
		yield return Toils_General.Wait(35);
		Toil execute = ToilMaker.MakeToil("MakeNewToils");
		execute.initAction = delegate
		{
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob is LordJob_Ritual lordJob_Ritual)
			{
				lordJob_Ritual.pawnsDeathIgnored.Add(victim);
			}
			ExecutionUtility.DoExecutionByCut(pawn, victim, 0, spawnBlood: false);
			ThoughtUtility.GiveThoughtsForPawnExecuted(victim, pawn, PawnExecutionKind.GenericBrutal);
			TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, pawn, victim);
			victim.health.killedByRitual = true;
		};
		execute.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return Toils_Reserve.Release(TargetIndex.A);
		yield return execute;
	}
}
