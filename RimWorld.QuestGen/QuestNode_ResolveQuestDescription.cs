using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_ResolveQuestDescription : QuestNode
{
	public SlateRef<RulePack> rules;

	public const string TextRoot = "questDescription";

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		if (rules.GetValue(QuestGen.slate) != null)
		{
			QuestGen.AddQuestDescriptionRules(rules.GetValue(QuestGen.slate));
		}
		Resolve();
	}

	public static void Resolve()
	{
		if (!QuestGen.slate.TryGet<string>("resolvedQuestDescription", out var var) && !QuestGen.quest.hidden)
		{
			var = QuestGenUtility.ResolveAbsoluteText(QuestGen.QuestDescriptionRulesReadOnly, QuestGen.QuestDescriptionConstantsReadOnly, "questDescription");
			QuestGen.slate.Set("resolvedQuestDescription", var);
		}
		QuestGen.quest.description = var;
	}
}
