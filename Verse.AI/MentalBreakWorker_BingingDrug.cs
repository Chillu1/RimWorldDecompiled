using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalBreakWorker_BingingDrug : MentalBreakWorker
{
	public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
	{
		float num = base.CommonalityFor(pawn, moodCaused);
		int num2 = BingeableAddictionsCount(pawn);
		if (num2 > 0)
		{
			num *= 1.4f * (float)num2;
		}
		if (moodCaused && pawn.story != null)
		{
			Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
			if (trait != null)
			{
				if (trait.Degree == 1)
				{
					num *= 2.5f;
				}
				else if (trait.Degree == 2)
				{
					num *= 5f;
				}
			}
		}
		return num;
	}

	private int BingeableAddictionsCount(Pawn pawn)
	{
		int num = 0;
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is Hediff_Addiction hediff_Addiction && AddictionUtility.CanBingeOnNow(pawn, hediff_Addiction.Chemical, DrugCategory.Any))
			{
				num++;
			}
		}
		return num;
	}
}
