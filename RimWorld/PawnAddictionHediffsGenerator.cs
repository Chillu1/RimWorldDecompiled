using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

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
		int i = 0;
		foreach (ChemicalDef forcedAddiction in pawn.kindDef.forcedAddictions)
		{
			ApplyAddiction(pawn, forcedAddiction);
			i++;
		}
		for (; i < 3; i++)
		{
			if (Rand.Value >= pawn.kindDef.chemicalAddictionChance)
			{
				break;
			}
			if (!allDrugs.Any())
			{
				allDrugs.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Item && x.GetCompProperties<CompProperties_Drug>() != null));
			}
			if (DefDatabase<ChemicalDef>.AllDefsListForReading.Where((ChemicalDef x) => x.canBeAddicted && PossibleWithTechLevel(x, pawn.Faction) && PossibleWithGenes(x, pawn) && !AddictionUtility.IsAddicted(pawn, x)).TryRandomElement(out var result))
			{
				ApplyAddiction(pawn, result);
				continue;
			}
			break;
		}
	}

	private static void ApplyAddiction(Pawn pawn, ChemicalDef chemicalDef)
	{
		Hediff hediff = HediffMaker.MakeHediff(chemicalDef.addictionHediff, pawn);
		hediff.Severity = GeneratedAddictionSeverityRange.RandomInRange;
		pawn.health.AddHediff(hediff);
		if (chemicalDef.toleranceHediff != null && Rand.Value < chemicalDef.onGeneratedAddictedToleranceChance)
		{
			Hediff hediff2 = HediffMaker.MakeHediff(chemicalDef.toleranceHediff, pawn);
			hediff2.Severity = GeneratedToleranceSeverityRange.RandomInRange;
			pawn.health.AddHediff(hediff2);
		}
		if (chemicalDef.onGeneratedAddictedEvents != null)
		{
			foreach (HediffGiver_Event onGeneratedAddictedEvent in chemicalDef.onGeneratedAddictedEvents)
			{
				onGeneratedAddictedEvent.EventOccurred(pawn);
			}
		}
		DoIngestionOutcomeDoers(pawn, chemicalDef);
	}

	private static bool PossibleWithTechLevel(ChemicalDef chemical, Faction faction)
	{
		if (faction == null)
		{
			return true;
		}
		return allDrugs.Any((ThingDef x) => x.GetCompProperties<CompProperties_Drug>().chemical == chemical && (int)x.techLevel <= (int)faction.def.techLevel);
	}

	private static bool PossibleWithGenes(ChemicalDef chemical, Pawn pawn)
	{
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			return Rand.Value < pawn.genes.AddictionChanceFactor(chemical);
		}
		return true;
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
					outcomeDoers[j].DoIngestionOutcome(pawn, null, 1);
				}
			}
		}
	}
}
