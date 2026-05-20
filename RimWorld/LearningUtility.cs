using Verse;
using Verse.AI;

namespace RimWorld;

public static class LearningUtility
{
	public const float NeedSatisfiedPerTick = 1.2E-05f;

	public const float StartJobMaxLearning = 0.9f;

	private const float EndJobMaxLearning = 0.999f;

	public const float LearningRateBonusOffset_Blackboard = 0.2f;

	public const int MaxConnectedBlackboards = 3;

	public static float SchoolDeskLearningRate(Thing schoolDesk)
	{
		return 1f + (float)ConnectedBlackboards(schoolDesk) * 0.2f;
	}

	public static int ConnectedBlackboards(Thing desk)
	{
		int num = 0;
		CompAffectedByFacilities compAffectedByFacilities = desk.TryGetComp<CompAffectedByFacilities>();
		if (compAffectedByFacilities != null)
		{
			foreach (Thing item in compAffectedByFacilities.LinkedFacilitiesListForReading)
			{
				if (item.def == ThingDefOf.Blackboard)
				{
					num++;
					if (num >= 3)
					{
						break;
					}
				}
			}
		}
		return num;
	}

	public static float LearningRateFactor(Pawn pawn)
	{
		Job curJob = pawn.CurJob;
		float num = pawn.GetStatValue(StatDefOf.LearningRateFactor);
		if (curJob.targetA.HasThing && curJob.targetA.Thing.def == ThingDefOf.SchoolDesk)
		{
			num *= SchoolDeskLearningRate(curJob.targetA.Thing);
		}
		return num;
	}

	public static bool LearningTickCheckEnd(Pawn pawn, int delta, bool forced = false)
	{
		pawn.needs.learning.Learn(1.2E-05f * LearningRateFactor(pawn) * (float)delta);
		if (LearningSatisfied(pawn) && !forced)
		{
			pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
			return true;
		}
		return false;
	}

	public static bool LearningSatisfied(Pawn pawn)
	{
		return pawn.needs.learning.CurLevel >= 0.999f;
	}
}
