using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class BackstoryDef : Def
{
	public const string PlayerColonistCategoryChild = "Child";

	[MayTranslate]
	public string baseDesc;

	[NoTranslate]
	public string identifier;

	public BackstorySlot slot;

	[MustTranslate]
	public string title;

	[MustTranslate]
	public string titleFemale;

	[MustTranslate]
	public string titleShort;

	[MustTranslate]
	public string titleShortFemale;

	public List<SkillGain> skillGains = new List<SkillGain>();

	public WorkTags workDisables;

	public WorkTags requiredWorkTags;

	[NoTranslate]
	public List<string> spawnCategories = new List<string>();

	public BodyTypeDef bodyTypeGlobal;

	public BodyTypeDef bodyTypeFemale;

	public BodyTypeDef bodyTypeMale;

	public List<BackstoryTrait> forcedTraits;

	public List<BackstoryTrait> disallowedTraits;

	public RulePackDef nameMaker;

	public List<PossessionThingDefCountClass> possessions = new List<PossessionThingDefCountClass>();

	public bool shuffleable = true;

	public bool requiresSpawnCategory;

	[Unsaved(false)]
	public string untranslatedTitle;

	[Unsaved(false)]
	public string untranslatedTitleFemale;

	[Unsaved(false)]
	public string untranslatedTitleShort;

	[Unsaved(false)]
	public string untranslatedTitleShortFemale;

	[Unsaved(false)]
	public string untranslatedDesc;

	private List<WorkTypeDef> cachedDisabledWorkTypes;

	private List<string> unlockedMeditationTypesTemp = new List<string>();

	public bool IsPlayerColonyChildBackstory => spawnCategories?.Contains("Child") ?? false;

	public List<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			if (cachedDisabledWorkTypes == null)
			{
				cachedDisabledWorkTypes = new List<WorkTypeDef>();
				List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (!AllowsWorkType(allDefsListForReading[i]))
					{
						cachedDisabledWorkTypes.Add(allDefsListForReading[i]);
					}
				}
			}
			return cachedDisabledWorkTypes;
		}
	}

	public IEnumerable<WorkGiverDef> DisabledWorkGivers
	{
		get
		{
			List<WorkGiverDef> list = DefDatabase<WorkGiverDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if (!AllowsWorkGiver(list[i]))
				{
					yield return list[i];
				}
			}
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		untranslatedTitle = title;
		untranslatedTitleFemale = titleFemale;
		untranslatedTitleShort = titleShort;
		untranslatedTitleShortFemale = titleShortFemale;
		untranslatedDesc = description;
		if (description.NullOrEmpty() && !baseDesc.NullOrEmpty())
		{
			description = baseDesc;
		}
		description = description.TrimEnd();
		description = description.Replace("\r", "");
	}

	public string TitleFor(Gender g)
	{
		if (g != Gender.Female || titleFemale.NullOrEmpty())
		{
			return title;
		}
		return titleFemale;
	}

	public string TitleCapFor(Gender g)
	{
		return TitleFor(g).CapitalizeFirst();
	}

	public string TitleShortFor(Gender g)
	{
		if (g == Gender.Female && !titleShortFemale.NullOrEmpty())
		{
			return titleShortFemale;
		}
		if (!titleShort.NullOrEmpty())
		{
			return titleShort;
		}
		return TitleFor(g);
	}

	public BodyTypeDef BodyTypeFor(Gender g)
	{
		if (bodyTypeGlobal == null)
		{
			switch (g)
			{
			case Gender.None:
				break;
			case Gender.Female:
				return bodyTypeFemale;
			default:
				return bodyTypeMale;
			}
		}
		return bodyTypeGlobal;
	}

	public TaggedString FullDescriptionFor(Pawn p)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(description.Formatted(p.Named("PAWN")).AdjustedFor(p).Resolve());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			SkillDef skillDef = allDefsListForReading[i];
			foreach (SkillGain skillGain in skillGains)
			{
				if (skillGain.skill == skillDef)
				{
					stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " + skillGain.amount.ToString("+##;-##"));
					break;
				}
			}
		}
		if (DisabledWorkTypes.Any() || DisabledWorkGivers.Any())
		{
			stringBuilder.AppendLine();
		}
		foreach (WorkTypeDef disabledWorkType in DisabledWorkTypes)
		{
			stringBuilder.AppendLine(disabledWorkType.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
		}
		foreach (WorkGiverDef disabledWorkGiver in DisabledWorkGivers)
		{
			stringBuilder.AppendLine(disabledWorkGiver.workType.gerundLabel.CapitalizeFirst() + ": " + disabledWorkGiver.LabelCap + " " + "DisabledLower".Translate());
		}
		if (ModsConfig.RoyaltyActive)
		{
			unlockedMeditationTypesTemp.Clear();
			foreach (MeditationFocusDef allDef in DefDatabase<MeditationFocusDef>.AllDefs)
			{
				for (int j = 0; j < allDef.requiredBackstoriesAny.Count; j++)
				{
					BackstoryCategoryAndSlot backstoryCategoryAndSlot = allDef.requiredBackstoriesAny[j];
					if (spawnCategories.Contains(backstoryCategoryAndSlot.categoryName) && backstoryCategoryAndSlot.slot == slot)
					{
						unlockedMeditationTypesTemp.Add(allDef.LabelCap);
						break;
					}
				}
			}
			if (unlockedMeditationTypesTemp.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("MeditationFocusesUnlocked".Translate() + ": ");
				stringBuilder.AppendLine(unlockedMeditationTypesTemp.ToLineList("  - "));
			}
		}
		if (!modContentPack.IsOfficialMod)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Source".Translate() + ": " + modContentPack.Name);
		}
		string str = stringBuilder.ToString().TrimEndNewlines();
		return Find.ActiveLanguageWorker.PostProcessed(str);
	}

	public void SetTitle(string newTitle, string newTitleFemale)
	{
		title = newTitle;
		titleFemale = newTitleFemale;
	}

	public void SetTitleShort(string newTitleShort, string newTitleShortFemale)
	{
		titleShort = newTitleShort;
		titleShortFemale = newTitleShortFemale;
	}

	public bool DisallowsTrait(TraitDef def, int degree)
	{
		if (disallowedTraits == null)
		{
			return false;
		}
		for (int i = 0; i < disallowedTraits.Count; i++)
		{
			if (disallowedTraits[i].def == def && disallowedTraits[i].degree == degree)
			{
				return true;
			}
		}
		return false;
	}

	private bool AllowsWorkType(WorkTypeDef workType)
	{
		return (workDisables & workType.workTags) == 0;
	}

	private bool AllowsWorkGiver(WorkGiverDef workGiver)
	{
		return (workDisables & workGiver.workTags) == 0;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (title.NullOrEmpty())
		{
			yield return "null title, description is " + description;
		}
		if (titleShort.NullOrEmpty())
		{
			yield return "null titleShort, description is " + description;
		}
		if ((workDisables & WorkTags.Violent) != WorkTags.None && spawnCategories.Contains("Pirate"))
		{
			yield return "cannot do Violent work but can spawn as a pirate";
		}
		if (shuffleable && spawnCategories.Count == 0)
		{
			yield return "no spawn categories";
		}
		if (forcedTraits != null)
		{
			foreach (BackstoryTrait forcedTrait in forcedTraits)
			{
				if (!forcedTrait.def.degreeDatas.Any((TraitDegreeData d) => d.degree == forcedTrait.degree))
				{
					yield return "Backstory " + title + " has invalid trait " + forcedTrait.def.defName + " degree=" + forcedTrait.degree;
				}
			}
		}
		if (!Prefs.DevMode)
		{
			yield break;
		}
		foreach (SkillGain skillGain in skillGains)
		{
			if (skillGain.skill.IsDisabled(workDisables, DisabledWorkTypes))
			{
				yield return "modifies skill " + skillGain.skill?.ToString() + " but also disables this skill";
			}
		}
	}
}
