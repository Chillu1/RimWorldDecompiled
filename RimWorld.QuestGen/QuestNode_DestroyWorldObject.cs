using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_DestroyWorldObject : QuestNode
{
	public SlateRef<WorldObject> worldObject;

	[NoTranslate]
	public SlateRef<string> inSignal;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_DestroyWorldObject questPart_DestroyWorldObject = new QuestPart_DestroyWorldObject();
		questPart_DestroyWorldObject.worldObject = worldObject.GetValue(slate);
		questPart_DestroyWorldObject.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		QuestGen.quest.AddPart(questPart_DestroyWorldObject);
	}
}
