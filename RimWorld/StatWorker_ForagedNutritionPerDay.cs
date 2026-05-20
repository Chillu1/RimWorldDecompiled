using System.Text;
using Verse;

namespace RimWorld;

public class StatWorker_ForagedNutritionPerDay : StatWorker
{
	private const float ForgeBodySizeFactor = 0.6f;

	public override float GetBaseValueFor(StatRequest req)
	{
		Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
		float result = base.GetBaseValueFor(req);
		if (ModsConfig.OdysseyActive && pawn != null)
		{
			Pawn_TrainingTracker training = pawn.training;
			if (training != null && training.HasLearned(TrainableDefOf.Forage))
			{
				result = pawn.BodySize * 0.6f;
			}
		}
		return result;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
		StringBuilder stringBuilder = new StringBuilder();
		float num = GetBaseValueFor(req);
		if (ModsConfig.OdysseyActive && pawn != null && pawn.IsAnimal)
		{
			Pawn_TrainingTracker training = pawn.training;
			if (training != null && training.HasLearned(TrainableDefOf.Forage))
			{
				num = pawn.BodySize * 0.6f;
				stringBuilder.AppendLine(string.Format("{0}: {1}", "ForageTrained".Translate(), num.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense)));
				stringBuilder.AppendLine();
				goto IL_00e8;
			}
		}
		if (num != 0f || stat.showZeroBaseValue)
		{
			stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(num, numberSense));
			stringBuilder.AppendLine();
		}
		goto IL_00e8;
		IL_00e8:
		GetOffsetsAndFactorsExplanation(req, stringBuilder, num);
		return stringBuilder.ToString();
	}

	public override bool ShouldShowFor(StatRequest req)
	{
		Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
		if (pawn != null && pawn.IsAnimal && pawn.training != null)
		{
			if (ModsConfig.OdysseyActive)
			{
				return pawn.training.HasLearned(TrainableDefOf.Forage);
			}
			return false;
		}
		return base.ShouldShowFor(req);
	}
}
