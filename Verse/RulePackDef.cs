using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class RulePackDef : Def
	{
		public List<RulePackDef> include;

		private RulePack rulePack;

		[Unsaved(false)]
		private List<Rule> cachedRules;

		[Unsaved(false)]
		private List<Rule> cachedUntranslatedRules;

		public List<Rule> RulesPlusIncludes
		{
			get
			{
				if (cachedRules == null)
				{
					cachedRules = new List<Rule>();
					if (rulePack != null)
					{
						cachedRules.AddRange(rulePack.Rules);
					}
					if (include != null)
					{
						for (int i = 0; i < include.Count; i++)
						{
							cachedRules.AddRange(include[i].RulesPlusIncludes);
						}
					}
				}
				return cachedRules;
			}
		}

		public List<Rule> UntranslatedRulesPlusIncludes
		{
			get
			{
				if (cachedUntranslatedRules == null)
				{
					cachedUntranslatedRules = new List<Rule>();
					if (rulePack != null)
					{
						cachedUntranslatedRules.AddRange(rulePack.UntranslatedRules);
					}
					if (include != null)
					{
						for (int i = 0; i < include.Count; i++)
						{
							cachedUntranslatedRules.AddRange(include[i].UntranslatedRulesPlusIncludes);
						}
					}
				}
				return cachedUntranslatedRules;
			}
		}

		public List<Rule> RulesImmediate
		{
			get
			{
				if (rulePack == null)
				{
					return null;
				}
				return rulePack.Rules;
			}
		}

		public List<Rule> UntranslatedRulesImmediate
		{
			get
			{
				if (rulePack == null)
				{
					return null;
				}
				return rulePack.UntranslatedRules;
			}
		}

		public string FirstRuleKeyword
		{
			get
			{
				List<Rule> rulesPlusIncludes = RulesPlusIncludes;
				if (!rulesPlusIncludes.Any())
				{
					return "none";
				}
				return rulesPlusIncludes[0].keyword;
			}
		}

		public string FirstUntranslatedRuleKeyword
		{
			get
			{
				List<Rule> untranslatedRulesPlusIncludes = UntranslatedRulesPlusIncludes;
				if (!untranslatedRulesPlusIncludes.Any())
				{
					return "none";
				}
				return untranslatedRulesPlusIncludes[0].keyword;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (include == null)
			{
				yield break;
			}
			for (int i = 0; i < include.Count; i++)
			{
				if (include[i].include != null && include[i].include.Contains(this))
				{
					yield return "includes other RulePackDef which includes it: " + include[i].defName;
				}
			}
		}

		public static RulePackDef Named(string defName)
		{
			return DefDatabase<RulePackDef>.GetNamed(defName);
		}
	}
}
