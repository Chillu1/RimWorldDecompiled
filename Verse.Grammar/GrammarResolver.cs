using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;

namespace Verse.Grammar;

public static class GrammarResolver
{
	private class RuleEntry
	{
		public Rule rule;

		public bool knownUnresolvable;

		public bool constantConstraintsChecked;

		public bool constantConstraintsValid;

		public int uses;

		public float SelectionWeight => rule.BaseSelectionWeight * 100000f / (float)((uses + 1) * 1000);

		public float Priority => rule.Priority;

		public RuleEntry(Rule rule)
		{
			this.rule = rule;
			knownUnresolvable = false;
		}

		public void MarkKnownUnresolvable()
		{
			knownUnresolvable = true;
		}

		public bool ValidateConstantConstraints(Dictionary<string, string> constraints)
		{
			if (!constantConstraintsChecked)
			{
				constantConstraintsValid = true;
				if (rule.constantConstraints != null)
				{
					constantConstraintsValid = rule.ValidateConstraints(constraints);
				}
				constantConstraintsChecked = true;
			}
			return constantConstraintsValid;
		}

		public bool ValidateRequiredTag(List<string> extraTags, List<string> resolvedTags)
		{
			if (rule.requiredTag.NullOrEmpty())
			{
				return true;
			}
			if (extraTags != null && extraTags.Contains(rule.requiredTag))
			{
				return true;
			}
			return resolvedTags.Contains(rule.requiredTag);
		}

		public bool ValidateTimesUsed()
		{
			if (!rule.usesLimit.HasValue)
			{
				return true;
			}
			return uses < rule.usesLimit.Value;
		}

		public override string ToString()
		{
			return rule.ToString();
		}
	}

	private static SimpleLinearPool<List<RuleEntry>> rulePool = new SimpleLinearPool<List<RuleEntry>>();

	private static Dictionary<string, List<RuleEntry>> rules = new Dictionary<string, List<RuleEntry>>();

	private static int loopCount;

	private static StringBuilder logSbTrace;

	private static StringBuilder logSbMid;

	private static StringBuilder logSbRules;

	private const int DepthLimit = 50;

	private const int LoopsLimit = 1000;

	private static Regex Spaces = new Regex(" +([,.])");

	private static Regex FunctionCall = new Regex("{\\s*(\\w+)\\s*\\:\\s*([^}]* ?)\\s*}");

	private static Regex GenderCall = new Regex("{\\s*(\\w+)_gender\\s*\\?\\s*([^}]* ?)\\s*}");

	public const char SymbolStartChar = '[';

	public const char SymbolEndChar = ']';

	private static readonly char[] SpecialChars = new char[4] { '[', ']', '{', '}' };

