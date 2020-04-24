using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddMemoryThought : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<ThoughtDef> def;

		public SlateRef<Pawn> pawn;

		public SlateRef<Pawn> otherPawn;

		public SlateRef<bool?> addToLookTargets;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_AddMemoryThought questPart_AddMemoryThought = new QuestPart_AddMemoryThought();
			questPart_AddMemoryThought.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			questPart_AddMemoryThought.def = def.GetValue(slate);
			questPart_AddMemoryThought.pawn = pawn.GetValue(slate);
			questPart_AddMemoryThought.otherPawn = otherPawn.GetValue(slate);
			questPart_AddMemoryThought.addToLookTargets = (addToLookTargets.GetValue(slate) ?? true);
			QuestGen.quest.AddPart(questPart_AddMemoryThought);
		}
	}
}
