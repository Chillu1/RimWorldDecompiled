using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public static class QuestGen_Filter
	{
		public static QuestPart_Filter_AnyPawnAlive AnyPawnAlive(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AnyPawnAlive questPart_Filter_AnyPawnAlive = new QuestPart_Filter_AnyPawnAlive();
			questPart_Filter_AnyPawnAlive.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AnyPawnAlive.signalListenMode = signalListenMode;
			questPart_Filter_AnyPawnAlive.pawns = pawns.ToList();
			questPart_Filter_AnyPawnAlive.inSignalRemovePawn = inSignalRemovePawn;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AnyPawnAlive.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyPawnAlive.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AnyPawnAlive);
			return questPart_Filter_AnyPawnAlive;
		}

		public static QuestPart_Filter_AllPawnsDespawned AllPawnsDespawned(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AllPawnsDespawned questPart_Filter_AllPawnsDespawned = new QuestPart_Filter_AllPawnsDespawned();
			questPart_Filter_AllPawnsDespawned.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AllPawnsDespawned.signalListenMode = signalListenMode;
			questPart_Filter_AllPawnsDespawned.pawns = pawns.ToList();
			questPart_Filter_AllPawnsDespawned.inSignalRemovePawn = inSignalRemovePawn;
			questPart_Filter_AllPawnsDespawned.outSignal = outSignal;
			questPart_Filter_AllPawnsDespawned.outSignalElse = outSignalElse;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AllPawnsDespawned.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AllPawnsDespawned.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AllPawnsDespawned);
			return questPart_Filter_AllPawnsDespawned;
		}

		public static QuestPart_Filter_AnyPawnUnhealthy AnyPawnUnhealthy(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AnyPawnUnhealthy questPart_Filter_AnyPawnUnhealthy = new QuestPart_Filter_AnyPawnUnhealthy();
			questPart_Filter_AnyPawnUnhealthy.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AnyPawnUnhealthy.signalListenMode = signalListenMode;
			questPart_Filter_AnyPawnUnhealthy.pawns = pawns.ToList();
			questPart_Filter_AnyPawnUnhealthy.inSignalRemovePawn = inSignalRemovePawn;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AnyPawnUnhealthy.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyPawnUnhealthy.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AnyPawnUnhealthy);
			return questPart_Filter_AnyPawnUnhealthy;
		}

		public static QuestPart_Filter_FactionHostileToOtherFaction FactionHostileToOtherFaction(this Quest quest, Faction faction, Faction other, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_FactionHostileToOtherFaction questPart_Filter_FactionHostileToOtherFaction = new QuestPart_Filter_FactionHostileToOtherFaction();
			questPart_Filter_FactionHostileToOtherFaction.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_FactionHostileToOtherFaction.signalListenMode = signalListenMode;
			questPart_Filter_FactionHostileToOtherFaction.faction = faction;
			questPart_Filter_FactionHostileToOtherFaction.other = other;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_FactionHostileToOtherFaction.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_FactionHostileToOtherFaction.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_FactionHostileToOtherFaction);
			return questPart_Filter_FactionHostileToOtherFaction;
		}

		public static QuestPart_Filter_AnyPawnPlayerControlled AnyPawnPlayerControlled(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AnyPawnPlayerControlled questPart_Filter_AnyPawnPlayerControlled = new QuestPart_Filter_AnyPawnPlayerControlled();
			questPart_Filter_AnyPawnPlayerControlled.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AnyPawnPlayerControlled.signalListenMode = signalListenMode;
			questPart_Filter_AnyPawnPlayerControlled.pawns = pawns.ToList();
			questPart_Filter_AnyPawnPlayerControlled.inSignalRemovePawn = inSignalRemovePawn;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AnyPawnPlayerControlled.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyPawnPlayerControlled.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AnyPawnPlayerControlled);
			return questPart_Filter_AnyPawnPlayerControlled;
		}

		public static QuestPart_Filter_AllPawnsDestroyed AllPawnsDestroyed(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AllPawnsDestroyed questPart_Filter_AllPawnsDestroyed = new QuestPart_Filter_AllPawnsDestroyed();
			questPart_Filter_AllPawnsDestroyed.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AllPawnsDestroyed.signalListenMode = signalListenMode;
			questPart_Filter_AllPawnsDestroyed.pawns = pawns.ToList();
			questPart_Filter_AllPawnsDestroyed.inSignalRemovePawn = inSignalRemovePawn;
			questPart_Filter_AllPawnsDestroyed.outSignal = outSignal;
			questPart_Filter_AllPawnsDestroyed.outSignalElse = outSignalElse;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AllPawnsDestroyed.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AllPawnsDestroyed.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AllPawnsDestroyed);
			return questPart_Filter_AllPawnsDestroyed;
		}

		public static QuestPart_Filter_FactionNonPlayer FactionNonPlayer(this Quest quest, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_FactionNonPlayer questPart_Filter_FactionNonPlayer = new QuestPart_Filter_FactionNonPlayer();
			questPart_Filter_FactionNonPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_FactionNonPlayer.signalListenMode = signalListenMode;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_FactionNonPlayer.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_FactionNonPlayer.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_FactionNonPlayer);
			return questPart_Filter_FactionNonPlayer;
		}

		public static QuestPart_Filter_AllPawnsDowned AllPawnsDowned(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AllPawnsDowned questPart_Filter_AllPawnsDowned = new QuestPart_Filter_AllPawnsDowned();
			questPart_Filter_AllPawnsDowned.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AllPawnsDowned.signalListenMode = signalListenMode;
			questPart_Filter_AllPawnsDowned.pawns = pawns.ToList();
			questPart_Filter_AllPawnsDowned.inSignalRemovePawn = inSignalRemovePawn;
			questPart_Filter_AllPawnsDowned.outSignal = outSignal;
			questPart_Filter_AllPawnsDowned.outSignalElse = outSignalElse;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AllPawnsDowned.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AllPawnsDowned.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AllPawnsDowned);
			return questPart_Filter_AllPawnsDowned;
		}

		public static QuestPart_Filter_AnyOnTransporter AnyOnTransporter(this Quest quest, IEnumerable<Pawn> pawns, Thing shuttle, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_Filter_AnyOnTransporter questPart_Filter_AnyOnTransporter = new QuestPart_Filter_AnyOnTransporter();
			questPart_Filter_AnyOnTransporter.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Filter_AnyOnTransporter.signalListenMode = signalListenMode;
			questPart_Filter_AnyOnTransporter.pawns = pawns.ToList();
			questPart_Filter_AnyOnTransporter.outSignal = outSignal;
			questPart_Filter_AnyOnTransporter.outSignalElse = outSignalElse;
			questPart_Filter_AnyOnTransporter.transporter = shuttle;
			if (action != null)
			{
				QuestGenUtility.RunInner(action, questPart_Filter_AnyOnTransporter.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			if (elseAction != null)
			{
				QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyOnTransporter.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
			}
			quest.AddPart(questPart_Filter_AnyOnTransporter);
			return questPart_Filter_AnyOnTransporter;
		}
	}
}
