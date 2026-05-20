using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Ingest : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (clickedThing.def.ingestible == null || !clickedThing.def.ingestible.showIngestFloatOption)
		{
			return null;
		}
		if (!clickedThing.IngestibleNow || !context.FirstSelectedPawn.RaceProps.CanEverEat(clickedThing.def))
		{
			return null;
		}
		string text = ((!clickedThing.def.ingestible.ingestCommandString.NullOrEmpty()) ? ((string)clickedThing.def.ingestible.ingestCommandString.Formatted(clickedThing.LabelShort)) : ((string)"ConsumeThing".Translate(clickedThing.LabelShort, clickedThing)));
		if (!clickedThing.IsSociallyProper(context.FirstSelectedPawn))
		{
			text = text + ": " + "ReservedForPrisoners".Translate().CapitalizeFirst();
		}
		else if (FoodUtility.MoodFromIngesting(context.FirstSelectedPawn, clickedThing, clickedThing.def) < 0f)
		{
			text = string.Format("{0} ({1})", text, "WarningFoodDisliked".Translate());
		}
		if (!clickedThing.def.IsDrug && !clickedThing.def.ingestible.nonDrugIngestibleWithoutFoodNeed && !context.FirstSelectedPawn.FoodIsSuitable(clickedThing.def))
		{
			return new FloatMenuOption(text + ": " + "FoodNotSuitable".Translate().CapitalizeFirst(), null);
		}
		if (clickedThing.def.IsDrug && !context.FirstSelectedPawn.DrugIsSuitable(clickedThing.def))
		{
			return new FloatMenuOption(text + ": " + "DrugNotSuitable".Translate().CapitalizeFirst(), null);
		}
		if (ModsConfig.IdeologyActive)
		{
			if (clickedThing.def.IsDrug && !new HistoryEvent(HistoryEventDefOf.IngestedDrug, context.FirstSelectedPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out var opt, text) && !PawnUtility.CanTakeDrugForDependency(context.FirstSelectedPawn, clickedThing.def))
			{
				return opt;
			}
			if (clickedThing.def.IsNonMedicalDrug && !new HistoryEvent(HistoryEventDefOf.IngestedRecreationalDrug, context.FirstSelectedPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text) && !PawnUtility.CanTakeDrugForDependency(context.FirstSelectedPawn, clickedThing.def))
			{
				return opt;
			}
			if (clickedThing.def.IsDrug && clickedThing.def.ingestible.drugCategory == DrugCategory.Hard && !new HistoryEvent(HistoryEventDefOf.IngestedHardDrug, context.FirstSelectedPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text))
			{
				return opt;
			}
		}
		if (clickedThing.def.IsNonMedicalDrug && !context.FirstSelectedPawn.CanTakeDrug(clickedThing.def))
		{
			return new FloatMenuOption(text + ": " + TraitDefOf.DrugDesire.DataAtDegree(-1).GetLabelCapFor(context.FirstSelectedPawn), null);
		}
		if (FoodUtility.InappropriateForTitle(clickedThing.def, context.FirstSelectedPawn, allowIfStarving: true))
		{
			return new FloatMenuOption(text + ": " + "FoodBelowTitleRequirements".Translate(context.FirstSelectedPawn.royalty.MostSeniorTitle.def.GetLabelFor(context.FirstSelectedPawn).CapitalizeFirst()).CapitalizeFirst(), null);
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.OnCell, Danger.Deadly))
		{
			return new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(clickedThing, context.FirstSelectedPawn, FoodUtility.WillIngestStackCountOf(context.FirstSelectedPawn, clickedThing.def, FoodUtility.NutritionForEater(context.FirstSelectedPawn, clickedThing)));
		FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
		{
			int maxAmountToPickup2 = FoodUtility.GetMaxAmountToPickup(clickedThing, context.FirstSelectedPawn, FoodUtility.WillIngestStackCountOf(context.FirstSelectedPawn, clickedThing.def, FoodUtility.NutritionForEater(context.FirstSelectedPawn, clickedThing)));
			if (maxAmountToPickup2 != 0)
			{
				clickedThing.SetForbidden(value: false);
				Job job = JobMaker.MakeJob(JobDefOf.Ingest, clickedThing);
				job.count = maxAmountToPickup2;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}, (clickedThing is Corpse) ? MenuOptionPriority.Low : MenuOptionPriority.Default), context.FirstSelectedPawn, clickedThing);
		if (maxAmountToPickup == 0)
		{
			floatMenuOption.action = null;
		}
		return floatMenuOption;
	}
}
