using Verse;

namespace RimWorld;

public class CompDrug : ThingComp
{
	public CompProperties_Drug Props => (CompProperties_Drug)props;

	public override void PrePostIngested(Pawn ingester)
	{
		if (!Props.Addictive || !ingester.RaceProps.IsFlesh)
		{
			return;
		}
		HediffDef addictionHediff = Props.chemical.addictionHediff;
		Hediff_Addiction hediff_Addiction = AddictionUtility.FindAddictionHediff(ingester, Props.chemical);
		float num = AddictionUtility.FindToleranceHediff(ingester, Props.chemical)?.Severity ?? 0f;
		if (hediff_Addiction != null)
		{
			hediff_Addiction.Severity += Props.existingAddictionSeverityOffset;
		}
		else
		{
			float num2 = DrugStatsUtility.GetAddictivenessAtTolerance(parent.def, num);
			if (ingester.genes != null)
			{
				num2 *= ingester.genes.AddictionChanceFactor(Props.chemical);
			}
			if (Rand.Value < num2 && num >= Props.minToleranceToAddict)
			{
				ingester.health.AddHediff(addictionHediff);
				if (PawnUtility.ShouldSendNotificationAbout(ingester))
				{
					Find.LetterStack.ReceiveLetter("LetterLabelNewlyAddicted".Translate(Props.chemical.label).CapitalizeFirst(), "LetterNewlyAddicted".Translate(ingester.LabelShort, Props.chemical.label, ingester.Named("PAWN")).AdjustedFor(ingester).CapitalizeFirst(), LetterDefOf.NegativeEvent, ingester);
				}
				AddictionUtility.CheckDrugAddictionTeachOpportunity(ingester);
			}
		}
		if (addictionHediff.chemicalNeed != null && ingester.needs.TryGetNeed(addictionHediff.chemicalNeed, out var need))
		{
			float effect = Props.needLevelOffset;
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(ingester, Props.chemical, ref effect, applyGeneToleranceFactor: false);
			need.CurLevel += effect;
		}
	}

	public override void PostIngested(Pawn ingester)
	{
		if (Props.Addictive && ingester.RaceProps.IsFlesh)
		{
			float num = 1f;
			if (ModsConfig.BiotechActive && ingester.genes != null)
			{
				foreach (Gene item in ingester.genes.GenesListForReading)
				{
					if (item.Active && item.def.chemical == Props.chemical)
					{
						num *= item.def.overdoseChanceFactor;
					}
				}
			}
			if (Rand.Chance(num))
			{
				float num2 = ingester.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose)?.Severity ?? 0f;
				bool flag = false;
				if (ModsConfig.BiotechActive && ingester.genes != null)
				{
					foreach (Gene item2 in ingester.genes.GenesListForReading)
					{
						if (item2 is Gene_ChemicalDependency gene_ChemicalDependency && gene_ChemicalDependency.def.chemical == Props.chemical)
						{
							flag = true;
							break;
						}
					}
				}
				if (num2 < 0.9f && !flag && Rand.Value < Props.largeOverdoseChance)
				{
					float num3 = Rand.Range(0.85f, 0.99f);
					HealthUtility.AdjustSeverity(ingester, HediffDefOf.DrugOverdose, num3 - num2);
					if (ingester.Faction == Faction.OfPlayer)
					{
						Messages.Message("MessageAccidentalOverdose".Translate(ingester.Named("INGESTER"), parent.LabelNoCount, parent.Named("DRUG")), ingester, MessageTypeDefOf.NegativeHealthEvent);
					}
				}
				else
				{
					float num4 = Props.overdoseSeverityOffset.RandomInRange / ingester.BodySize;
					if (num4 > 0f)
					{
						HealthUtility.AdjustSeverity(ingester, HediffDefOf.DrugOverdose, num4);
					}
				}
			}
		}
		if (Props.isCombatEnhancingDrug && !ingester.Dead)
		{
			ingester.mindState.lastTakeCombatEnhancingDrugTick = Find.TickManager.TicksGame;
		}
		if (parent.def.ingestible.drugCategory != DrugCategory.Medical && !ingester.Dead)
		{
			ingester.mindState.lastTakeRecreationalDrugTick = Find.TickManager.TicksGame;
		}
		ingester.drugs?.Notify_DrugIngested(parent);
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.IngestedDrug, ingester.Named(HistoryEventArgsNames.Doer)));
		if (parent.def.ingestible.drugCategory == DrugCategory.Hard)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.IngestedHardDrug, ingester.Named(HistoryEventArgsNames.Doer)));
		}
		if (parent.def.IsNonMedicalDrug)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.IngestedRecreationalDrug, ingester.Named(HistoryEventArgsNames.Doer)));
		}
	}
}
