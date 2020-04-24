using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_ChangeFactionGoodwill : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<Faction> faction;

		public SlateRef<Thing> factionOf;

		public SlateRef<int> change;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
			questPart_FactionGoodwillChange.change = change.GetValue(slate);
			questPart_FactionGoodwillChange.faction = (faction.GetValue(slate) ?? factionOf.GetValue(slate).Faction);
			questPart_FactionGoodwillChange.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			QuestGen.quest.AddPart(questPart_FactionGoodwillChange);
		}
	}
}
