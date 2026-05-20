using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddPassageOffworldReward : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalChoiceUsed;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_Choice questPart_Choice = new QuestPart_Choice();
		questPart_Choice.inSignalChoiceUsed = QuestGenUtility.HardcodedSignalWithQuestID(inSignalChoiceUsed.GetValue(slate));
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
		choice.rewards.Add(new Reward_PassageOffworld());
		questPart_Choice.choices.Add(choice);
		QuestGen.quest.AddPart(questPart_Choice);
	}
}
