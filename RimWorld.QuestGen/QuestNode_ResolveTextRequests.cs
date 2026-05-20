using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_ResolveTextRequests : QuestNode
{
	public SlateRef<RulePack> rules;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		if (rules.GetValue(QuestGen.slate) != null)
		{
			QuestGen.AddQuestDescriptionRules(rules.GetValue(QuestGen.slate));
			QuestGen.AddQuestContentRules(rules.GetValue(QuestGen.slate));
		}
		Resolve();
	}

	public static void Resolve()
	{
		List<QuestTextRequest> textRequestsReadOnly = QuestGen.TextRequestsReadOnly;
		for (int i = 0; i < textRequestsReadOnly.Count; i++)
		{
			try
			{
				List<Rule> list = new List<Rule>();
				list.AddRange(QuestGen.QuestDescriptionRulesReadOnly);
				list.AddRange(QuestGen.QuestContentRulesReadOnly);
				if (textRequestsReadOnly[i].extraRules != null)
				{
					list.AddRange(textRequestsReadOnly[i].extraRules);
				}
				string obj = QuestGenUtility.ResolveAbsoluteText(list, QuestGen.QuestDescriptionConstantsReadOnly, textRequestsReadOnly[i].keyword);
				textRequestsReadOnly[i].setter(obj);
			}
			catch (Exception ex)
			{
				Log.Error("Error while resolving text request: " + ex);
			}
		}
		textRequestsReadOnly.Clear();
	}
}
