using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public static class QuestGenUtility
{
	public const string OuterNodeCompletedSignal = "OuterNodeCompleted";

	private static HashSet<string> tmpAddedSlateVars = new HashSet<string>();

	private static StringBuilder tmpSymbol = new StringBuilder();

	private static StringBuilder tmpVarAbsoluteName = new StringBuilder();

	private static List<string> tmpPathParts = new List<string>();

	private static StringBuilder tmpSb = new StringBuilder();

	public static string HardcodedSignalWithQuestID(string signal)
	{
		if (!QuestGen.Working)
		{
			return signal;
		}
		if (signal.NullOrEmpty())
		{
			return null;
		}
		if (signal.StartsWith("Quest") && signal.IndexOf('.') >= 0)
		{
			return signal;
		}
		if (signal.IndexOf('.') >= 0)
		{
			int num = signal.IndexOf('.');
			string text = signal.Substring(0, num);
			string text2 = signal.Substring(num + 1);
			if (!QuestGen.slate.CurrentPrefix.NullOrEmpty())
			{
				text = QuestGen.slate.CurrentPrefix + "/" + text;
			}
			text = NormalizeVarPath(text);
			QuestGen.AddSlateQuestTagToAddWhenFinished(text);
			return QuestGen.GenerateNewSignal(text + "." + text2, ensureUnique: false);
		}
		if (!QuestGen.slate.CurrentPrefix.NullOrEmpty())
		{
			signal = QuestGen.slate.CurrentPrefix + "/" + signal;
		}
		signal = NormalizeVarPath(signal);
		return QuestGen.GenerateNewSignal(signal, ensureUnique: false);
	}

	public static string HardcodedTargetQuestTagWithQuestID(string questTag)
	{
		if (!QuestGen.Working)
		{
			return questTag;
		}
		if (questTag.NullOrEmpty())
		{
			return null;
		}
		if (questTag.StartsWith("Quest") && questTag.IndexOf('.') >= 0)
		{
			return questTag;
		}
		if (!QuestGen.slate.CurrentPrefix.NullOrEmpty())
		{
			questTag = QuestGen.slate.CurrentPrefix + "/" + questTag;
		}
		questTag = NormalizeVarPath(questTag);
		return QuestGen.GenerateNewTargetQuestTag(questTag, ensureUnique: false);
	}

	public static string QuestTagSignal(string questTag, string signal)
	{
		return questTag + "." + signal;
	}

	public static void RunInner(Action inner, QuestPartActivable outerQuestPart)
	{
		string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
		outerQuestPart.outSignalsCompleted.Add(text);
		RunInner(inner, text);
	}

	public static void RunInner(Action inner, string innerNodeInSignal)
	{
		Slate.VarRestoreInfo restoreInfo = QuestGen.slate.GetRestoreInfo("inSignal");
		QuestGen.slate.Set("inSignal", innerNodeInSignal);
		try
		{
			inner();
		}
		finally
		{
			QuestGen.slate.Restore(restoreInfo);
		}
	}

	public static void RunInnerNode(QuestNode node, QuestPartActivable outerQuestPart)
	{
		string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
		outerQuestPart.outSignalsCompleted.Add(text);
		RunInnerNode(node, text);
	}

	public static void RunInnerNode(QuestNode node, string innerNodeInSignal)
	{
		Slate.VarRestoreInfo restoreInfo = QuestGen.slate.GetRestoreInfo("inSignal");
		QuestGen.slate.Set("inSignal", innerNodeInSignal);
		try
		{
			node.Run();
		}
		finally
		{
			QuestGen.slate.Restore(restoreInfo);
		}
	}

	public static void AddSlateVars(ref GrammarRequest req)
	{
		tmpAddedSlateVars.Clear();
		List<Rule> rules = req.Rules;
		for (int i = 0; i < rules.Count; i++)
		{
			if (!(rules[i] is Rule_String rule_String))
			{
				continue;
			}
			string text = rule_String.Generate();
			if (text != null)
			{
				bool flag = false;
				tmpSymbol.Clear();
				for (int j = 0; j < text.Length; j++)
				{
					if (text[j] == '[')
					{
						flag = true;
					}
					else if (text[j] == ']')
					{
						AddSlateVar(ref req, tmpSymbol.ToString(), tmpAddedSlateVars);
						tmpSymbol.Clear();
						flag = false;
					}
					else if (flag)
					{
						tmpSymbol.Append(text[j]);
					}
				}
			}
			if (rule_String.constantConstraints != null)
			{
				for (int k = 0; k < rule_String.constantConstraints.Count; k++)
				{
					string key = rule_String.constantConstraints[k].key;
					AddSlateVar(ref req, key, tmpAddedSlateVars);
				}
			}
		}
	}

	private static void AddSlateVar(ref GrammarRequest req, string absoluteName, HashSet<string> added)
	{
		if (absoluteName == null)
		{
			return;
		}
		tmpVarAbsoluteName.Clear();
		tmpVarAbsoluteName.Append(absoluteName);
		while (tmpVarAbsoluteName.Length > 0)
		{
			string text = tmpVarAbsoluteName.ToString();
			if (added.Contains(text))
			{
				break;
			}
			if (QuestGen.slate.TryGet<object>(text, out var var, isAbsoluteName: true))
			{
				AddSlateVar(ref req, text, var);
				added.Add(text);
				break;
			}
			if (char.IsNumber(tmpVarAbsoluteName[tmpVarAbsoluteName.Length - 1]))
			{
				while (char.IsNumber(tmpVarAbsoluteName[tmpVarAbsoluteName.Length - 1]))
				{
					tmpVarAbsoluteName.Length--;
				}
				continue;
			}
			int num = text.LastIndexOf('_');
			if (num >= 0)
			{
				int num2 = text.LastIndexOf('/');
				if (num >= num2)
				{
					tmpVarAbsoluteName.Length = num;
					continue;
				}
				break;
			}
			break;
		}
	}

	private static void AddSlateVar(ref GrammarRequest req, string absoluteName, object obj)
	{
		if (obj == null)
		{
			return;
		}
		if (obj is BodyPartRecord)
		{
			req.Rules.AddRange(GrammarUtility.RulesForBodyPartRecord(absoluteName, (BodyPartRecord)obj));
		}
		else if (obj is Def)
		{
			req.Rules.AddRange(GrammarUtility.RulesForDef(absoluteName, (Def)obj));
		}
		else if (obj is Faction)
		{
			Faction faction = (Faction)obj;
			req.Rules.AddRange(GrammarUtility.RulesForFaction(absoluteName, faction, req.Constants));
			if (faction.leader != null)
			{
				req.Rules.AddRange(GrammarUtility.RulesForPawn(absoluteName + "_leader", faction.leader, req.Constants));
			}
		}
		else if (obj is Pawn)
		{
			Pawn pawn = (Pawn)obj;
			req.Rules.AddRange(GrammarUtility.RulesForPawn(absoluteName, pawn, req.Constants));
			if (pawn.Faction != null)
			{
				req.Rules.AddRange(GrammarUtility.RulesForFaction(absoluteName + "_faction", pawn.Faction, req.Constants));
			}
		}
		else if (obj is Thing thing)
		{
			req.Rules.AddRange(GrammarUtility.RulesForThing(absoluteName, thing));
		}
		else if (obj is WorldObject)
		{
			req.Rules.AddRange(GrammarUtility.RulesForWorldObject(absoluteName, (WorldObject)obj));
		}
		else if (obj is Map)
		{
			req.Rules.AddRange(GrammarUtility.RulesForWorldObject(absoluteName, ((Map)obj).Parent));
		}
		else if (obj is IntVec2)
		{
			req.Rules.Add(new Rule_String(absoluteName, ((IntVec2)obj).ToStringCross()));
		}
		else if (obj is IEnumerable && !(obj is string))
		{
			if (obj is IEnumerable<Thing>)
			{
				req.Rules.Add(new Rule_String(absoluteName, GenLabel.ThingsLabel(((IEnumerable<Thing>)obj).Where((Thing x) => x != null))));
			}
			else if (obj is IEnumerable<Pawn>)
			{
				req.Rules.Add(new Rule_String(absoluteName, GenLabel.ThingsLabel(((IEnumerable<Pawn>)obj).Where((Pawn x) => x != null).Cast<Thing>())));
			}
			else if (obj is IEnumerable<object> && ((IEnumerable<object>)obj).Any() && ((IEnumerable<object>)obj).All((object x) => x is Thing))
			{
				req.Rules.Add(new Rule_String(absoluteName, GenLabel.ThingsLabel(((IEnumerable<object>)obj).Where((object x) => x != null).Cast<Thing>())));
			}
			else if (obj is IEnumerable<WorldObject> source)
			{
				req.Rules.Add(new Rule_String(absoluteName, source.Select(PossiblyWithTags).ToCommaList(useAnd: true)));
			}
			else
			{
				List<string> list = new List<string>();
				foreach (object item in (IEnumerable)obj)
				{
					if (item != null)
					{
						list.Add(item.ToString());
					}
				}
				req.Rules.Add(new Rule_String(absoluteName, list.ToCommaList(useAnd: true)));
			}
			req.Rules.Add(new Rule_String(absoluteName + "_count", ((IEnumerable)obj).EnumerableCount().ToString()));
			int num = 0;
			foreach (object item2 in (IEnumerable)obj)
			{
				AddSlateVar(ref req, absoluteName + num, item2);
				num++;
			}
		}
		else if (obj is Ideo)
		{
			req.Rules.AddRange(GrammarUtility.RulesForIdeo(absoluteName, (Ideo)obj));
		}
		else if (obj is Precept precept)
		{
			req.Rules.AddRange(GrammarUtility.RulesForPrecept(absoluteName, precept));
		}
		else
		{
			req.Rules.Add(new Rule_String(absoluteName, obj.ToString()));
			if (ConvertHelper.CanConvert<int>(obj))
			{
				req.Rules.Add(new Rule_String(absoluteName + "_duration", ConvertHelper.Convert<int>(obj).ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: true, allowYears: false).Colorize(ColoredText.DateTimeColor)));
			}
			if (ConvertHelper.CanConvert<float>(obj))
			{
				req.Rules.Add(new Rule_String(absoluteName + "_money", ConvertHelper.Convert<float>(obj).ToStringMoney()));
			}
			if (ConvertHelper.CanConvert<float>(obj))
			{
				req.Rules.Add(new Rule_String(absoluteName + "_percent", ConvertHelper.Convert<float>(obj).ToStringPercent()));
			}
			if (ConvertHelper.CanConvert<FloatRange>(obj))
			{
				AddSlateVar(ref req, absoluteName + "_average", ConvertHelper.Convert<FloatRange>(obj).Average);
			}
			if (ConvertHelper.CanConvert<FloatRange>(obj))
			{
				AddSlateVar(ref req, absoluteName + "_min", ConvertHelper.Convert<FloatRange>(obj).min);
			}
			if (ConvertHelper.CanConvert<FloatRange>(obj))
			{
				AddSlateVar(ref req, absoluteName + "_max", ConvertHelper.Convert<FloatRange>(obj).max);
			}
		}
		if (obj is Def)
		{
			if (!req.Constants.ContainsKey(absoluteName))
			{
				req.Constants.Add(absoluteName, ((Def)obj).defName);
			}
		}
		else if (obj is Faction)
		{
			if (!req.Constants.ContainsKey(absoluteName))
			{
				req.Constants.Add(absoluteName, ((Faction)obj).def.defName);
			}
		}
		else if ((obj.GetType().IsPrimitive || obj is string || obj.GetType().IsEnum) && !req.Constants.ContainsKey(absoluteName))
		{
			req.Constants.Add(absoluteName, obj.ToString());
		}
		if (obj is IEnumerable && !(obj is string))
		{
			string key = absoluteName + "_count";
			if (!req.Constants.ContainsKey(key))
			{
				req.Constants.Add(key, ((IEnumerable)obj).EnumerableCount().ToString());
			}
		}
		static string PossiblyWithTags(WorldObject w)
		{
			string text = Find.ActiveLanguageWorker.WithDefiniteArticle(w.Label, plural: false, w.HasName);
			if (w.Faction == null || !w.HasName)
			{
				return text;
			}
			return text.ApplyTag(TagType.Settlement, w.Faction.GetUniqueLoadID()).Resolve();
		}
	}

	public static string ResolveLocalTextWithDescriptionRules(RulePack localRules, string localRootKeyword = "root")
	{
		List<Rule> list = new List<Rule>();
		list.AddRange(QuestGen.QuestDescriptionRulesReadOnly);
		if (localRules != null)
		{
			list.AddRange(AppendCurrentPrefix(localRules.Rules));
		}
		string text = localRootKeyword;
		if (!QuestGen.slate.CurrentPrefix.NullOrEmpty())
		{
			text = QuestGen.slate.CurrentPrefix + "/" + text;
		}
		text = NormalizeVarPath(text);
		return ResolveAbsoluteText(list, QuestGen.QuestDescriptionConstantsReadOnly, text, capitalizeFirstSentence: false);
	}

	public static string ResolveLocalText(RulePack localRules, string localRootKeyword = "root")
	{
		return ResolveLocalText(localRules?.Rules, null, localRootKeyword);
	}

	public static string ResolveLocalText(List<Rule> localRules, Dictionary<string, string> localConstants = null, string localRootKeyword = "root", bool capitalizeFirstSentence = true)
	{
		string text = localRootKeyword;
		if (!QuestGen.slate.CurrentPrefix.NullOrEmpty())
		{
			text = QuestGen.slate.CurrentPrefix + "/" + text;
		}
		text = NormalizeVarPath(text);
		return ResolveAbsoluteText(AppendCurrentPrefix(localRules), AppendCurrentPrefix(localConstants), text, capitalizeFirstSentence);
	}

	public static string ResolveAbsoluteText(List<Rule> absoluteRules, Dictionary<string, string> absoluteConstants = null, string absoluteRootKeyword = "root", bool capitalizeFirstSentence = true)
	{
		GrammarRequest req = default(GrammarRequest);
		if (absoluteRules != null)
		{
			req.Rules.AddRange(absoluteRules);
		}
		if (absoluteConstants != null)
		{
			foreach (KeyValuePair<string, string> absoluteConstant in absoluteConstants)
			{
				req.Constants.Add(absoluteConstant.Key, absoluteConstant.Value);
			}
		}
		AddSlateVars(ref req);
		return GrammarResolver.Resolve(absoluteRootKeyword, req, null, forceLog: false, null, null, null, capitalizeFirstSentence);
	}

	public static List<Rule> AppendCurrentPrefix(List<Rule> rules)
	{
		if (rules == null)
		{
			return null;
		}
		List<Rule> list = new List<Rule>();
		string currentPrefix = QuestGen.slate.CurrentPrefix;
		for (int i = 0; i < rules.Count; i++)
		{
			Rule rule = rules[i].DeepCopy();
			if (!currentPrefix.NullOrEmpty())
			{
				rule.keyword = currentPrefix + "/" + rule.keyword;
			}
			rule.keyword = NormalizeVarPath(rule.keyword);
			if (rule is Rule_String { OutputNull: false } rule_String)
			{
				rule_String.AppendPrefixToAllKeywords(currentPrefix);
			}
			list.Add(rule);
		}
		return list;
	}

	public static Dictionary<string, string> AppendCurrentPrefix(Dictionary<string, string> constants)
	{
		if (constants == null)
		{
			return null;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string currentPrefix = QuestGen.slate.CurrentPrefix;
		foreach (KeyValuePair<string, string> constant in constants)
		{
			string text = constant.Key;
			if (!currentPrefix.NullOrEmpty())
			{
				text = currentPrefix + "/" + text;
			}
			text = NormalizeVarPath(text);
			dictionary.Add(text, constant.Value);
		}
		return dictionary;
	}

	public static LookTargets ToLookTargets(SlateRef<IEnumerable<object>> objects, Slate slate)
	{
		if (objects.GetValue(slate) == null || !objects.GetValue(slate).Any())
		{
			return LookTargets.Invalid;
		}
		return ToLookTargets(objects.GetValue(slate));
	}

	public static LookTargets ToLookTargets(IEnumerable<object> objects)
	{
		if (objects == null || !objects.Any())
		{
			return LookTargets.Invalid;
		}
		LookTargets lookTargets = new LookTargets();
		foreach (object @object in objects)
		{
			if (@object is Thing)
			{
				lookTargets.targets.Add((Thing)@object);
			}
			else if (@object is WorldObject)
			{
				lookTargets.targets.Add((WorldObject)@object);
			}
			else if (@object is Map)
			{
				lookTargets.targets.Add(((Map)@object).Parent);
			}
			else if (@object is GlobalTargetInfo)
			{
				lookTargets.targets.Add((GlobalTargetInfo)@object);
			}
		}
		return lookTargets;
	}

	public static List<Rule> MergeRules(RulePack rules, string singleRule, string root)
	{
		List<Rule> list = new List<Rule>();
		if (rules != null)
		{
			list.AddRange(rules.Rules);
		}
		if (!singleRule.NullOrEmpty())
		{
			list.Add(new Rule_String(root, singleRule));
		}
		return list;
	}

	public static ChoiceLetter MakeLetter(string labelKeyword, string textKeyword, LetterDef def, Faction relatedFaction = null, Quest quest = null)
	{
		ChoiceLetter letter = LetterMaker.MakeLetter("error", "error", def, relatedFaction, quest);
		QuestGen.AddTextRequest(labelKeyword, delegate(string x)
		{
			letter.Label = x;
		});
		QuestGen.AddTextRequest(textKeyword, delegate(string x)
		{
			letter.Text = x;
		});
		return letter;
	}

	public static ChoiceLetter MakeLetter(string labelKeyword, string textKeyword, LetterDef def, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null)
	{
		ChoiceLetter letter = LetterMaker.MakeLetter("error", "error", def, lookTargets, relatedFaction, quest);
		QuestGen.AddTextRequest(labelKeyword, delegate(string x)
		{
			letter.Label = x;
		});
		QuestGen.AddTextRequest(textKeyword, delegate(string x)
		{
			letter.Text = x;
		});
		return letter;
	}

	public static void AddToOrMakeList(Slate slate, string name, object obj)
	{
		if (!slate.TryGet<List<object>>(name, out var var))
		{
			var = new List<object>();
		}
		var.Add(obj);
		slate.Set(name, var);
	}

	public static void AddRangeToOrMakeList(Slate slate, string name, List<object> objs)
	{
		if (!objs.NullOrEmpty())
		{
			if (!slate.TryGet<List<object>>(name, out var var))
			{
				var = new List<object>();
			}
			var.AddRange(objs);
			slate.Set(name, var);
		}
	}

	public static bool IsInList(Slate slate, string name, object obj)
	{
		if (!slate.TryGet<List<object>>(name, out var var) || var == null)
		{
			return false;
		}
		return var.Contains(obj);
	}

	public static List<Slate.VarRestoreInfo> SetVarsForPrefix(List<PrefixCapturedVar> capturedVars, string prefix, Slate slate)
	{
		if (capturedVars.NullOrEmpty())
		{
			return null;
		}
		if (prefix.NullOrEmpty())
		{
			List<Slate.VarRestoreInfo> list = new List<Slate.VarRestoreInfo>();
			for (int i = 0; i < capturedVars.Count; i++)
			{
				list.Add(slate.GetRestoreInfo(capturedVars[i].name));
			}
			for (int j = 0; j < capturedVars.Count; j++)
			{
				if (capturedVars[j].value.TryGetValue(slate, out var value))
				{
					if (capturedVars[j].name == "inSignal" && value is string)
					{
						value = HardcodedSignalWithQuestID((string)value);
					}
					slate.Set(capturedVars[j].name, value);
				}
			}
			return list;
		}
		for (int k = 0; k < capturedVars.Count; k++)
		{
			if (capturedVars[k].value.TryGetValue(slate, out var value2))
			{
				if (capturedVars[k].name == "inSignal" && value2 is string)
				{
					value2 = HardcodedSignalWithQuestID((string)value2);
				}
				string name = prefix + "/" + capturedVars[k].name;
				slate.Set(name, value2);
			}
		}
		return null;
	}

	public static void RestoreVarsForPrefix(List<Slate.VarRestoreInfo> varsRestoreInfo, Slate slate)
	{
		if (!varsRestoreInfo.NullOrEmpty())
		{
			for (int i = 0; i < varsRestoreInfo.Count; i++)
			{
				slate.Restore(varsRestoreInfo[i]);
			}
		}
	}

	public static void GetReturnedVars(List<SlateRef<string>> varNames, string prefix, Slate slate)
	{
		if (varNames.NullOrEmpty() || prefix.NullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < varNames.Count; i++)
		{
			string name = prefix + "/" + varNames[i].GetValue(slate);
			if (slate.TryGet<object>(name, out var var))
			{
				slate.Set(varNames[i].GetValue(slate), var);
			}
		}
	}

	public static string NormalizeVarPath(string path)
	{
		if (path.NullOrEmpty())
		{
			return path;
		}
		if (!path.Contains(".."))
		{
			return path;
		}
		tmpSb.Length = 0;
		tmpPathParts.Clear();
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] == '/')
			{
				tmpPathParts.Add(tmpSb.ToString());
				tmpSb.Length = 0;
			}
			else
			{
				tmpSb.Append(path[i]);
			}
		}
		if (tmpSb.Length != 0)
		{
			tmpPathParts.Add(tmpSb.ToString());
		}
		for (int j = 0; j < tmpPathParts.Count; j++)
		{
			while (j < tmpPathParts.Count && tmpPathParts[j] == "..")
			{
				if (j == 0)
				{
					tmpPathParts.RemoveAt(0);
					continue;
				}
				tmpPathParts.RemoveAt(j);
				tmpPathParts.RemoveAt(j - 1);
				j--;
			}
		}
		tmpSb.Length = 0;
		for (int k = 0; k < tmpPathParts.Count; k++)
		{
			if (k != 0)
			{
				tmpSb.Append('/');
			}
			tmpSb.Append(tmpPathParts[k]);
		}
		return tmpSb.ToString();
	}

	public static void RunAdjustPointsForDistantFight()
	{
		QuestScriptDefOf.Util_AdjustPointsForDistantFight.root.Run();
	}

	public static void TestRunAdjustPointsForDistantFight(Slate slate)
	{
		QuestScriptDefOf.Util_AdjustPointsForDistantFight.root.TestRun(slate);
	}
}
