using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SetFactionRelations : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<Faction> faction;

		public SlateRef<FactionRelationKind> relationKind;

		public SlateRef<bool?> sendLetter;

		private const string RootSymbol = "root";

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_SetFactionRelations questPart_SetFactionRelations = new QuestPart_SetFactionRelations();
			questPart_SetFactionRelations.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SetFactionRelations.faction = faction.GetValue(slate);
			questPart_SetFactionRelations.relationKind = relationKind.GetValue(slate);
			questPart_SetFactionRelations.canSendLetter = sendLetter.GetValue(slate) ?? true;
			QuestGen.quest.AddPart(questPart_SetFactionRelations);
		}
	}
}
