using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GiveRoyalFavor : QuestNode
	{
		public SlateRef<Pawn> giveTo;

		public SlateRef<bool> giveToAccepter;

		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<Faction> faction;

		public SlateRef<Thing> factionOf;

		public SlateRef<int> amount;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = new QuestPart_GiveRoyalFavor();
			questPart_GiveRoyalFavor.giveTo = giveTo.GetValue(slate);
			questPart_GiveRoyalFavor.giveToAccepter = giveToAccepter.GetValue(slate);
			questPart_GiveRoyalFavor.faction = (faction.GetValue(slate) ?? factionOf.GetValue(slate).Faction);
			questPart_GiveRoyalFavor.amount = amount.GetValue(slate);
			questPart_GiveRoyalFavor.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			QuestGen.quest.AddPart(questPart_GiveRoyalFavor);
		}
	}
}
