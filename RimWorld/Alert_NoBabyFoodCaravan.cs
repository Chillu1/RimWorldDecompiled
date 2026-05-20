using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_NoBabyFoodCaravan : Alert
{
	private List<Caravan> caravansReportOut = new List<Caravan>(8);

	public Alert_NoBabyFoodCaravan()
	{
		defaultLabel = "AlertNoBabyFoodCaravan".Translate();
		defaultPriority = AlertPriority.High;
		requireBiotech = true;
	}

	public override TaggedString GetExplanation()
	{
		return "AlertNoBabyFoodCaravanDesc".Translate(Faction.OfPlayer.Named("FACTION"));
	}

	public override AlertReport GetReport()
	{
		return new AlertReport
		{
			active = LowBabyFoodNutrition(caravansReportOut),
			culpritsCaravans = caravansReportOut
		};
	}

	private bool LowBabyFoodNutrition(List<Caravan> foodlessCaravansOut)
	{
		foodlessCaravansOut.Clear();
		foreach (Caravan caravan in Find.WorldObjects.Caravans)
		{
			bool flag = false;
			for (int i = 0; i < caravan.PawnsListForReading.Count; i++)
			{
				Pawn pawn = caravan.PawnsListForReading[i];
				if (ChildcareUtility.CanSuckle(pawn, out var _))
				{
					ChildcareUtility.BreastfeedFailReason? reason2;
					Predicate<Pawn, Pawn> feederPredicate = (Pawn _baby, Pawn _mom) => ChildcareUtility.CanMomBreastfeedBaby(_mom, _baby, out reason2);
					if (!pawn.mindState.AnyAutofeeder(AutofeedMode.Urgent, feederPredicate, caravan.PawnsListForReading) && !pawn.mindState.AnyAutofeeder(AutofeedMode.Childcare, feederPredicate, caravan.PawnsListForReading))
					{
						flag = true;
					}
				}
			}
			if (flag && !CaravanInventoryUtility.AllInventoryItems(caravan).Any((Thing thing) => thing.def.IsNutritionGivingIngestibleForHumanlikeBabies))
			{
				foodlessCaravansOut.Add(caravan);
			}
		}
		return foodlessCaravansOut.Count > 0;
	}
}