	public static string Resolve(string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false, string untranslatedRootKeyword = null, List<string> extraTags = null, List<string> outTags = null, bool capitalizeFirstSentence = true)
	{
		if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage)
		{
			return ResolveUnsafe(rootKeyword, request, debugLabel, forceLog, useUntranslatedRules: false, extraTags, outTags, capitalizeFirstSentence);
		}
		string text;
		bool success;
		Exception ex;
		try
		{
			text = ResolveUnsafe(rootKeyword, request, out success, debugLabel, forceLog, useUntranslatedRules: false, extraTags, outTags, capitalizeFirstSentence);
			ex = null;
		}
		catch (Exception ex2)
		{
			success = false;
			text = "";
			ex = ex2;
		}
		if (success)
		{
			return text;
		}
		string text2 = "Failed to resolve text. Trying again with English.";
		if (ex != null)
		{
			text2 = text2 + " Exception: " + ex;
		}
		Log.ErrorOnce(text2, text.GetHashCode());
		outTags?.Clear();
		return ResolveUnsafe(untranslatedRootKeyword ?? rootKeyword, request, out success, debugLabel, forceLog, useUntranslatedRules: true, extraTags, outTags, capitalizeFirstSentence);
	}

	public static string ResolveUnsafe(string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false, bool useUntranslatedRules = false, List<string> extraTags = null, List<string> outTags = null, bool capitalizeFirstSentence = true)
	{
		bool success;
		return ResolveUnsafe(rootKeyword, request, out success, debugLabel, forceLog, useUntranslatedRules, extraTags, outTags, capitalizeFirstSentence);
	}

	public static string ResolveUnsafe(string rootKeyword, GrammarRequest request, out bool success, string debugLabel = null, bool forceLog = false, bool useUntranslatedRules = false, List<string> extraTags = null, List<string> outTags = null, bool capitalizeFirstSentence = true)
	{
		bool flag = forceLog || DebugViewSettings.logGrammarResolution;
		rules.Clear();
		rulePool.Clear();
		if (flag)
		{
			logSbTrace = new StringBuilder();
			logSbMid = new StringBuilder();
			logSbRules = new StringBuilder();
		}
		List<Rule> rulesAllowNull = request.RulesAllowNull;
		if (rulesAllowNull != null)
		{
			if (flag)
			{
				logSbRules.AppendLine("CUSTOM RULES");
			}
			for (int i = 0; i < rulesAllowNull.Count; i++)
			{
				AddRule(rulesAllowNull[i]);
				if (flag)
				{
					logSbRules.AppendLine("■" + rulesAllowNull[i].ToString());
				}
			}
			if (flag)
			{
				logSbRules.AppendLine();
			}
		}
		List<RulePackDef> includesAllowNull = request.IncludesAllowNull;
		if (includesAllowNull != null)
		{
			HashSet<RulePackDef> hashSet = new HashSet<RulePackDef>();
			List<RulePackDef> list = new List<RulePackDef>(includesAllowNull);
			if (flag)
			{
				logSbMid.AppendLine("INCLUDES");
			}
			while (list.Count > 0)
			{
				RulePackDef rulePackDef = list[list.Count - 1];
				list.RemoveLast();
				if (hashSet.Contains(rulePackDef))
				{
					continue;
				}
				if (flag)
				{
					logSbMid.AppendLine($"{rulePackDef.defName}");
				}
				hashSet.Add(rulePackDef);
				List<Rule> list2 = (useUntranslatedRules ? rulePackDef.UntranslatedRulesImmediate : rulePackDef.RulesImmediate);
				if (list2 != null)
				{
					foreach (Rule item in list2)
					{
						AddRule(item);
					}
				}
				if (!rulePackDef.include.NullOrEmpty())
				{
					list.AddRange(rulePackDef.include);
				}
			}
		}
		List<RulePack> includesBareAllowNull = request.IncludesBareAllowNull;
		if (includesBareAllowNull != null)
		{
			if (flag)
			{
				logSbMid.AppendLine();
				logSbMid.AppendLine("BARE INCLUDES");
			}
			for (int j = 0; j < includesBareAllowNull.Count; j++)
			{
				List<Rule> list3 = (useUntranslatedRules ? includesBareAllowNull[j].UntranslatedRules : includesBareAllowNull[j].Rules);
				for (int k = 0; k < list3.Count; k++)
				{
					AddRule(list3[k]);
					if (flag)
					{
						logSbMid.AppendLine("  " + list3[k].ToString());
					}
				}
			}
		}
		if (flag && !extraTags.NullOrEmpty())
		{
			logSbMid.AppendLine();
			logSbMid.AppendLine("EXTRA TAGS");
			for (int l = 0; l < extraTags.Count; l++)
			{
				logSbMid.AppendLine("  " + extraTags[l]);
			}
		}
		List<Rule> list4 = (useUntranslatedRules ? RulePackDefOf.GlobalUtility.UntranslatedRulesPlusIncludes : RulePackDefOf.GlobalUtility.RulesPlusIncludes);
		for (int m = 0; m < list4.Count; m++)
		{
			AddRule(list4[m]);
		}
		loopCount = 0;
		Dictionary<string, string> constantsAllowNull = request.ConstantsAllowNull;
		if (flag && constantsAllowNull != null)
		{
			logSbMid.AppendLine("CONSTANTS");
			foreach (KeyValuePair<string, string> item2 in constantsAllowNull)
			{
				logSbMid.AppendLine(item2.Key.PadRight(38) + " " + item2.Value);
			}
		}
		if (flag)
		{
			logSbTrace.Append("GRAMMAR RESOLUTION TRACE");
		}
		string output = "err";
		bool flag2 = false;
		List<string> list5 = new List<string>();
		if (TryResolveRecursive(new RuleEntry(new Rule_String("", "[" + rootKeyword + "]")), 0, constantsAllowNull, out output, flag, extraTags, list5, request.customizer))
		{
			if (outTags != null)
			{
				outTags.Clear();
				outTags.AddRange(list5);
			}
		}
		else
		{
			flag2 = true;
			output = ((!request.Rules.NullOrEmpty()) ? ("ERR: " + request.Rules[0].Generate()) : "ERR");
			if (flag)
			{
				logSbTrace.Insert(0, "Grammar unresolvable. Root '" + rootKeyword + "'\n\n");
			}
			else
			{
				ResolveUnsafe(rootKeyword, request, debugLabel, forceLog: true, useUntranslatedRules, extraTags);
			}
		}
		output = ResolveAllFunctions(output, constantsAllowNull);
		output = GenText.CapitalizeSentences(Find.ActiveLanguageWorker.PostProcessed(output), capitalizeFirstSentence);
		output = Spaces.Replace(output, (Match match) => match.Groups[1].Value);
		output = output.Trim();
		if (flag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(logSbTrace.ToString().TrimEndNewlines());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(logSbMid.ToString().TrimEndNewlines());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(logSbRules.ToString().TrimEndNewlines());
			if (flag2)
			{
				if (DebugViewSettings.logGrammarResolution)
				{
					Log.Error(stringBuilder.ToString().Trim() + "\n");
				}
				else
				{
					Log.ErrorOnce(stringBuilder.ToString().Trim() + "\n", stringBuilder.ToString().Trim().GetHashCode());
				}
			}
			else
			{
				Log.Message(stringBuilder.ToString().Trim() + "\n");
			}
			logSbTrace = null;
			logSbMid = null;
			logSbRules = null;
		}
		success = !flag2;
		return output;
	}

	public static string ResolveAllFunctions(string input, Dictionary<string, string> constants)
	{
		if (input.Contains("%%"))
		{
			return input;
		}
		string text = input;
		MatchCollection matchCollection = FunctionCall.Matches(text);
		for (int num = matchCollection.Count - 1; num >= 0; num--)
		{
			Match match = matchCollection[num];
			string value = match.Groups[1].Value;
			string args = match.Groups[2].Value.Trim();
			if (value == "lookup" || value == "replace")
			{
				string value2 = GrammarResolverSimple.ResolveFunction(value, args, match.Value);
				text = text.Remove(match.Index, match.Length);
				text = text.Insert(match.Index, value2);
			}
			else
			{
				Log.Warning("Unknown grammar function name: " + value + ". Supported functions: lookup, replace.");
			}
		}
		MatchCollection matchCollection2 = GenderCall.Matches(text);
		for (int num2 = matchCollection2.Count - 1; num2 >= 0; num2--)
		{
			Match match2 = matchCollection2[num2];
			string value3 = match2.Groups[1].Value;
			string args2 = match2.Groups[2].Value.Trim();
			if (constants.TryGetValue(value3 + "_gender", out var value4))
			{
				if (Enum.TryParse<Gender>(value4, out var result))
				{
					string value5 = GrammarResolverSimple.ResolveGenderSymbol(result, animal: false, args2, match2.Value);
					text = text.Remove(match2.Index, match2.Length);
					text = text.Insert(match2.Index, value5);
				}
				else
				{
					Log.Warning("Unknown gender: " + value4 + ".");
				}
			}
			else
			{
				Log.Warning("Cannot find rules for pawn symbol " + value3 + ".");
			}
		}
		return text;
	}

	private static void AddRule(Rule rule)
	{
		List<RuleEntry> value = null;
		if (!rules.TryGetValue(rule.keyword, out value))
		{
			value = rulePool.Get();
			value.Clear();
			rules[rule.keyword] = value;
		}
		value.Add(new RuleEntry(rule));
	}

	private static bool TryResolveRecursive(RuleEntry entry, int depth, Dictionary<string, string> constants, out string output, bool log, List<string> extraTags, List<string> resolvedTags, GrammarRequest.ICustomizer customizer)
	{
		string text = "";
		for (int i = 0; i < depth; i++)
		{
			text += "  ";
		}
		if (log && depth > 0)
		{
			logSbTrace.AppendLine();
			logSbTrace.Append(depth.ToStringCached().PadRight(3));
			logSbTrace.Append(text + entry);
		}
		text += "     ";
		loopCount++;
		if (loopCount > 1000)
		{
			Log.Error("Hit loops limit resolving grammar.");
			output = "HIT_LOOPS_LIMIT";
			if (log)
			{
				logSbTrace.Append("\n" + text + "UNRESOLVABLE: Hit loops limit");
			}
			return false;
		}
		if (depth > 50)
		{
			Log.Error("Grammar recurred too deep while resolving keyword (>" + 50 + " deep)");
			output = "DEPTH_LIMIT_REACHED";
			if (log)
			{
				logSbTrace.Append("\n" + text + "UNRESOLVABLE: Depth limit reached");
			}
			return false;
		}
		string text2 = entry.rule.Generate();
		bool flag = false;
		int num = -1;
		for (int j = 0; j < text2.Length; j++)
		{
			char num2 = text2[j];
			if (num2 == '[')
			{
				num = j;
			}
			if (num2 != ']')
			{
				continue;
			}
			if (num == -1)
			{
				Log.Error("Could not resolve rule because of mismatched brackets: " + text2);
				output = "MISMATCHED_BRACKETS";
				if (log)
				{
					logSbTrace.Append("\n" + text + "UNRESOLVABLE: Mismatched brackets");
				}
				flag = true;
				continue;
			}
			string text3 = text2.Substring(num + 1, j - num - 1);
			while (true)
			{
				RuleEntry ruleEntry = RandomPossiblyResolvableEntry(text3, constants, extraTags, resolvedTags, customizer);
				if (ruleEntry == null)
				{
					entry.MarkKnownUnresolvable();
					output = "CANNOT_RESOLVE_SUBSYMBOL:" + text3;
					if (log)
					{
						logSbTrace.Append("\n" + text + text3 + " → UNRESOLVABLE");
					}
					flag = true;
					break;
				}
				ruleEntry.uses++;
				List<string> list = resolvedTags.ToList();
				if (TryResolveRecursive(ruleEntry, depth + 1, constants, out var output2, log, extraTags, list, customizer))
				{
					text2 = text2.Substring(0, num) + output2 + text2.Substring(j + 1);
					j = num;
					resolvedTags.Clear();
					resolvedTags.AddRange(list);
					if (!ruleEntry.rule.tag.NullOrEmpty() && !resolvedTags.Contains(ruleEntry.rule.tag))
					{
						resolvedTags.Add(ruleEntry.rule.tag);
					}
					customizer?.Notify_RuleUsed(ruleEntry.rule);
					break;
				}
				ruleEntry.MarkKnownUnresolvable();
			}
		}
		output = text2;
		return !flag;
	}

	private static RuleEntry RandomPossiblyResolvableEntry(string keyword, Dictionary<string, string> constants, List<string> extraTags, List<string> resolvedTags, GrammarRequest.ICustomizer customizer)
	{
		List<RuleEntry> list = rules.TryGetValue(keyword);
		if (list == null)
		{
			return null;
		}
		float maxPriority = float.MinValue;
		int maxBucket = 0;
		for (int i = 0; i < list.Count; i++)
		{
			RuleEntry ruleEntry = list[i];
			if (ValidateRule(constants, extraTags, resolvedTags, ruleEntry, customizer) && ruleEntry.SelectionWeight != 0f)
			{
				maxPriority = Mathf.Max(maxPriority, ruleEntry.Priority);
				if (customizer != null)
				{
					maxBucket = Mathf.Max(maxBucket, customizer.PriorityBucket(ruleEntry.rule));
				}
			}
		}
		return list.RandomElementByWeightWithFallback((RuleEntry rule) => (!ValidateRule(constants, extraTags, resolvedTags, rule, customizer) || rule.Priority != maxPriority || (customizer?.PriorityBucket(rule.rule) ?? 0) != maxBucket) ? 0f : rule.SelectionWeight);
	}

	private static bool ValidateRule(Dictionary<string, string> constants, List<string> extraTags, List<string> resolvedTags, RuleEntry rule, GrammarRequest.ICustomizer customizer)
	{
		if (!rule.knownUnresolvable && rule.ValidateConstantConstraints(constants) && rule.ValidateRequiredTag(extraTags, resolvedTags) && rule.ValidateTimesUsed())
		{
			return customizer?.ValidateRule(rule.rule) ?? true;
		}
		return false;
	}

	public static bool ContainsSpecialChars(string str)
	{
		return str.IndexOfAny(SpecialChars) >= 0;
	}
}
