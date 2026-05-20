using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_MoodBelow : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignal;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<float> threshold;

	public QuestNode node;

	private const int MinTicksBelowMinMood = 40000;

	protected override bool TestRunInt(Slate slate)
	{
		if (node != null)
		{
			return node.TestRun(slate);
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_MoodBelow questPart_MoodBelow = new QuestPart_MoodBelow();
			questPart_MoodBelow.pawns.AddRange(pawns.GetValue(slate));
			questPart_MoodBelow.threshold = threshold.GetValue(slate);
			questPart_MoodBelow.minTicksBelowThreshold = 40000;
			questPart_MoodBelow.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			if (node != null)
			{
				QuestGenUtility.RunInnerNode(node, questPart_MoodBelow);
			}
			if (!outSignal.GetValue(slate).NullOrEmpty())
			{
				questPart_MoodBelow.outSignalsCompleted.Add(outSignal.GetValue(slate));
			}
			QuestGen.quest.AddPart(questPart_MoodBelow);
		}
	}
}
