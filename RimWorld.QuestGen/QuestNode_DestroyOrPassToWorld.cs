using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_DestroyOrPassToWorld : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Thing>> things;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!things.GetValue(slate).EnumerableNullOrEmpty())
		{
			QuestPart_DestroyThingsOrPassToWorld questPart_DestroyThingsOrPassToWorld = new QuestPart_DestroyThingsOrPassToWorld();
			questPart_DestroyThingsOrPassToWorld.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_DestroyThingsOrPassToWorld.things.AddRange(things.GetValue(slate));
			QuestGen.quest.AddPart(questPart_DestroyThingsOrPassToWorld);
		}
	}
}
