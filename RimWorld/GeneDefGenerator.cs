using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class GeneDefGenerator
{
	public static IEnumerable<GeneDef> ImpliedGeneDefs(bool hotReload = false)
	{
		if (!ModsConfig.BiotechActive)
		{
			yield break;
		}
		foreach (GeneTemplateDef g in DefDatabase<GeneTemplateDef>.AllDefs)
		{
			switch (g.geneTemplateType)
			{
			case GeneTemplateDef.GeneTemplateType.Skill:
				foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
				{
					yield return GetFromTemplate(g, allDef, allDef.index * 1000, hotReload);
				}
				break;
			case GeneTemplateDef.GeneTemplateType.Chemical:
				foreach (ChemicalDef allDef2 in DefDatabase<ChemicalDef>.AllDefs)
				{
					if (allDef2.generateAddictionGenes)
					{
						yield return GetFromTemplate(g, allDef2, allDef2.index * 1000, hotReload);
					}
				}
				break;
			}
		}
	}

	public static IEnumerable<ThoughtDef> ImpliedThoughtDefs(bool hotReload = false)
	{
		if (!ModsConfig.BiotechActive)
		{
			yield break;
		}
		foreach (GeneTemplateDef allDef in DefDatabase<GeneTemplateDef>.AllDefs)
		{
			if (allDef.geneTemplateType != GeneTemplateDef.GeneTemplateType.Chemical || !(allDef.geneClass == typeof(Gene_ChemicalDependency)))
			{
				continue;
			}
			foreach (ChemicalDef allDef2 in DefDatabase<ChemicalDef>.AllDefs)
			{
				if (allDef2.generateAddictionGenes)
				{
					yield return DependencyThoughtDef(allDef2, hotReload);
				}
			}
		}
	}

	private static GeneDef GetFromTemplate(GeneTemplateDef template, Def def, int displayOrderBase, bool hotReload = false)
	{
		string defName = template.defName + "_" + def.defName;
		GeneDef geneDef = (hotReload ? (DefDatabase<GeneDef>.GetNamed(defName, errorOnFail: false) ?? new GeneDef()) : new GeneDef());
		geneDef.defName = defName;
		geneDef.geneClass = template.geneClass;
		geneDef.label = template.label.Formatted(def.label);
		geneDef.iconPath = template.iconPath.Formatted(def.defName);
		geneDef.description = ResolveDescription();
		geneDef.labelShortAdj = template.labelShortAdj.Formatted(def.label);
		geneDef.selectionWeight = template.selectionWeight;
		geneDef.biostatCpx = template.biostatCpx;
		geneDef.biostatMet = template.biostatMet;
		geneDef.displayCategory = template.displayCategory;
		geneDef.displayOrderInCategory = displayOrderBase + template.displayOrderOffset;
		geneDef.minAgeActive = template.minAgeActive;
		geneDef.modContentPack = template.modContentPack;
		if (!template.exclusionTagPrefix.NullOrEmpty())
		{
			geneDef.exclusionTags = new List<string> { template.exclusionTagPrefix + "_" + def.defName };
		}
		if (def is SkillDef skill)
		{
			if (template.aptitudeOffset != 0)
			{
				geneDef.aptitudes = new List<Aptitude>
				{
					new Aptitude(skill, template.aptitudeOffset)
				};
			}
			if (template.passionModType != PassionMod.PassionModType.None)
			{
				geneDef.passionMod = new PassionMod(skill, template.passionModType);
			}
		}
		else if (def is ChemicalDef chemicalDef)
		{
			geneDef.chemical = chemicalDef;
			geneDef.addictionChanceFactor = template.addictionChanceFactor;
			if (!template.chemicalBiostatOverrides.NullOrEmpty())
			{
				foreach (GeneTemplateDef.ChemicalBiostatOverride chemicalBiostatOverride in template.chemicalBiostatOverrides)
				{
					if (chemicalBiostatOverride.chemical == chemicalDef)
					{
						geneDef.biostatCpx = chemicalBiostatOverride.biostatCpx ?? geneDef.biostatCpx;
						geneDef.biostatMet = chemicalBiostatOverride.biostatMet ?? geneDef.biostatMet;
						geneDef.biostatArc = chemicalBiostatOverride.biostatArc ?? geneDef.biostatArc;
					}
				}
			}
			if (geneDef.geneClass != typeof(Gene_ChemicalDependency))
			{
				if (geneDef.addictionChanceFactor <= 0f)
				{
					geneDef.overdoseChanceFactor = chemicalDef.geneOverdoseChanceFactorImmune;
					geneDef.toleranceBuildupFactor = chemicalDef.geneToleranceBuildupFactorImmune;
				}
				else
				{
					geneDef.overdoseChanceFactor = chemicalDef.geneOverdoseChanceFactorResist;
					geneDef.toleranceBuildupFactor = chemicalDef.geneToleranceBuildupFactorResist;
				}
				if (geneDef.overdoseChanceFactor != 1f)
				{
					geneDef.description += " " + ((geneDef.overdoseChanceFactor == 0f) ? "GeneOverdoseImmune" : "GeneOverdoseFactor").Translate(chemicalDef, geneDef.overdoseChanceFactor.ToStringPercent());
				}
				if (geneDef.toleranceBuildupFactor != 1f)
				{
					geneDef.description += " " + ((geneDef.toleranceBuildupFactor == 0f) ? "GeneToleranceBuildupImmune" : "GeneToleranceBuildupFactor").Translate(chemicalDef, geneDef.toleranceBuildupFactor.ToStringPercent());
				}
			}
		}
		return geneDef;
		string ResolveDescription()
		{
			if (template.geneClass == typeof(Gene_ChemicalDependency))
			{
				return template.description.Formatted(def.label, "PeriodDays".Translate(5f).Named("DEFICIENCYDURATION"), "PeriodDays".Translate(30f).Named("COMADURATION"), "PeriodDays".Translate(60f).Named("DEATHDURATION"));
			}
			return template.description.Formatted(def.label);
		}
	}

	private static ThoughtDef DependencyThoughtDef(ChemicalDef chemicalDef, bool hotReload = false)
	{
		string defName = "GeneticChemicalDependency_" + chemicalDef.defName;
		ThoughtDef obj = (hotReload ? (DefDatabase<ThoughtDef>.GetNamed(defName, errorOnFail: false) ?? new ThoughtDef()) : new ThoughtDef());
		obj.defName = defName;
		obj.thoughtClass = typeof(Thought_Situational_GeneticChemicalDependency);
		obj.workerClass = typeof(ThoughtWorker_GeneticChemicalDependency);
		obj.developmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;
		obj.chemicalDef = chemicalDef;
		obj.stages = new List<ThoughtStage>
		{
			new ThoughtStage
			{
				label = "GeneticChemicalDependency_Label".Translate(chemicalDef.Named("CHEMICAL")),
				description = "GeneticChemicalDependency_Desc".Translate(chemicalDef.Named("CHEMICAL")),
				baseMoodEffect = -1f
			}
		};
		return obj;
	}
}
