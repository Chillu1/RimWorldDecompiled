using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Alert_LowBabyFood : Alert
{
	private const float NutritionPerDayPerBaby = 0.25f;

	private const float NutritionThresholdPerBaby = 1f;

	private List<Pawn> babiesOutReport = new List<Pawn>(8);

	private List<Pawn> babiesOutExplanation = new List<Pawn>(8);

	private string babyFoodList;

	public Alert_LowBabyFood()
	{
		defaultLabel = "AlertLowBabyFood".Translate();
		defaultPriority = AlertPriority.High;
		requireBiotech = true;
		babyFoodList = (from def in DefDatabase<ThingDef>.AllDefs
			where def.IsNutritionGivingIngestible && def.ingestible.HumanEdible && def.ingestible.babiesCanIngest
			select def.label).ToCommaListOr();
	}

	public override TaggedString GetExplanation()
	{
		if (!LowBabyFoodNutrition(babiesOutExplanation, out var babyFoodNutrition))
		{
			return string.Empty;
		}
		return "AlertLowBabyFoodDesc".Translate(babiesOutExplanation.Count.ToStringCached(), Mathf.FloorToInt(babyFoodNutrition.Value / (0.25f * (float)babiesOutExplanation.Count)).ToStringCached(), babyFoodList);
	}

	public override AlertReport GetReport()
	{
		if ((float)Find.TickManager.TicksGame < 150000f)
		{
			return false;
		}
		float? babyFoodNutrition;
		return new AlertReport
		{
			active = LowBabyFoodNutrition(babiesOutReport, out babyFoodNutrition),
			culpritsPawns = babiesOutReport
		};
	}

	private bool LowBabyFoodNutrition(List<Pawn> lowFoodBabiesOut, out float? babyFoodNutrition)
	{
		foreach (Map map in Find.Maps)
		{
			using (new ProfilerBlock("Populate lowFoodBabiesOut"))
			{
				lowFoodBabiesOut.Clear();
				List<Pawn> freeColonistsAndPrisoners;
				using (new ProfilerBlock("FreeColonistsAndPrisoners"))
				{
					freeColonistsAndPrisoners = map.mapPawns.FreeColonistsAndPrisoners;
				}
				for (int i = 0; i < freeColonistsAndPrisoners.Count; i++)
				{
					Pawn pawn = freeColonistsAndPrisoners[i];
					if (ChildcareUtility.CanSuckle(pawn, out var _) && pawn.SpawnedParentOrMe is Pawn)
					{
						ChildcareUtility.BreastfeedFailReason? reason2;
						Predicate<Pawn, Pawn> feederPredicate = (Pawn _baby, Pawn _mom) => ChildcareUtility.CanMomBreastfeedBaby(_mom, _baby, out reason2) && ChildcareUtility.CanHaulBabyToMomNow(_mom, _mom, _baby, ignoreOtherReservations: true, out reason2);
						if (!pawn.mindState.AnyAutofeeder(AutofeedMode.Urgent, feederPredicate) && !pawn.mindState.AnyAutofeeder(AutofeedMode.Childcare, feederPredicate))
						{
							lowFoodBabiesOut.Add(pawn);
						}
					}
				}
			}
			if (lowFoodBabiesOut.Count > 0)
			{
				float totalHumanBabyEdibleNutrition = map.resourceCounter.TotalHumanBabyEdibleNutrition;
				if (totalHumanBabyEdibleNutrition < 1f * (float)lowFoodBabiesOut.Count)
				{
					babyFoodNutrition = totalHumanBabyEdibleNutrition;
					return true;
				}
			}
		}
		babyFoodNutrition = null;
		return false;
	}
}
