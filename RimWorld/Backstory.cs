using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[CaseInsensitiveXMLParsing]
	public class Backstory
	{
		public string identifier;

		public BackstorySlot slot;

		public string title;

		public string titleFemale;

		public string titleShort;

		public string titleShortFemale;

		public string baseDesc;

		private Dictionary<string, int> skillGains = new Dictionary<string, int>();

		[Unsaved(false)]
		public Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();

		public WorkTags workDisables;

		public WorkTags requiredWorkTags;

		public List<string> spawnCategories = new List<string>();

		[LoadAlias("bodyNameGlobal")]
		private string bodyTypeGlobal;

		[LoadAlias("bodyNameFemale")]
		private string bodyTypeFemale;

		[LoadAlias("bodyNameMale")]
		private string bodyTypeMale;

		[Unsaved(false)]
		private BodyTypeDef bodyTypeGlobalResolved;

		[Unsaved(false)]
		private BodyTypeDef bodyTypeFemaleResolved;

		[Unsaved(false)]
		private BodyTypeDef bodyTypeMaleResolved;

		public List<TraitEntry> forcedTraits;

		public List<TraitEntry> disallowedTraits;

		public List<string> hairTags;

		private string nameMaker;

		private RulePackDef nameMakerResolved;

		public bool shuffleable = true;

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

		[Unsaved(false)]
		public bool titleTranslated;

		[Unsaved(false)]
		public bool titleFemaleTranslated;

		[Unsaved(false)]
		public bool titleShortTranslated;

		[Unsaved(false)]
		public bool titleShortFemaleTranslated;

		[Unsaved(false)]
		public bool descTranslated;

		private List<string> unlockedMeditationTypesTemp = new List<string>();

		public RulePackDef NameMaker => nameMakerResolved;

		public IEnumerable<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				for (int i = 0; i < list.Count; i++)
				{
					if (!AllowsWorkType(list[i]))
					{
						yield return list[i];
					}
				}
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

		public string TitleShortCapFor(Gender g)
		{
			return TitleShortFor(g).CapitalizeFirst();
		}

		public BodyTypeDef BodyTypeFor(Gender g)
		{
			if (bodyTypeGlobalResolved == null)
			{
				switch (g)
				{
				case Gender.None:
					break;
				case Gender.Female:
					return bodyTypeFemaleResolved;
				default:
					return bodyTypeMaleResolved;
				}
			}
			return bodyTypeGlobalResolved;
		}

		public TaggedString FullDescriptionFor(Pawn p)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(baseDesc.Formatted(p.Named("PAWN")).AdjustedFor(p).Resolve());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				if (skillGainsResolved.ContainsKey(skillDef))
				{
					stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " + skillGainsResolved[skillDef].ToString("+##;-##"));
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
			string str = stringBuilder.ToString().TrimEndNewlines();
			return Find.ActiveLanguageWorker.PostProcessed(str);
		}

		private bool AllowsWorkType(WorkTypeDef workType)
		{
			return (workDisables & workType.workTags) == 0;
		}

		private bool AllowsWorkGiver(WorkGiverDef workGiver)
		{
			return (workDisables & workGiver.workTags) == 0;
		}

		internal void AddForcedTrait(TraitDef traitDef, int degree = 0)
		{
			if (forcedTraits == null)
			{
				forcedTraits = new List<TraitEntry>();
			}
			forcedTraits.Add(new TraitEntry(traitDef, degree));
		}

		internal void AddDisallowedTrait(TraitDef traitDef, int degree = 0)
		{
			if (disallowedTraits == null)
			{
				disallowedTraits = new List<TraitEntry>();
			}
			disallowedTraits.Add(new TraitEntry(traitDef, degree));
		}

		public void PostLoad()
		{
			untranslatedTitle = title;
			untranslatedTitleFemale = titleFemale;
			untranslatedTitleShort = titleShort;
			untranslatedTitleShortFemale = titleShortFemale;
			untranslatedDesc = baseDesc;
			baseDesc = baseDesc.TrimEnd();
			baseDesc = baseDesc.Replace("\r", "");
		}

		public void ResolveReferences()
		{
			int num = Mathf.Abs(GenText.StableStringHash(baseDesc) % 100);
			string s = title.Replace('-', ' ');
			s = GenText.CapitalizedNoSpaces(s);
			identifier = GenText.RemoveNonAlphanumeric(s) + num;
			foreach (KeyValuePair<string, int> skillGain in skillGains)
			{
				skillGainsResolved.Add(DefDatabase<SkillDef>.GetNamed(skillGain.Key), skillGain.Value);
			}
			skillGains = null;
			if (!bodyTypeGlobal.NullOrEmpty())
			{
				bodyTypeGlobalResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeGlobal);
			}
			if (!bodyTypeFemale.NullOrEmpty())
			{
				bodyTypeFemaleResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeFemale);
			}
			if (!bodyTypeMale.NullOrEmpty())
			{
				bodyTypeMaleResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeMale);
			}
			if (!nameMaker.NullOrEmpty())
			{
				nameMakerResolved = DefDatabase<RulePackDef>.GetNamed(nameMaker);
			}
			if (slot == BackstorySlot.Adulthood && bodyTypeGlobalResolved == null)
			{
				if (bodyTypeMaleResolved == null)
				{
					Log.Error("Adulthood backstory " + title + " is missing male body type. Defaulting...");
					bodyTypeMaleResolved = BodyTypeDefOf.Male;
				}
				if (bodyTypeFemaleResolved == null)
				{
					Log.Error("Adulthood backstory " + title + " is missing female body type. Defaulting...");
					bodyTypeFemaleResolved = BodyTypeDefOf.Female;
				}
			}
		}

		public IEnumerable<string> ConfigErrors(bool ignoreNoSpawnCategories)
		{
			if (title.NullOrEmpty())
			{
				yield return "null title, baseDesc is " + baseDesc;
			}
			if (titleShort.NullOrEmpty())
			{
				yield return "null titleShort, baseDesc is " + baseDesc;
			}
			if ((workDisables & WorkTags.Violent) != 0 && spawnCategories.Contains("Pirate"))
			{
				yield return "cannot do Violent work but can spawn as a pirate";
			}
			if (spawnCategories.Count == 0 && !ignoreNoSpawnCategories)
			{
				yield return "no spawn categories";
			}
			if (!baseDesc.NullOrEmpty())
			{
				if (char.IsWhiteSpace(baseDesc[0]))
				{
					yield return "baseDesc starts with whitepspace";
				}
				if (char.IsWhiteSpace(baseDesc[baseDesc.Length - 1]))
				{
					yield return "baseDesc ends with whitespace";
				}
			}
			if (forcedTraits != null)
			{
				foreach (TraitEntry forcedTrait in forcedTraits)
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
			foreach (KeyValuePair<SkillDef, int> item in skillGainsResolved)
			{
				if (item.Key.IsDisabled(workDisables, DisabledWorkTypes))
				{
					yield return string.Concat("modifies skill ", item.Key, " but also disables this skill");
				}
			}
			foreach (KeyValuePair<string, Backstory> allBackstory in BackstoryDatabase.allBackstories)
			{
				if (allBackstory.Value != this && allBackstory.Value.identifier == identifier)
				{
					yield return "backstory identifier used more than once: " + identifier;
				}
			}
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

		public override string ToString()
		{
			if (title.NullOrEmpty())
			{
				return "(NullTitleBackstory)";
			}
			return "(" + title + ")";
		}

		public override int GetHashCode()
		{
			return identifier.GetHashCode();
		}
	}
}
