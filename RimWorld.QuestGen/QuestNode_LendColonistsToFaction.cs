using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_LendColonistsToFaction : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignalEnable;

		[NoTranslate]
		public SlateRef<string> outSignalComplete;

		public SlateRef<Thing> shuttle;

		public SlateRef<Pawn> lendColonistsToFactionOf;

		public SlateRef<int> returnLentColonistsInTicks;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_LendColonistsToFaction questPart_LendColonistsToFaction = new QuestPart_LendColonistsToFaction
			{
				inSignalEnable = (QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal")),
				shuttle = shuttle.GetValue(slate),
				lendColonistsToFaction = lendColonistsToFactionOf.GetValue(slate).Faction,
				returnLentColonistsInTicks = returnLentColonistsInTicks.GetValue(slate),
				returnMap = slate.Get<Map>("map").Parent
			};
			if (!outSignalComplete.GetValue(slate).NullOrEmpty())
			{
				questPart_LendColonistsToFaction.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
			}
			QuestGen.quest.AddPart(questPart_LendColonistsToFaction);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
