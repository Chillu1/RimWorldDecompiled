using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Factions
{
	public static QuestPart_AssaultColony AssaultColony(this Quest quest, Faction faction, MapParent mapParent, IEnumerable<Pawn> pawns, string inSignal = null, string inSignalRemovePawn = null)
	{
		QuestPart_AssaultColony questPart_AssaultColony = new QuestPart_AssaultColony();
		questPart_AssaultColony.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_AssaultColony.faction = faction;
		questPart_AssaultColony.mapParent = mapParent;
		questPart_AssaultColony.pawns.AddRange(pawns);
		questPart_AssaultColony.inSignalRemovePawn = inSignalRemovePawn;
		quest.AddPart(questPart_AssaultColony);
		return questPart_AssaultColony;
	}

	public static QuestPart_ExtraFaction ExtraFaction(this Quest quest, Faction faction, IEnumerable<Pawn> pawns, ExtraFactionType factionType, bool areHelpers = false, List<string> inSignalsRemovePawn = null)
	{
		QuestPart_ExtraFaction questPart_ExtraFaction = new QuestPart_ExtraFaction
		{
			affectedPawns = pawns.ToList(),
			extraFaction = new ExtraFaction(faction, factionType),
			areHelpers = areHelpers,
			inSignalsRemovePawn = inSignalsRemovePawn
		};
		quest.AddPart(questPart_ExtraFaction);
		return questPart_ExtraFaction;
	}

	public static QuestPart_ReserveFaction ReserveFaction(this Quest quest, Faction faction)
	{
		QuestPart_ReserveFaction questPart_ReserveFaction = new QuestPart_ReserveFaction();
		questPart_ReserveFaction.faction = faction;
		quest.AddPart(questPart_ReserveFaction);
		return questPart_ReserveFaction;
	}

	public static QuestPart_FactionRelationChange FactionRelationToPlayerChange(this Quest quest, Faction faction, FactionRelationKind relationKind, bool canSendHostilityLetter = true, string inSignal = null)
	{
		QuestPart_FactionRelationChange questPart_FactionRelationChange = new QuestPart_FactionRelationChange();
		questPart_FactionRelationChange.faction = faction;
		questPart_FactionRelationChange.relationKind = relationKind;
		questPart_FactionRelationChange.canSendHostilityLetter = canSendHostilityLetter;
		questPart_FactionRelationChange.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		quest.AddPart(questPart_FactionRelationChange);
		return questPart_FactionRelationChange;
	}

	public static QuestPart_FactionGoodwillChange FactionGoodwillChange(this Quest quest, Faction faction, int change, string inSignal = null, bool canSendMessage = true, bool canSendHostilityLetter = true, bool getLookTargetFromSignal = true, HistoryEventDef historyEvent = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly, bool ensureMakesHostile = false)
	{
		QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
		questPart_FactionGoodwillChange.faction = faction;
		questPart_FactionGoodwillChange.change = change;
		questPart_FactionGoodwillChange.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		questPart_FactionGoodwillChange.canSendMessage = canSendMessage;
		questPart_FactionGoodwillChange.canSendHostilityLetter = canSendHostilityLetter;
		questPart_FactionGoodwillChange.getLookTargetFromSignal = getLookTargetFromSignal;
		questPart_FactionGoodwillChange.signalListenMode = signalListenMode;
		questPart_FactionGoodwillChange.historyEvent = historyEvent;
		questPart_FactionGoodwillChange.ensureMakesHostile = ensureMakesHostile;
		quest.AddPart(questPart_FactionGoodwillChange);
		return questPart_FactionGoodwillChange;
	}

	public static QuestPart_SetFactionHidden SetFactionHidden(this Quest quest, Faction faction, bool hidden = false, string inSignal = null)
	{
		QuestPart_SetFactionHidden questPart_SetFactionHidden = new QuestPart_SetFactionHidden();
		questPart_SetFactionHidden.faction = faction;
		questPart_SetFactionHidden.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
		questPart_SetFactionHidden.hidden = hidden;
		quest.AddPart(questPart_SetFactionHidden);
		return questPart_SetFactionHidden;
	}
}
