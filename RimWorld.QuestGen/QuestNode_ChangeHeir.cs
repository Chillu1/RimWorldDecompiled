using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ChangeHeir : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<Faction> faction;

	public SlateRef<Thing> factionOf;

	public SlateRef<Pawn> holder;

	public SlateRef<Pawn> heir;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_ChangeHeir questPart_ChangeHeir = new QuestPart_ChangeHeir();
		questPart_ChangeHeir.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_ChangeHeir.faction = faction.GetValue(slate) ?? factionOf.GetValue(slate).Faction;
		questPart_ChangeHeir.holder = holder.GetValue(slate);
		questPart_ChangeHeir.heir = heir.GetValue(slate);
		QuestGen.quest.AddPart(questPart_ChangeHeir);
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
