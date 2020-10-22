using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
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

		public static QuestPart_ExtraFaction ExtraFaction(this Quest quest, Faction faction, IEnumerable<Pawn> pawns, ExtraFactionType factionType, bool areHelpers = false, string inSignalRemovePawn = null)
		{
			QuestPart_ExtraFaction questPart_ExtraFaction = new QuestPart_ExtraFaction
			{
				affectedPawns = pawns.ToList(),
				extraFaction = new ExtraFaction(faction, factionType),
				areHelpers = areHelpers,
				inSignalRemovePawn = inSignalRemovePawn
			};
			quest.AddPart(questPart_ExtraFaction);
			return questPart_ExtraFaction;
		}

		public static QuestPart_SetFactionRelations SetFactionRelations(this Quest quest, Faction faction, FactionRelationKind relationKind, string inSignal = null, bool? canSendLetter = null)
		{
			QuestPart_SetFactionRelations questPart_SetFactionRelations = new QuestPart_SetFactionRelations();
			questPart_SetFactionRelations.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_SetFactionRelations.faction = faction;
			questPart_SetFactionRelations.relationKind = relationKind;
			questPart_SetFactionRelations.canSendLetter = canSendLetter ?? true;
			quest.AddPart(questPart_SetFactionRelations);
			return questPart_SetFactionRelations;
		}

		public static QuestPart_ReserveFaction ReserveFaction(this Quest quest, Faction faction)
		{
			QuestPart_ReserveFaction questPart_ReserveFaction = new QuestPart_ReserveFaction();
			questPart_ReserveFaction.faction = faction;
			quest.AddPart(questPart_ReserveFaction);
			return questPart_ReserveFaction;
		}

		public static QuestPart_FactionGoodwillChange FactionGoodwillChange(this Quest quest, Faction faction, int change = 0, string inSignal = null, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, bool getLookTargetFromSignal = true, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
			questPart_FactionGoodwillChange.faction = faction;
			questPart_FactionGoodwillChange.change = change;
			questPart_FactionGoodwillChange.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_FactionGoodwillChange.canSendMessage = canSendMessage;
			questPart_FactionGoodwillChange.canSendHostilityLetter = canSendHostilityLetter;
			questPart_FactionGoodwillChange.reason = reason;
			questPart_FactionGoodwillChange.getLookTargetFromSignal = getLookTargetFromSignal;
			questPart_FactionGoodwillChange.signalListenMode = signalListenMode;
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
}
