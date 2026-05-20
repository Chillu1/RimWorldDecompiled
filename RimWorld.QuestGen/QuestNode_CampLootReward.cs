namespace RimWorld.QuestGen;

public class QuestNode_CampLootReward : QuestNode
{
	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		QuestPart_Choice questPart_Choice = QuestGen.quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_CampLoot() }
		};
		questPart_Choice.choices.Add(item);
	}
}
