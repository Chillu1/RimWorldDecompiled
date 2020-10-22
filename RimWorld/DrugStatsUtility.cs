using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DrugStatsUtility
	{
		public static CompProperties_Drug GetDrugComp(ThingDef d)
		{
			return d.GetCompProperties<CompProperties_Drug>();
		}

		public static ChemicalDef GetChemical(ThingDef d)
		{
			return GetDrugComp(d)?.chemical;
		}

		public static NeedDef GetNeed(ThingDef d)
		{
			return GetChemical(d)?.addictionHediff?.causesNeed;
		}

		public static HediffDef GetTolerance(ThingDef d)
		{
			return GetChemical(d)?.toleranceHediff;
		}

		public static IngestionOutcomeDoer_GiveHediff GetDrugHighGiver(ThingDef d)
		{
			if (d.ingestible == null || d.ingestible.outcomeDoers == null)
			{
				return null;
			}
			foreach (IngestionOutcomeDoer outcomeDoer in d.ingestible.outcomeDoers)
			{
				IngestionOutcomeDoer_GiveHediff ingestionOutcomeDoer_GiveHediff;
				if ((ingestionOutcomeDoer_GiveHediff = outcomeDoer as IngestionOutcomeDoer_GiveHediff) != null && typeof(Hediff_High).IsAssignableFrom(ingestionOutcomeDoer_GiveHediff.hediffDef.hediffClass))
				{
					return ingestionOutcomeDoer_GiveHediff;
				}
			}
			return null;
		}

		public static IngestionOutcomeDoer_GiveHediff GetToleranceGiver(ThingDef d)
		{
			if (d.ingestible == null || d.ingestible.outcomeDoers == null)
			{
				return null;
			}
			foreach (IngestionOutcomeDoer outcomeDoer in d.ingestible.outcomeDoers)
			{
				IngestionOutcomeDoer_GiveHediff ingestionOutcomeDoer_GiveHediff;
				if ((ingestionOutcomeDoer_GiveHediff = outcomeDoer as IngestionOutcomeDoer_GiveHediff) != null && ingestionOutcomeDoer_GiveHediff.hediffDef == GetTolerance(d))
				{
					return ingestionOutcomeDoer_GiveHediff;
				}
			}
			return null;
		}

		public static float GetHighOffsetPerDay(ThingDef d)
		{
			IngestionOutcomeDoer_GiveHediff drugHighGiver = GetDrugHighGiver(d);
			if (drugHighGiver == null)
			{
				return 0f;
			}
			return drugHighGiver.hediffDef.CompProps<HediffCompProperties_SeverityPerDay>()?.severityPerDay ?? 0f;
		}

		public static float GetToleranceGain(ThingDef d)
		{
			if (d.ingestible == null || d.ingestible.outcomeDoers == null)
			{
				return 0f;
			}
			HediffDef tolerance = GetTolerance(d);
			if (tolerance != null)
			{
				foreach (IngestionOutcomeDoer outcomeDoer in d.ingestible.outcomeDoers)
				{
					IngestionOutcomeDoer_GiveHediff ingestionOutcomeDoer_GiveHediff;
					if ((ingestionOutcomeDoer_GiveHediff = outcomeDoer as IngestionOutcomeDoer_GiveHediff) != null && ingestionOutcomeDoer_GiveHediff.hediffDef == tolerance)
					{
						return ingestionOutcomeDoer_GiveHediff.severity;
					}
				}
			}
			return 0f;
		}

		public static float GetToleranceOffsetPerDay(ThingDef d)
		{
			HediffDef tolerance = GetTolerance(d);
			if (tolerance == null)
			{
				return 0f;
			}
			return tolerance.CompProps<HediffCompProperties_SeverityPerDay>()?.severityPerDay ?? 0f;
		}

		public static float GetAddictionOffsetPerDay(ThingDef d)
		{
			HediffDef hediffDef = GetChemical(d)?.addictionHediff;
			if (hediffDef == null)
			{
				return 0f;
			}
			return hediffDef.CompProps<HediffCompProperties_SeverityPerDay>()?.severityPerDay ?? 0f;
		}

		public static float GetAddictionNeedCostPerDay(ThingDef d)
		{
			NeedDef need = GetNeed(d);
			if (need != null)
			{
				return d.BaseMarketValue * need.fallPerDay * (1f + (1f - GetDrugComp(d).needLevelOffset));
			}
			return 0f;
		}

		public static float GetSafeDoseInterval(ThingDef d)
		{
			CompProperties_Drug drugComp = GetDrugComp(d);
			if (drugComp == null || !drugComp.Addictive)
			{
				return 0f;
			}
			if (drugComp.addictiveness >= 1f || GetToleranceGiver(d) == null)
			{
				return -1f;
			}
			float num = Mathf.Abs(GetToleranceOffsetPerDay(d));
			return Mathf.Max(drugComp.overdoseSeverityOffset.TrueMax, (num > 0f) ? (GetToleranceGiver(d).severity / num) : (-1f));
		}

		public static string GetSafeDoseIntervalReadout(ThingDef d)
		{
			IngestionOutcomeDoer_GiveHediff toleranceGiver = GetToleranceGiver(d);
			float safeDoseInterval = GetSafeDoseInterval(d);
			float num = ((toleranceGiver != null) ? (GetDrugComp(d).minToleranceToAddict / toleranceGiver.severity) : 0f);
			if (safeDoseInterval == 0f)
			{
				return "AlwaysSafe".Translate();
			}
			if (num < 1f)
			{
				return "NeverSafe".Translate();
			}
			return "PeriodDays".Translate(safeDoseInterval.ToString("F1"));
		}

		public static IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef def)
		{
			CompProperties_Drug drugComp = GetDrugComp(def);
			if (drugComp == null)
			{
				yield break;
			}
			IngestionOutcomeDoer_GiveHediff highGiver = GetDrugHighGiver(def);
			if (highGiver != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Drug, "HighGain".Translate(), highGiver.severity.ToStringPercent(), "Stat_Thing_Drug_HighGainPerDose_Desc".Translate(), 2480);
				float highFall = Mathf.Abs(GetHighOffsetPerDay(def));
				if (highFall > 0f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Drug, "HighFallRate".Translate(), "PerDay".Translate(highFall.ToStringPercent()), "Stat_Thing_Drug_HighFallPerDay_Desc".Translate(), 2470);
					yield return new StatDrawEntry(StatCategoryDefOf.Drug, "HighDuration".Translate(), "PeriodDays".Translate((highGiver.severity / highFall).ToString("F1")), "Stat_Thing_Drug_HighDurationPerDose_Desc".Translate(), 2460);
				}
			}
			if (GetTolerance(def) != null)
			{
				float toleranceGain = GetToleranceGain(def);
				if (toleranceGain > 0f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Drug, "ToleranceGain".Translate(), toleranceGain.ToStringPercent(), "Stat_Thing_Drug_ToleranceGainPerDose_Desc".Translate(), 2450);
				}
				float num = Mathf.Abs(GetToleranceOffsetPerDay(def));
				if (num > 0f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Drug, "ToleranceFallRate".Translate(), "PerDay".Translate(num.ToStringPercent()), "Stat_Thing_Drug_ToleranceFallPerDay_Desc".Translate(), 2440);
				}
			}
			if (drugComp.Addictive)
			{
				HediffDef addictionHediff = GetChemical(def)?.addictionHediff;
				if (addictionHediff != null)
				{
					float num2 = Mathf.Abs(GetAddictionOffsetPerDay(def));
					if (num2 > 0f)
					{
						yield return new StatDrawEntry(valueString: "PeriodDays".Translate((addictionHediff.initialSeverity / num2).ToString("F1")), category: StatCategoryDefOf.DrugAddiction, label: "AddictionRecoveryTime".Translate(), reportText: "Stat_Thing_Drug_AddictionRecoveryTime_Desc".Translate(), displayPriorityWithinCategory: 2395);
					}
					yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionSeverityInitial".Translate(), addictionHediff.initialSeverity.ToStringPercent(), "Stat_Thing_Drug_AddictionSeverityInitial_Desc".Translate(), 2427);
				}
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionNeedFallRate".Translate(), "PerDay".Translate(GetNeed(def).fallPerDay.ToStringPercent()), "Stat_Thing_Drug_AddictionNeedFallRate_Desc".Translate(), 2410);
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionCost".Translate(), "PerDay".Translate(GetAddictionNeedCostPerDay(def).ToStringMoney()), "Stat_Thing_Drug_AddictionCost_Desc".Translate(), 2390);
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionNeedDoseInterval".Translate(), "PeriodDays".Translate((drugComp.needLevelOffset / GetNeed(def).fallPerDay).ToString("F1")), "Stat_Thing_Drug_AddictionNeedDoseInterval_Desc".Translate(), 2400);
				if (drugComp.chemical != null)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Drug, "Chemical".Translate(), drugComp.chemical.LabelCap, "Stat_Thing_Drug_Chemical_Desc".Translate(), 2490);
				}
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "Addictiveness".Translate(), drugComp.addictiveness.ToStringPercent(), "Stat_Thing_Drug_Addictiveness_Desc".Translate(), 2428);
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionNeedOffset".Translate(), drugComp.needLevelOffset.ToStringPercent(), "Stat_Thing_Drug_AddictionNeedOffset_Desc".Translate(), 2420);
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "MinimumToleranceForAddiction".Translate(), drugComp.minToleranceToAddict.ToStringPercent(), "Stat_Thing_Drug_MinToleranceForAddiction_Desc".Translate(), 2437);
				yield return new StatDrawEntry(StatCategoryDefOf.DrugAddiction, "AddictionSeverityPerDose".Translate(), drugComp.existingAddictionSeverityOffset.ToStringPercent(), "Stat_Thing_Drug_AddictionSeverityPerDose_Desc".Translate(), 2424);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Drug, "RandomODChance".Translate(), drugComp.largeOverdoseChance.ToStringPercent(), "Stat_Thing_Drug_RandomODChance_Desc".Translate(), 2380);
			yield return new StatDrawEntry(StatCategoryDefOf.Drug, "SafeDoseInterval".Translate(), GetSafeDoseIntervalReadout(def), "Stat_Thing_Drug_SafeDoseInterval_Desc".Translate(), 2435);
		}
	}
}
