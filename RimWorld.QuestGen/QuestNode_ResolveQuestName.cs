using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_ResolveQuestName : QuestNode
{
	public SlateRef<RulePack> rules;

	public const string TextRoot = "questName";

	private const int MaxTriesTryAvoidDuplicateName = 20;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		if (rules.GetValue(QuestGen.slate) != null)
		{
			QuestGen.AddQuestNameRules(rules.GetValue(QuestGen.slate));
		}
		Resolve();
	}

	public static void Resolve()
	{
		if (!QuestGen.slate.TryGet<string>("resolvedQuestName", out var var))
		{
			var = (QuestGen.quest.hidden ? QuestGen.quest.root.defName : GenerateName().StripTags());
			QuestGen.slate.Set("resolvedQuestName", var);
		}
		QuestGen.quest.name = var;
	}

	private static string GenerateName()
	{
		GrammarRequest req = default(GrammarRequest);
		req.Rules.AddRange(QuestGen.QuestNameRulesReadOnly);
		foreach (KeyValuePair<string, string> item in QuestGen.QuestNameConstantsReadOnly)
		{
			req.Constants.Add(item.Key, item.Value);
		}
		QuestGenUtility.AddSlateVars(ref req);
		Predicate<string> predicate = (string x) => !Find.QuestManager.QuestsListForReading.Any((Quest y) => y.name == x);
		if (QuestGen.Root.nameMustBeUnique)
		{
			return NameGenerator.GenerateName(req, predicate, appendNumberIfNameUsed: false, "questName");
		}
		string text = null;
		for (int num = 0; num < 20; num++)
		{
			text = NameGenerator.GenerateName(req, null, appendNumberIfNameUsed: false, "questName");
			if (predicate(text) || QuestGen.Root.defaultHidden)
			{
				break;
			}
		}
		return text;
	}
}
