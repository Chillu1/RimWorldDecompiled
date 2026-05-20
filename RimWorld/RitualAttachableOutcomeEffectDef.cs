using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectDef : Def
{
	public Type workerClass = typeof(RitualAttachableOutcomeEffectWorker);

	public List<MemeDef> requiredMemeAny;

	public List<RitualPatternDef> allowedRituals;

	public List<RitualPatternDef> disallowedRituals;

	public FactionDef requiredFaction;

	public bool onlyPositiveOutcomes = true;

	public bool onlyBestOutcome;

	public bool onlyNegativeOutcomes;

	public bool onlyWorstOutcome;

	[MustTranslate]
	public string letterInfoText;

	[MustTranslate]
	public string effectDesc;

	private RitualAttachableOutcomeEffectWorker workerInstance;

	private string requiredMemeList;

	private List<string> appliesToOutcomeListTmp = new List<string>();

	public RitualAttachableOutcomeEffectWorker Worker
	{
		get
		{
			if (workerInstance == null)
			{
				workerInstance = (RitualAttachableOutcomeEffectWorker)Activator.CreateInstance(workerClass);
				workerInstance.def = this;
			}
			return workerInstance;
		}
	}

	public bool AppliesToAllOutcomes
	{
		get
		{
			if (!onlyBestOutcome && !onlyPositiveOutcomes && !onlyNegativeOutcomes)
			{
				return !onlyWorstOutcome;
			}
			return false;
		}
	}

	private string RequiredMemeList
	{
		get
		{
			if (requiredMemeList == null && !requiredMemeAny.NullOrEmpty())
			{
				requiredMemeList = requiredMemeAny.Select((MemeDef m) => m.label.ResolveTags()).ToCommaList();
			}
			return requiredMemeList;
		}
	}

	public bool AppliesToSeveralOutcomes(Precept_Ritual ritual)
	{
		bool flag = false;
		foreach (RitualOutcomePossibility outcomeChance in ritual.outcomeEffect.def.outcomeChances)
		{
			if (AppliesToOutcome(ritual.outcomeEffect.def, outcomeChance))
			{
				if (flag)
				{
					return true;
				}
				flag = true;
			}
		}
		return false;
	}

	public AcceptanceReport CanAttachToRitual(Precept_Ritual ritual)
	{
		if (!requiredMemeAny.NullOrEmpty() && !ritual.ideo.memes.SharesElementWith(requiredMemeAny))
		{
			return "RitualAttachedRewardRequiredMemeAny".Translate() + ": " + RequiredMemeList.CapitalizeFirst();
		}
		if ((!allowedRituals.NullOrEmpty() && !allowedRituals.Contains(ritual.sourcePattern)) || (!disallowedRituals.NullOrEmpty() && disallowedRituals.Contains(ritual.sourcePattern)))
		{
			return "RitualAttachedRewardCantUseWithRitual".Translate(LabelCap, ritual.LabelCap);
		}
		if (requiredFaction != null && Find.FactionManager.FirstFactionOfDef(requiredFaction) == null)
		{
			return "RitualAttachedRewardRequiredFaction".Translate(requiredFaction.LabelCap);
		}
		return true;
	}

	public bool AppliesToOutcome(RitualOutcomeEffectDef effectDef, RitualOutcomePossibility outcomeChance)
	{
		if (!ModLister.CheckIdeology("Attachable ritual reward"))
		{
			return false;
		}
		if (onlyPositiveOutcomes && !outcomeChance.Positive)
		{
			return false;
		}
		if (onlyNegativeOutcomes && outcomeChance.Positive)
		{
			return false;
		}
		if (onlyBestOutcome && outcomeChance != effectDef.BestOutcome)
		{
			return false;
		}
		if (onlyWorstOutcome && outcomeChance != effectDef.WorstOutcome)
		{
			return false;
		}
		return true;
	}

	public string DescriptionForRitualValidated(Precept_Ritual ritual)
	{
		if (!AppliesToAllOutcomes)
		{
			return "RitualAttachedOutcomesInfo".Translate(ritual.shortDescOverride ?? ritual.def.label, AppliesToOutcomesListString(ritual.outcomeEffect.def)) + ", " + effectDesc;
		}
		return description.CapitalizeFirst();
	}

	public string DescriptionForRitualValidated(Precept_Ritual ritual, Map map)
	{
		string text = DescriptionForRitualValidated(ritual);
		AcceptanceReport acceptanceReport = Worker.CanApplyNow(ritual, map);
		if (!acceptanceReport)
		{
			text += " (" + "RitualAttachedOutcomeCantApply".Translate() + ": " + acceptanceReport.Reason + ")";
			text = text.Colorize(ColorLibrary.RedReadable);
		}
		return text;
	}

	public string TooltipForRitual(Precept_Ritual ritual)
	{
		string text = DescriptionForRitualValidated(ritual).ResolveTags();
		if (!requiredMemeAny.NullOrEmpty())
		{
			text = text + "\n\n" + ("UnlockedByMeme".Translate().Resolve() + ":\n").Colorize(ColoredText.TipSectionTitleColor);
			foreach (MemeDef item in requiredMemeAny)
			{
				text = text + "  - " + item.LabelCap.Resolve() + "\n";
			}
		}
		return text;
	}

	public string AppliesToOutcomesListString(RitualOutcomeEffectDef effectDef)
	{
		appliesToOutcomeListTmp.Clear();
		foreach (RitualOutcomePossibility outcomeChance in effectDef.outcomeChances)
		{
			if (AppliesToOutcome(effectDef, outcomeChance))
			{
				appliesToOutcomeListTmp.Add(outcomeChance.label.ToLower());
			}
		}
		return appliesToOutcomeListTmp.ToCommaListOr();
	}

	public string AppliesToOutcomesString(RitualOutcomeEffectDef effectDef)
	{
		if (onlyPositiveOutcomes)
		{
			return "RitualAttachedOutcomes_Positive".Translate();
		}
		if (onlyNegativeOutcomes)
		{
			return "RitualAttachedOutcomes_Negative".Translate();
		}
		if (onlyBestOutcome)
		{
			return effectDef.outcomeChances.MaxBy((RitualOutcomePossibility o) => o.positivityIndex).label;
		}
		if (onlyWorstOutcome)
		{
			return effectDef.outcomeChances.MinBy((RitualOutcomePossibility o) => o.positivityIndex).label;
		}
		throw new NotImplementedException();
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (workerClass == null || workerClass.IsAbstract || workerClass.IsInterface)
		{
			yield return "worker class must be instantiable: " + workerClass;
		}
		if ((onlyPositiveOutcomes && onlyNegativeOutcomes) || (onlyPositiveOutcomes && onlyWorstOutcome) || (onlyNegativeOutcomes && onlyBestOutcome))
		{
			yield return "conflicting outcome positivity configuration";
		}
	}
}
