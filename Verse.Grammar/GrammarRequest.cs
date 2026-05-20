using System.Collections.Generic;

namespace Verse.Grammar;

public struct GrammarRequest
{
	public interface ICustomizer
	{
		int PriorityBucket(Rule rule);

		void Notify_RuleUsed(Rule rule);

		bool ValidateRule(Rule rule);
	}

	private List<Rule> rules;

	private List<RulePack> includesBare;

	private List<RulePackDef> includes;

	private Dictionary<string, string> constants;

	public ICustomizer customizer;

	public List<Rule> RulesAllowNull => rules;

	public List<Rule> Rules
	{
		get
		{
			if (rules == null)
			{
				rules = new List<Rule>();
			}
			return rules;
		}
	}

	public List<RulePack> IncludesBareAllowNull => includesBare;

	public List<RulePack> IncludesBare
	{
		get
		{
			if (includesBare == null)
			{
				includesBare = new List<RulePack>();
			}
			return includesBare;
		}
	}

	public List<RulePackDef> IncludesAllowNull => includes;

	public List<RulePackDef> Includes
	{
		get
		{
			if (includes == null)
			{
				includes = new List<RulePackDef>();
			}
			return includes;
		}
	}

	public Dictionary<string, string> ConstantsAllowNull => constants;

	public Dictionary<string, string> Constants
	{
		get
		{
			if (constants == null)
			{
				constants = new Dictionary<string, string>();
			}
			return constants;
		}
	}

	public bool HasRule(string keyword)
	{
		if (rules != null && rules.Any(HasTargetRule))
		{
			return true;
		}
		if (includes != null && includes.Any((RulePackDef i) => i.RulesPlusIncludes.Any(HasTargetRule)))
		{
			return true;
		}
		if (includesBare != null && includesBare.Any((RulePack rp) => rp.Rules.Any(HasTargetRule)))
		{
			return true;
		}
		return false;
		bool HasTargetRule(Rule r)
		{
			return r.keyword == keyword;
		}
	}

	public void Clear()
	{
		if (rules != null)
		{
			rules.Clear();
		}
		if (includesBare != null)
		{
			includesBare.Clear();
		}
		if (includes != null)
		{
			includes.Clear();
		}
		if (constants != null)
		{
			constants.Clear();
		}
	}
}
