using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public class RulePack
	{
		[MustTranslate]
		[TranslationCanChangeCount]
		private List<string> rulesStrings = new List<string>();

		[MayTranslate]
		[TranslationCanChangeCount]
		private List<string> rulesFiles = new List<string>();

		private List<Rule> rulesRaw;

		public List<RulePackDef> include;

		[Unsaved(false)]
		private List<Rule> rulesResolved;

		[Unsaved(false)]
		private List<Rule> untranslatedRulesResolved;

		[Unsaved(false)]
		private List<string> untranslatedRulesStrings;

		[Unsaved(false)]
		private List<string> untranslatedRulesFiles;

		[Unsaved(false)]
		private List<Rule> untranslatedRulesRaw;

		public List<Rule> Rules
		{
			get
			{
				if (rulesResolved == null)
				{
					rulesResolved = GetRulesResolved(rulesRaw, rulesStrings, rulesFiles);
					if (include != null)
					{
						foreach (RulePackDef item in include)
						{
							rulesResolved.AddRange(item.RulesPlusIncludes);
						}
					}
				}
				return rulesResolved;
			}
		}

		public List<Rule> UntranslatedRules
		{
			get
			{
				if (untranslatedRulesResolved == null)
				{
					untranslatedRulesResolved = GetRulesResolved(untranslatedRulesRaw, untranslatedRulesStrings, untranslatedRulesFiles);
					if (include != null)
					{
						foreach (RulePackDef item in include)
						{
							untranslatedRulesResolved.AddRange(item.UntranslatedRulesPlusIncludes);
						}
					}
				}
				return untranslatedRulesResolved;
			}
		}

		public void PostLoad()
		{
			untranslatedRulesStrings = rulesStrings.ToList();
			untranslatedRulesFiles = rulesFiles.ToList();
			if (rulesRaw != null)
			{
				untranslatedRulesRaw = new List<Rule>();
				for (int i = 0; i < rulesRaw.Count; i++)
				{
					untranslatedRulesRaw.Add(rulesRaw[i].DeepCopy());
				}
			}
		}

		private static List<Rule> GetRulesResolved(List<Rule> rulesRaw, List<string> rulesStrings, List<string> rulesFiles)
		{
			List<Rule> list = new List<Rule>();
			for (int i = 0; i < rulesStrings.Count; i++)
			{
				try
				{
					Rule_String rule_String = new Rule_String(rulesStrings[i]);
					rule_String.Init();
					list.Add(rule_String);
				}
				catch (Exception ex)
				{
					Log.Error("Exception parsing grammar rule from " + rulesStrings[i] + ": " + ex);
				}
			}
			for (int j = 0; j < rulesFiles.Count; j++)
			{
				try
				{
					string[] array = rulesFiles[j].Split(new string[1]
					{
						"->"
					}, StringSplitOptions.None);
					Rule_File rule_File = new Rule_File();
					rule_File.keyword = array[0].Trim();
					rule_File.path = array[1].Trim();
					rule_File.Init();
					list.Add(rule_File);
				}
				catch (Exception ex2)
				{
					Log.Error("Error initializing Rule_File " + rulesFiles[j] + ": " + ex2);
				}
			}
			if (rulesRaw != null)
			{
				for (int k = 0; k < rulesRaw.Count; k++)
				{
					try
					{
						rulesRaw[k].Init();
						list.Add(rulesRaw[k]);
					}
					catch (Exception ex3)
					{
						Log.Error("Error initializing rule " + rulesRaw[k].ToStringSafe() + ": " + ex3);
					}
				}
			}
			return list;
		}
	}
}
