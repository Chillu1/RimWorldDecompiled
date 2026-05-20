using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class XenotypeDef : Def
{
	public List<GeneDef> genes = new List<GeneDef>();

	public bool inheritable;

	[NoTranslate]
	public string iconPath;

	[MustTranslate]
	public string descriptionShort;

	public float combatPowerFactor = 1f;

	public bool canGenerateAsCombatant = true;

	public float factionlessGenerationWeight = 1f;

	public RulePackDef nameMaker;

	public RulePackDef nameMakerFemale;

	public float chanceToUseNameMaker;

	public List<WeaponClassDef> forbiddenWeaponClasses;

	public float generateWithXenogermReplicatingHediffChance;

	public FloatRange xenogermReplicatingDurationLeftDaysRange = FloatRange.Zero;

	public SoundDef soundDefOnImplant;

	public List<XenotypeChance> doubleXenotypeChances;

	public float displayPriority;

	[Unsaved(false)]
	private Texture2D cachedIcon;

	public static readonly Color IconColor = new Color(0.75f, 0.75f, 0.75f);

	public Texture2D Icon
	{
		get
		{
			if (cachedIcon == null)
			{
				if (iconPath.NullOrEmpty())
				{
					cachedIcon = BaseContent.BadTex;
				}
				else
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
				}
			}
			return cachedIcon;
		}
	}

	public List<GeneDef> AllGenes => genes;

	public bool Archite => AllGenes.Any((GeneDef g) => g.displayCategory == GeneCategoryDefOf.Archite);

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (genes.NullOrEmpty())
		{
			return;
		}
		if (descriptionHyperlinks == null)
		{
			descriptionHyperlinks = new List<DefHyperlink>();
		}
		foreach (GeneDef gene in genes)
		{
			descriptionHyperlinks.Add(new DefHyperlink(gene));
		}
	}

	public RulePackDef GetNameMaker(Gender gender)
	{
		if (gender == Gender.Female && nameMakerFemale != null)
		{
			return nameMakerFemale;
		}
		return nameMaker;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Genes".Translate().CapitalizeFirst(), genes.Select((GeneDef x) => x.label).ToCommaList().CapitalizeFirst(), "GenesDesc".Translate() + "\n\n" + genes.Select((GeneDef x) => x.label).ToLineList("  - ", capitalizeItems: true), 1000);
		if (!genes.NullOrEmpty())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "GenesAreInheritable".Translate(), inheritable.ToStringYesNo(), "GenesAreInheritableXenotypeDef".Translate(), 990);
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (iconPath.NullOrEmpty())
		{
			yield return "iconPath is empty.";
		}
		if (doubleXenotypeChances == null)
		{
			yield break;
		}
		if (doubleXenotypeChances.Sum((XenotypeChance x) => x.chance) > 1f)
		{
			yield return "doubleXenotypeChance sum is > 1";
		}
		foreach (XenotypeChance doubleXenotypeChance in doubleXenotypeChances)
		{
			if (!doubleXenotypeChance.xenotype.inheritable)
			{
				yield return "doubleXenotypeChance contains non-inheritable xenotype " + doubleXenotypeChance.xenotype.label;
			}
		}
	}
}
