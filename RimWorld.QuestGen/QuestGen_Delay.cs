using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Delay
{
	public static QuestPart_WorldObjectTimeout WorldObjectTimeout(this Quest quest, WorldObject worldObject, int delayTicks, string inSignalEnable = null, string inSignalDisable = null, bool reactivatable = false, List<string> outSignalsCompleted = null, bool isQuestTimeout = true)
	{
		QuestPart_WorldObjectTimeout questPart_WorldObjectTimeout = new QuestPart_WorldObjectTimeout();
		questPart_WorldObjectTimeout.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_WorldObjectTimeout.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable);
		questPart_WorldObjectTimeout.worldObject = worldObject;
		questPart_WorldObjectTimeout.reactivatable = reactivatable;
		questPart_WorldObjectTimeout.delayTicks = delayTicks;
		questPart_WorldObjectTimeout.outSignalsCompleted = outSignalsCompleted;
		questPart_WorldObjectTimeout.destroyOnCleanup = true;
		if (isQuestTimeout)
		{
			questPart_WorldObjectTimeout.isBad = true;
			questPart_WorldObjectTimeout.expiryInfoPart = "QuestExpiresIn".Translate();
			questPart_WorldObjectTimeout.expiryInfoPartTip = "QuestExpiresOn".Translate();
		}
		quest.AddPart(questPart_WorldObjectTimeout);
		return questPart_WorldObjectTimeout;
	}

	public static QuestPart_Delay Delay(this Quest quest, int delayTicks, Action inner, string inSignalEnable = null, string inSignalDisable = null, string outSignalComplete = null, bool reactivatable = false, IEnumerable<ISelectable> inspectStringTargets = null, string inspectString = null, bool isQuestTimeout = false, string expiryInfoPart = null, string expiryInfoPartTip = null, string debugLabel = null, bool tickHistorically = false, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, bool waitUntilPlayerHasHomeMap = false)
	{
		QuestPart_Delay questPart_Delay = new QuestPart_Delay();
		questPart_Delay.delayTicks = delayTicks;
		questPart_Delay.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Delay.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable);
		questPart_Delay.reactivatable = reactivatable;
		questPart_Delay.signalListenMode = signalListenMode;
		if (!inspectStringTargets.EnumerableNullOrEmpty())
		{
			questPart_Delay.inspectString = inspectString;
			questPart_Delay.inspectStringTargets = new List<ISelectable>();
			questPart_Delay.inspectStringTargets.AddRange(inspectStringTargets);
		}
		if (isQuestTimeout)
		{
			questPart_Delay.isBad = true;
			questPart_Delay.expiryInfoPart = "QuestExpiresIn".Translate();
			questPart_Delay.expiryInfoPartTip = "QuestExpiresOn".Translate();
		}
		else
		{
			questPart_Delay.expiryInfoPart = expiryInfoPart;
			questPart_Delay.expiryInfoPartTip = expiryInfoPartTip;
		}
		questPart_Delay.waitUntilPlayerHasHomeMap = waitUntilPlayerHasHomeMap;
		if (inner != null)
		{
			QuestGenUtility.RunInner(inner, questPart_Delay);
		}
		if (!outSignalComplete.NullOrEmpty())
		{
			questPart_Delay.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete));
		}
		if (!debugLabel.NullOrEmpty())
		{
			questPart_Delay.debugLabel = debugLabel;
		}
		quest.AddPart(questPart_Delay);
		return questPart_Delay;
	}
}
