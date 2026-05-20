using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Delay : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	public SlateRef<string> expiryInfoPart;

	public SlateRef<string> expiryInfoPartTip;

	public SlateRef<string> inspectString;

	public SlateRef<IEnumerable<ISelectable>> inspectStringTargets;

	public SlateRef<int> delayTicks;

	public SlateRef<IntRange?> delayTicksRange;

	public SlateRef<bool> isQuestTimeout;

	public SlateRef<bool> reactivatable;

	public SlateRef<bool> waitUntilPlayerHasHomeMap;

	public SlateRef<bool> useAcceptanceExpiry;

	public QuestNode node;

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
		QuestPart_Delay questPart_Delay;
		if (useAcceptanceExpiry.GetValue(slate))
		{
			questPart_Delay = MakeDelayQuestPart();
			questPart_Delay.delayTicks = QuestGen.quest.TicksUntilExpiry;
		}
		else if (delayTicksRange.GetValue(slate).HasValue)
		{
			questPart_Delay = new QuestPart_DelayRandom();
			((QuestPart_DelayRandom)questPart_Delay).delayTicksRange = delayTicksRange.GetValue(slate).Value;
		}
		else
		{
			questPart_Delay = MakeDelayQuestPart();
			questPart_Delay.delayTicks = delayTicks.GetValue(slate);
		}
		questPart_Delay.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Delay.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		questPart_Delay.reactivatable = reactivatable.GetValue(slate);
		questPart_Delay.waitUntilPlayerHasHomeMap = waitUntilPlayerHasHomeMap.GetValue(slate);
		if (!inspectStringTargets.GetValue(slate).EnumerableNullOrEmpty())
		{
			questPart_Delay.inspectString = inspectString.GetValue(slate);
			questPart_Delay.inspectStringTargets = new List<ISelectable>();
			questPart_Delay.inspectStringTargets.AddRange(inspectStringTargets.GetValue(slate));
		}
		if (isQuestTimeout.GetValue(slate))
		{
			questPart_Delay.isBad = true;
			questPart_Delay.expiryInfoPart = "QuestExpiresIn".Translate();
			questPart_Delay.expiryInfoPartTip = "QuestExpiresOn".Translate();
		}
		else
		{
			questPart_Delay.expiryInfoPart = expiryInfoPart.GetValue(slate);
			questPart_Delay.expiryInfoPartTip = expiryInfoPartTip.GetValue(slate);
		}
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_Delay);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_Delay.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_Delay);
	}

	protected virtual QuestPart_Delay MakeDelayQuestPart()
	{
		return new QuestPart_Delay();
	}
}
