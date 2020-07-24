using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddItemsReward : QuestNode
	{
		public SlateRef<IEnumerable<Thing>> items;

		[NoTranslate]
		public SlateRef<string> inSignalChoiceUsed;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			IEnumerable<Thing> value = items.GetValue(slate);
			if (!value.EnumerableNullOrEmpty())
			{
				QuestPart_Choice questPart_Choice = new QuestPart_Choice();
				questPart_Choice.inSignalChoiceUsed = QuestGenUtility.HardcodedSignalWithQuestID(inSignalChoiceUsed.GetValue(slate));
				QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
				Reward_Items reward_Items = new Reward_Items();
				reward_Items.items.AddRange(value);
				choice.rewards.Add(reward_Items);
				questPart_Choice.choices.Add(choice);
				QuestGen.quest.AddPart(questPart_Choice);
			}
		}
	}
}
