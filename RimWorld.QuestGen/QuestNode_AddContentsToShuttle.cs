using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddContentsToShuttle : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<Thing> shuttle;

	public SlateRef<IEnumerable<Thing>> contents;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (contents.GetValue(slate) != null)
		{
			QuestPart_AddContentsToShuttle questPart_AddContentsToShuttle = new QuestPart_AddContentsToShuttle();
			questPart_AddContentsToShuttle.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
			questPart_AddContentsToShuttle.shuttle = shuttle.GetValue(slate);
			questPart_AddContentsToShuttle.Things = contents.GetValue(slate);
			QuestGen.quest.AddPart(questPart_AddContentsToShuttle);
		}
	}
}
