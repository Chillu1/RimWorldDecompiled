using Verse;

namespace RimWorld;

public static class NuzzleUtility
{
	public static float GetNuzzleMTBHours(Pawn pawn)
	{
		float num = pawn.RaceProps.nuzzleMtbHours;
		if (ModsConfig.OdysseyActive)
		{
			Pawn_TrainingTracker training = pawn.training;
			if (training != null && training.HasLearned(TrainableDefOf.Comfort))
			{
				num *= 0.75f;
			}
		}
		return num;
	}

	public static int GetNuzzleStageIndex(Pawn pawn)
	{
		if (ModsConfig.OdysseyActive)
		{
			Pawn_TrainingTracker training = pawn.training;
			if (training != null && training.HasLearned(TrainableDefOf.Comfort))
			{
				return 1;
			}
		}
		return 0;
	}
}
