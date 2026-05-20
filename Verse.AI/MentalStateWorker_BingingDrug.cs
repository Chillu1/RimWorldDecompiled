using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalStateWorker_BingingDrug : MentalStateWorker
{
	public override bool StateCanOccur(Pawn pawn)
	{
		if (!base.StateCanOccur(pawn))
		{
			return false;
		}
		if (!pawn.Spawned)
		{
			return false;
		}
		List<ChemicalDef> allDefsListForReading = DefDatabase<ChemicalDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (AddictionUtility.CanBingeOnNow(pawn, allDefsListForReading[i], def.drugCategory))
			{
				return true;
			}
			if (def.drugCategory == DrugCategory.Hard && AddictionUtility.CanBingeOnNow(pawn, allDefsListForReading[i], DrugCategory.Social))
			{
				return true;
			}
		}
		return false;
	}
}
