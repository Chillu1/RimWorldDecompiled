using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnAddictionHediffsGenerator
	{
		private static List<ThingDef> allDrugs = new List<ThingDef>();

		private const int MaxAddictions = 3;

		private static readonly FloatRange GeneratedAddictionSeverityRange = new FloatRange(0.6f, 1f);

		private static readonly FloatRange GeneratedToleranceSeverityRange = new FloatRange(0.1f, 0.9f);

		public static void GenerateAddictionsAndTolerancesFor(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh || !pawn.RaceProps.Humanlike || pawn.IsTeetotaler())
			{
				return;
			}
			allDrugs.Clear();
			for (int i = 0; i < 3; i++)
			{
				if (Rand.Value >= pawn.kindDef.chemicalAddictionChance)
				{
					break;
				}
				if (!allDrugs.Any())
				{
					allDrugs.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Item && x.GetCompProperties<CompProperties_Drug>() != null));
				}
				if (!DefDatabase<ChemicalDef>.AllDefsListForReading.Where((ChemicalDef x) => PossibleWithTechLevel(x, pawn.Faction) && !AddictionUtility.IsAddicted(pawn, x)).TryRandomElement(out var result))
				{
					break;
				}
				Hediff hediff = HediffMaker.MakeHediff(result.addictionHediff, pawn);
				hediff.Severity = GeneratedAddictionSeverityRange.RandomInRange;
				pawn.health.AddHediff(hediff);
				if (result.toleranceHediff != null && Rand.Value < result.onGeneratedAddictedToleranceChance)
				{
					Hediff hediff2 = HediffMaker.MakeHediff(result.toleranceHediff, pawn);
					hediff2.Severity = GeneratedToleranceSeverityRange.RandomInRange;
					pawn.health.AddHediff(hediff2);
				}
				if (result.onGeneratedAddictedEvents != null)
				{
					foreach (HediffGiver_Event onGeneratedAddictedEvent in result.onGeneratedAddictedEvents)
					{
						onGeneratedAddictedEvent.EventOccurred(pawn);
					}
				}
				DoIngestionOutcomeDoers(pawn, result);
			}
		}

		private static bool PossibleWithTechLevel(ChemicalDef chemical, Faction faction)
		{
			if (faction == null)
			{
				return true;
			}
			return allDrugs.Any((ThingDef x) => x.GetCompProperties<CompProperties_Drug>().chemical == chemical && (int)x.techLevel <= (int)faction.def.techLevel);
		}

		private static void DoIngestionOutcomeDoers(Pawn pawn, ChemicalDef chemical)
		{
			for (int i = 0; i < allDrugs.Count; i++)
			{
				if (allDrugs[i].GetCompProperties<CompProperties_Drug>().chemical != chemical)
				{
					continue;
				}
				List<IngestionOutcomeDoer> outcomeDoers = allDrugs[i].ingestible.outcomeDoers;
				for (int j = 0; j < outcomeDoers.Count; j++)
				{
					if (outcomeDoers[j].doToGeneratedPawnIfAddicted)
					{
						outcomeDoers[j].DoIngestionOutcome(pawn, null);
					}
				}
			}
		}
	}
}
