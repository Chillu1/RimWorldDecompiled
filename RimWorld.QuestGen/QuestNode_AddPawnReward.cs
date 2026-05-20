using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddPawnReward : QuestNode
{
	public SlateRef<Pawn> pawn;

	[NoTranslate]
	public SlateRef<string> inSignalChoiceUsed;

	public SlateRef<bool> rewardDetailsHidden;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Pawn value = pawn.GetValue(slate);
		if (value != null)
		{
			QuestPart_Choice questPart_Choice = new QuestPart_Choice();
			questPart_Choice.inSignalChoiceUsed = QuestGenUtility.HardcodedSignalWithQuestID(inSignalChoiceUsed.GetValue(slate));
			QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
			choice.rewards.Add(new Reward_Pawn
			{
				pawn = value,
				detailsHidden = rewardDetailsHidden.GetValue(slate)
			});
			questPart_Choice.choices.Add(choice);
			QuestGen.quest.AddPart(questPart_Choice);
		}
	}
}
