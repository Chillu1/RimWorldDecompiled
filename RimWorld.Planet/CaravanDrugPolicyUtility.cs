using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanDrugPolicyUtility
	{
		private const int TryTakeScheduledDrugsIntervalTicks = 120;

		public static void CheckTakeScheduledDrugs(Caravan caravan)
		{
			if (caravan.IsHashIntervalTick(120))
			{
				TryTakeScheduledDrugs(caravan);
			}
		}

		public static void TryTakeScheduledDrugs(Caravan caravan)
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				TryTakeScheduledDrugs(pawnsListForReading[i], caravan);
			}
		}

		private static void TryTakeScheduledDrugs(Pawn pawn, Caravan caravan)
		{
			if (pawn.drugs == null)
			{
				return;
			}
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug) && CaravanInventoryUtility.TryGetThingOfDef(caravan, currentPolicy[i].drug, out var thing, out var owner))
				{
					caravan.needs.IngestDrug(pawn, thing, owner);
				}
			}
		}
	}
}
