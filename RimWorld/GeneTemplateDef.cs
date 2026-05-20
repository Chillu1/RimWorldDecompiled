using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GeneTemplateDef : Def
{
	public enum GeneTemplateType
	{
		Skill,
		Chemical
	}

	public class ChemicalBiostatOverride
	{
		public ChemicalDef chemical;

		public int? biostatCpx;

		public int? biostatMet;

		public int? biostatArc;
	}

	public Type geneClass = typeof(Gene);

	public int biostatCpx;

	public int biostatMet;

	public int aptitudeOffset;

	public float addictionChanceFactor = 1f;

	public PassionMod.PassionModType passionModType;

	public GeneTemplateType geneTemplateType;

	public float minAgeActive;

	public GeneCategoryDef displayCategory;

	public int displayOrderOffset;

	public float selectionWeight = 1f;

	public List<ChemicalBiostatOverride> chemicalBiostatOverrides;

	[MustTranslate]
	public string labelShortAdj;

	[NoTranslate]
	public string iconPath;

	[NoTranslate]
	public string exclusionTagPrefix;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (!typeof(Gene).IsAssignableFrom(geneClass))
		{
			yield return "geneClass is not Gene or child thereof.";
		}
	}
}
