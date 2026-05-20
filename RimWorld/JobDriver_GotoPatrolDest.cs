using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_GotoPatrolDest : JobDriver
{
	private const float WaitAtDestChance = 0.35f;

	private static readonly IntRange WaitAtDestTicks = new IntRange(120, 9600);

	private LocalTargetInfo Destination => job.targetA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, Destination.Cell);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		yield return Toils_General.Wait((!Rand.Chance(0.35f)) ? 1 : WaitAtDestTicks.RandomInRange);
	}
}
