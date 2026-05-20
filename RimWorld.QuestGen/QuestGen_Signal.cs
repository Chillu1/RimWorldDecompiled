using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Signal
{
	public static void Signal(this Quest quest, string inSignal = null, Action action = null, IEnumerable<string> outSignals = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		_ = QuestGen.slate;
		switch ((outSignals?.Count() ?? 0) + ((action != null) ? 1 : 0))
		{
		case 0:
			return;
		case 1:
		{
			QuestPart_Pass questPart_Pass = new QuestPart_Pass();
			questPart_Pass.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal);
			if (action != null)
			{
				questPart_Pass.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInner(action, questPart_Pass.outSignal);
			}
			else
			{
				questPart_Pass.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.First());
			}
			questPart_Pass.signalListenMode = signalListenMode;
			quest.AddPart(questPart_Pass);
			return;
		}
		}
		QuestPart_PassOutMany questPart_PassOutMany = new QuestPart_PassOutMany();
		questPart_PassOutMany.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal);
		if (action != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassOutMany.outSignals.Add(text);
			QuestGenUtility.RunInner(action, text);
		}
		foreach (string outSignal in outSignals)
		{
			questPart_PassOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignal));
		}
		questPart_PassOutMany.signalListenMode = signalListenMode;
		quest.AddPart(questPart_PassOutMany);
	}

	public static void AnySignal(this Quest quest, IEnumerable<string> inSignals = null, Action action = null, IEnumerable<string> outSignals = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		_ = QuestGen.slate;
		switch ((outSignals?.Count() ?? 0) + ((action != null) ? 1 : 0))
		{
		case 0:
			return;
		case 1:
		{
			QuestPart_PassAny questPart_PassAny = new QuestPart_PassAny();
			foreach (string inSignal in inSignals)
			{
				questPart_PassAny.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(inSignal));
			}
			if (action != null)
			{
				questPart_PassAny.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInner(action, questPart_PassAny.outSignal);
			}
			else
			{
				questPart_PassAny.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.First());
			}
			questPart_PassAny.signalListenMode = signalListenMode;
			quest.AddPart(questPart_PassAny);
			return;
		}
		}
		QuestPart_PassAnyOutMany questPart_PassAnyOutMany = new QuestPart_PassAnyOutMany();
		foreach (string inSignal2 in inSignals)
		{
			questPart_PassAnyOutMany.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(inSignal2));
		}
		if (action != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassAnyOutMany.outSignals.Add(text);
			QuestGenUtility.RunInner(action, text);
		}
		foreach (string outSignal in outSignals)
		{
			questPart_PassAnyOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignal));
		}
		questPart_PassAnyOutMany.signalListenMode = signalListenMode;
		quest.AddPart(questPart_PassAnyOutMany);
	}

	public static void SignalRandom(this Quest quest, IEnumerable<Action> actions, string inSignal = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_PassOutRandom questPart_PassOutRandom = new QuestPart_PassOutRandom();
		questPart_PassOutRandom.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_PassOutRandom.signalListenMode = signalListenMode;
		quest.AddPart(questPart_PassOutRandom);
		foreach (Action action in actions)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassOutRandom.outSignals.Add(text);
			QuestGenUtility.RunInner(action, text);
		}
	}

	public static void SendSignals(this Quest quest, IEnumerable<string> outSignals, string outSignalsFormat = "", int outSignalsFormattedCount = 0)
	{
		_ = QuestGen.slate;
		IEnumerable<string> enumerable = Enumerable.Empty<string>();
		if (outSignals != null)
		{
			enumerable = enumerable.Concat(outSignals);
		}
		if (outSignalsFormattedCount > 0)
		{
			for (int i = 0; i < outSignalsFormattedCount; i++)
			{
				enumerable = enumerable.Concat(Gen.YieldSingle(outSignalsFormat.Formatted(i.Named("INDEX")).ToString()));
			}
		}
		if (enumerable.EnumerableNullOrEmpty())
		{
			return;
		}
		if (enumerable.Count() == 1)
		{
			QuestPart_Pass questPart_Pass = new QuestPart_Pass();
			questPart_Pass.inSignal = QuestGen.slate.Get<string>("inSignal");
			questPart_Pass.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(enumerable.First());
			QuestGen.quest.AddPart(questPart_Pass);
			return;
		}
		QuestPart_PassOutMany questPart_PassOutMany = new QuestPart_PassOutMany();
		questPart_PassOutMany.inSignal = QuestGen.slate.Get<string>("inSignal");
		foreach (string item in enumerable)
		{
			questPart_PassOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
		}
		QuestGen.quest.AddPart(questPart_PassOutMany);
	}

	public static void SignalPassOutMany(this Quest quest, Action action = null, string inSignal = null, IEnumerable<string> outSignals = null)
	{
		QuestPart_PassOutMany questPart_PassOutMany = new QuestPart_PassOutMany();
		questPart_PassOutMany.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		if (action != null)
		{
			string innerNodeInSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, innerNodeInSignal);
		}
		if (outSignals != null)
		{
			foreach (string outSignal in outSignals)
			{
				questPart_PassOutMany.outSignals.Add(outSignal);
			}
		}
		quest.AddPart(questPart_PassOutMany);
	}

	public static QuestPart_PassActivable SignalPassActivable(this Quest quest, Action action = null, string inSignalEnable = null, string inSignal = null, string outSignalCompleted = null, IEnumerable<string> outSignalsCompleted = null, string inSignalDisable = null, bool reactivatable = false)
	{
		QuestPart_PassActivable questPart_PassActivable = new QuestPart_PassActivable();
		questPart_PassActivable.inSignalEnable = inSignalEnable ?? QuestGen.slate.Get<string>("inSignal");
		questPart_PassActivable.inSignalDisable = inSignalDisable;
		questPart_PassActivable.inSignal = inSignal;
		questPart_PassActivable.reactivatable = reactivatable;
		if (action != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, text);
			questPart_PassActivable.outSignalsCompleted.Add(text);
		}
		if (outSignalsCompleted != null)
		{
			questPart_PassActivable.outSignalsCompleted.AddRange(outSignalsCompleted);
		}
		if (!outSignalCompleted.NullOrEmpty())
		{
			questPart_PassActivable.outSignalsCompleted.Add(outSignalCompleted);
		}
		quest.AddPart(questPart_PassActivable);
		return questPart_PassActivable;
	}

	public static void SignalPass(this Quest quest, Action action = null, string inSignal = null, string outSignal = null)
	{
		QuestPart_Pass questPart_Pass = new QuestPart_Pass();
		questPart_Pass.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		if (action != null)
		{
			outSignal = outSignal ?? QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, outSignal);
		}
		questPart_Pass.outSignal = outSignal;
		quest.AddPart(questPart_Pass);
	}

	public static void SignalPassAll(this Quest quest, Action action = null, List<string> inSignals = null, string outSignal = null)
	{
		QuestPart_PassAll questPart_PassAll = new QuestPart_PassAll();
		questPart_PassAll.inSignals = inSignals ?? new List<string> { QuestGen.slate.Get<string>("inSignal") };
		if (action != null)
		{
			outSignal = outSignal ?? QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, outSignal);
		}
		questPart_PassAll.outSignal = outSignal;
		quest.AddPart(questPart_PassAll);
	}

	public static void SignalPassAny(this Quest quest, Action action = null, List<string> inSignals = null, string outSignal = null)
	{
		QuestPart_PassAny questPart_PassAny = new QuestPart_PassAny();
		questPart_PassAny.inSignals = inSignals ?? new List<string> { QuestGen.slate.Get<string>("inSignal") };
		if (action != null)
		{
			outSignal = outSignal ?? QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, outSignal);
		}
		questPart_PassAny.outSignal = outSignal;
		quest.AddPart(questPart_PassAny);
	}

	public static void SignalPassAllSequence(this Quest quest, Action action = null, List<string> inSignals = null, string outSignal = null)
	{
		QuestPart_PassAllSequence questPart_PassAllSequence = new QuestPart_PassAllSequence();
		questPart_PassAllSequence.inSignals = inSignals ?? new List<string> { QuestGen.slate.Get<string>("inSignal") };
		if (action != null)
		{
			outSignal = outSignal ?? QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, outSignal);
		}
		questPart_PassAllSequence.outSignal = outSignal;
		quest.AddPart(questPart_PassAllSequence);
	}

	public static void SignalPassWithFaction(this Quest quest, Faction faction, Action action = null, Action outAction = null, string inSignal = null, string outSignal = null)
	{
		QuestPart_PassWithFactionArg questPart_PassWithFactionArg = new QuestPart_PassWithFactionArg();
		questPart_PassWithFactionArg.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		questPart_PassWithFactionArg.faction = faction;
		questPart_PassWithFactionArg.outSignal = outSignal;
		if (action != null)
		{
			string innerNodeInSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			QuestGenUtility.RunInner(action, innerNodeInSignal);
		}
		if (outAction != null)
		{
			if (questPart_PassWithFactionArg.outSignal == null)
			{
				questPart_PassWithFactionArg.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			}
			QuestGenUtility.RunInner(outAction, questPart_PassWithFactionArg.outSignal);
		}
		quest.AddPart(questPart_PassWithFactionArg);
	}
}
