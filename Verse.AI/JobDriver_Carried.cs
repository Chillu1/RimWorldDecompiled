using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_Carried : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		AddEndCondition(() => (!pawn.Spawned) ? JobCondition.Ongoing : JobCondition.Succeeded);
		yield return CarryToil();
	}

	private static Toil CarryToil()
	{
		Toil toil = ToilMaker.MakeToil("CarryToil");
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
	}

	public override string GetReport()
	{
		return GetReport(pawn, pawn.SpawnedParentOrMe);
	}

	public static string GetReport(Pawn pawn, Thing spawnedParentOrMe)
	{
		if (spawnedParentOrMe == null || spawnedParentOrMe == pawn)
		{
			return "";
		}
		if (spawnedParentOrMe is Pawn pawn2)
		{
			if (ModsConfig.AnomalyActive && pawn2.CurJobDef == JobDefOf.DevourerDigest)
			{
				return "DigestedBy".Translate(pawn2);
			}
			return "CarriedBy".Translate(pawn2);
		}
		if (spawnedParentOrMe is PawnFlyer)
		{
			return spawnedParentOrMe.LabelCap + ".";
		}
		return "InContainer".Translate(spawnedParentOrMe);
	}
}
