using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_TextRules : QuestNode
	{
		public SlateRef<RulePack> rules;

		public SlateRef<TextRulesTarget> target;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			switch (target.GetValue(slate))
			{
			case TextRulesTarget.Description:
				QuestGen.AddQuestDescriptionRules(rules.GetValue(slate));
				break;
			case TextRulesTarget.Name:
				QuestGen.AddQuestNameRules(rules.GetValue(slate));
				break;
			case TextRulesTarget.DecriptionAndName:
				QuestGen.AddQuestDescriptionRules(rules.GetValue(slate));
				QuestGen.AddQuestNameRules(rules.GetValue(slate));
				break;
			}
		}
	}
}
