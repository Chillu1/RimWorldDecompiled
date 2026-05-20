using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Filter
{
	public static QuestPart_Filter_AnyPawnAlive AnyPawnAlive(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AnyPawnAlive questPart_Filter_AnyPawnAlive = new QuestPart_Filter_AnyPawnAlive();
		questPart_Filter_AnyPawnAlive.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AnyPawnAlive.signalListenMode = signalListenMode;
		questPart_Filter_AnyPawnAlive.pawns = pawns.ToList();
		questPart_Filter_AnyPawnAlive.inSignalRemovePawn = inSignalRemovePawn;
		questPart_Filter_AnyPawnAlive.outSignal = outSignal;
		questPart_Filter_AnyPawnAlive.outSignalElse = outSignalElse;
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

	public static QuestPart_Filter_AnyPawnInCombatShape AnyPawnInCombatShape(this Quest quest, IEnumerable<Pawn> pawns, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AnyPawnInCombatShape questPart_Filter_AnyPawnInCombatShape = new QuestPart_Filter_AnyPawnInCombatShape();
		questPart_Filter_AnyPawnInCombatShape.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AnyPawnInCombatShape.signalListenMode = signalListenMode;
		questPart_Filter_AnyPawnInCombatShape.pawns = pawns.ToList();
		questPart_Filter_AnyPawnInCombatShape.inSignalRemovePawn = inSignalRemovePawn;
		questPart_Filter_AnyPawnInCombatShape.outSignal = outSignal;
		questPart_Filter_AnyPawnInCombatShape.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_AnyPawnInCombatShape.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyPawnInCombatShape.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_AnyPawnInCombatShape);
		return questPart_Filter_AnyPawnInCombatShape;
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
		questPart_Filter_AnyPawnUnhealthy.outSignal = outSignal;
		questPart_Filter_AnyPawnUnhealthy.outSignalElse = outSignalElse;
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
		questPart_Filter_FactionHostileToOtherFaction.outSignal = outSignal;
		questPart_Filter_FactionHostileToOtherFaction.outSignalElse = outSignalElse;
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
		questPart_Filter_AnyPawnPlayerControlled.outSignal = outSignal;
		questPart_Filter_AnyPawnPlayerControlled.outSignalElse = outSignalElse;
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

	public static QuestPart_Filter_PawnDestroyed PawnDestroyed(this Quest quest, Pawn pawn, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_PawnDestroyed questPart_Filter_PawnDestroyed = new QuestPart_Filter_PawnDestroyed
		{
			inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal")),
			pawn = pawn,
			outSignal = outSignal,
			outSignalElse = outSignalElse,
			signalListenMode = signalListenMode
		};
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_PawnDestroyed.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_PawnDestroyed.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_PawnDestroyed);
		return questPart_Filter_PawnDestroyed;
	}

	public static QuestPart_Filter_FactionNonPlayer FactionNonPlayer(this Quest quest, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_FactionNonPlayer questPart_Filter_FactionNonPlayer = new QuestPart_Filter_FactionNonPlayer();
		questPart_Filter_FactionNonPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_FactionNonPlayer.signalListenMode = signalListenMode;
		questPart_Filter_FactionNonPlayer.outSignal = outSignal;
		questPart_Filter_FactionNonPlayer.outSignalElse = outSignalElse;
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

	public static QuestPart_Filter_AnyOnTransporter AnyOnTransporter(this Quest quest, IEnumerable<Pawn> pawns, Thing shuttle, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
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

	public static QuestPart_Filter_AcceptedAfterTicks AcceptedAfterTicks(this Quest quest, int ticks, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AcceptedAfterTicks questPart_Filter_AcceptedAfterTicks = new QuestPart_Filter_AcceptedAfterTicks();
		questPart_Filter_AcceptedAfterTicks.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AcceptedAfterTicks.signalListenMode = signalListenMode;
		questPart_Filter_AcceptedAfterTicks.timeTicks = ticks;
		questPart_Filter_AcceptedAfterTicks.outSignal = outSignal;
		questPart_Filter_AcceptedAfterTicks.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_AcceptedAfterTicks.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_AcceptedAfterTicks.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_AcceptedAfterTicks);
		return questPart_Filter_AcceptedAfterTicks;
	}

	public static QuestPart_Filter_AnyColonistWithCharityPrecept AnyColonistWithCharityPrecept(this Quest quest, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AnyColonistWithCharityPrecept questPart_Filter_AnyColonistWithCharityPrecept = new QuestPart_Filter_AnyColonistWithCharityPrecept();
		questPart_Filter_AnyColonistWithCharityPrecept.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AnyColonistWithCharityPrecept.signalListenMode = signalListenMode;
		questPart_Filter_AnyColonistWithCharityPrecept.outSignal = outSignal;
		questPart_Filter_AnyColonistWithCharityPrecept.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_AnyColonistWithCharityPrecept.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyColonistWithCharityPrecept.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_AnyColonistWithCharityPrecept);
		return questPart_Filter_AnyColonistWithCharityPrecept;
	}

	public static QuestPart_Filter_BuiltNearSettlement BuiltNearSettlement(this Quest quest, Faction settlementFaction, MapParent mapParent, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_BuiltNearSettlement questPart_Filter_BuiltNearSettlement = new QuestPart_Filter_BuiltNearSettlement();
		questPart_Filter_BuiltNearSettlement.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_BuiltNearSettlement.signalListenMode = signalListenMode;
		questPart_Filter_BuiltNearSettlement.settlementFaction = settlementFaction;
		questPart_Filter_BuiltNearSettlement.mapParent = mapParent;
		questPart_Filter_BuiltNearSettlement.outSignal = outSignal;
		questPart_Filter_BuiltNearSettlement.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_BuiltNearSettlement.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_BuiltNearSettlement.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_BuiltNearSettlement);
		return questPart_Filter_BuiltNearSettlement;
	}

	public static QuestPart_Filter_AnyHostileThreatToPlayer AnyHostileThreatToPlayer(this Quest quest, MapParent mapParent, bool countDormantPawns = false, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AnyHostileThreatToPlayer questPart_Filter_AnyHostileThreatToPlayer = new QuestPart_Filter_AnyHostileThreatToPlayer();
		questPart_Filter_AnyHostileThreatToPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AnyHostileThreatToPlayer.signalListenMode = signalListenMode;
		questPart_Filter_AnyHostileThreatToPlayer.outSignal = outSignal;
		questPart_Filter_AnyHostileThreatToPlayer.outSignalElse = outSignalElse;
		questPart_Filter_AnyHostileThreatToPlayer.countDormantPawnsAsHostile = countDormantPawns;
		questPart_Filter_AnyHostileThreatToPlayer.mapParent = mapParent;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_AnyHostileThreatToPlayer.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyHostileThreatToPlayer.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_AnyHostileThreatToPlayer);
		return questPart_Filter_AnyHostileThreatToPlayer;
	}

	public static QuestPart_Filter_CanAcceptQuest CanAcceptQuest(this Quest quest, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_CanAcceptQuest questPart_Filter_CanAcceptQuest = new QuestPart_Filter_CanAcceptQuest();
		questPart_Filter_CanAcceptQuest.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_CanAcceptQuest.signalListenMode = signalListenMode;
		questPart_Filter_CanAcceptQuest.outSignal = outSignal;
		questPart_Filter_CanAcceptQuest.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_CanAcceptQuest.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_CanAcceptQuest.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_CanAcceptQuest);
		return questPart_Filter_CanAcceptQuest;
	}

	public static QuestPart_Filter_ThingAnalyzed ThingAnalyzed(this Quest quest, ThingDef thingDef, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_ThingAnalyzed questPart_Filter_ThingAnalyzed = new QuestPart_Filter_ThingAnalyzed();
		questPart_Filter_ThingAnalyzed.thingDef = thingDef;
		questPart_Filter_ThingAnalyzed.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_ThingAnalyzed.signalListenMode = signalListenMode;
		questPart_Filter_ThingAnalyzed.outSignal = outSignal;
		questPart_Filter_ThingAnalyzed.outSignalElse = outSignalElse;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_ThingAnalyzed.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_ThingAnalyzed.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_ThingAnalyzed);
		return questPart_Filter_ThingAnalyzed;
	}

	public static QuestPart_Filter_AnyPawnHasHediff AnyPawnHasHediff(this Quest quest, IEnumerable<Pawn> pawns, HediffDef hediff, Action action = null, Action elseAction = null, string inSignal = null, string outSignal = null, string outSignalElse = null, string inSignalRemovePawn = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Filter_AnyPawnHasHediff questPart_Filter_AnyPawnHasHediff = new QuestPart_Filter_AnyPawnHasHediff();
		questPart_Filter_AnyPawnHasHediff.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Filter_AnyPawnHasHediff.signalListenMode = signalListenMode;
		questPart_Filter_AnyPawnHasHediff.pawns = pawns.ToList();
		questPart_Filter_AnyPawnHasHediff.hediff = hediff;
		questPart_Filter_AnyPawnHasHediff.outSignal = outSignal;
		questPart_Filter_AnyPawnHasHediff.outSignalElse = outSignalElse;
		questPart_Filter_AnyPawnHasHediff.inSignalRemovePawn = inSignalRemovePawn;
		if (action != null)
		{
			QuestGenUtility.RunInner(action, questPart_Filter_AnyPawnHasHediff.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		if (elseAction != null)
		{
			QuestGenUtility.RunInner(elseAction, questPart_Filter_AnyPawnHasHediff.outSignalElse = QuestGen.GenerateNewSignal("OuterNodeCompleted"));
		}
		quest.AddPart(questPart_Filter_AnyPawnHasHediff);
		return questPart_Filter_AnyPawnHasHediff;
	}
}
