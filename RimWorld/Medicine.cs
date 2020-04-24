using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Medicine : ThingWithComps
	{
		private static List<Hediff> tendableHediffsInTendPriorityOrder = new List<Hediff>();

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		public static int GetMedicineCountToFullyHeal(Pawn pawn)
		{
			int num = 0;
			int num2 = pawn.health.hediffSet.hediffs.Count + 1;
			tendableHediffsInTendPriorityOrder.Clear();
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].TendableNow())
				{
					tendableHediffsInTendPriorityOrder.Add(hediffs[i]);
				}
			}
			TendUtility.SortByTendPriority(tendableHediffsInTendPriorityOrder);
			int num3 = 0;
			while (true)
			{
				num++;
				if (num > num2)
				{
					Log.Error("Too many iterations.");
					break;
				}
				TendUtility.GetOptimalHediffsToTendWithSingleTreatment(pawn, usingMedicine: true, tmpHediffs, tendableHediffsInTendPriorityOrder);
				if (!tmpHediffs.Any())
				{
					break;
				}
				num3++;
				for (int j = 0; j < tmpHediffs.Count; j++)
				{
					tendableHediffsInTendPriorityOrder.Remove(tmpHediffs[j]);
				}
			}
			tmpHediffs.Clear();
			tendableHediffsInTendPriorityOrder.Clear();
			return num3;
		}
	}
}
