using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_InspectString : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	public SlateRef<IEnumerable<ISelectable>> targets;

	public SlateRef<string> inspectString;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!targets.GetValue(slate).EnumerableNullOrEmpty())
		{
			QuestPart_InspectString questPart_InspectString = new QuestPart_InspectString();
			questPart_InspectString.targets.AddRange(targets.GetValue(slate));
			questPart_InspectString.inspectString = inspectString.GetValue(slate);
			questPart_InspectString.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_InspectString);
		}
	}
}
