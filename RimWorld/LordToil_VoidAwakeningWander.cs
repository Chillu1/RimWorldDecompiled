using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_VoidAwakeningWander : LordToil
{
	private const float WanderRadius = 12f;

	private const float NextDefendPointRadius = 50f;

	private const int SwitchDefendPointInterval = 1800;

	public override void UpdateAllDuties()
	{
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			UpdateDefendPoint(ownedPawn);
		}
	}

	public override void LordToilTick()
	{
		for (int num = lord.ownedPawns.Count - 1; num >= 0; num--)
		{
			Pawn pawn = lord.ownedPawns[num];
			if (pawn.IsHashIntervalTick(1800))
			{
				UpdateDefendPoint(pawn);
			}
			if (pawn.mindState.enemyTarget != null)
			{
				lord.RemovePawn(pawn);
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	private void UpdateDefendPoint(Pawn pawn)
	{
		if (pawn.mindState != null && CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, 50f, TraverseParms.For(pawn), null, null, out var result))
		{
			pawn.mindState.duty = new PawnDuty(DutyDefOf.VoidAwakeningWander, result);
			pawn.mindState.duty.wanderRadius = 12f;
		}
	}
}
