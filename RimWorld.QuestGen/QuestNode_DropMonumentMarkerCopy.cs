using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_DropMonumentMarkerCopy : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	[NoTranslate]
	public SlateRef<string> outSignalResult;

	public SlateRef<bool> destroyOrPassToWorldOnCleanup;

	protected override bool TestRunInt(Slate slate)
	{
		return slate.Exists("map");
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_DropMonumentMarkerCopy questPart_DropMonumentMarkerCopy = new QuestPart_DropMonumentMarkerCopy();
		questPart_DropMonumentMarkerCopy.mapParent = slate.Get<Map>("map").Parent;
		questPart_DropMonumentMarkerCopy.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
		questPart_DropMonumentMarkerCopy.outSignalResult = QuestGenUtility.HardcodedSignalWithQuestID(outSignalResult.GetValue(slate));
		questPart_DropMonumentMarkerCopy.destroyOrPassToWorldOnCleanup = destroyOrPassToWorldOnCleanup.GetValue(slate);
		QuestGen.quest.AddPart(questPart_DropMonumentMarkerCopy);
	}
}
