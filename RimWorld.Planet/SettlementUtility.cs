using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class SettlementUtility
{
	public static bool IsPlayerAttackingAnySettlementOf(Faction faction)
	{
		if (faction == Faction.OfPlayer)
		{
			return false;
		}
		if (!faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].info.parent is Settlement settlement && settlement.Faction == faction)
			{
				return true;
			}
		}
		return false;
	}

	public static void Attack(Caravan caravan, Settlement settlement)
	{
		if (!settlement.HasMap)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				AttackNow(caravan, settlement);
			}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		}
		else
		{
			AttackNow(caravan, settlement);
		}
	}

	private static void AttackNow(Caravan caravan, Settlement settlement)
	{
		bool num = !settlement.HasMap;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
		TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
		TaggedString letterText = "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, settlement.Label.ApplyTag(TagType.Settlement, settlement.Faction.GetUniqueLoadID())).CapitalizeFirst();
		AffectRelationsOnAttacked(settlement, ref letterText);
		if (num)
		{
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
		}
		Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction);
		CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
		Find.GoodwillSituationManager.RecalculateAll(canSendHostilityChangedLetter: true);
	}

	public static void AffectRelationsOnAttacked(MapParent mapParent, ref TaggedString letterText)
	{
		if (mapParent.Faction != null && mapParent.Faction != Faction.OfPlayer)
		{
			FactionRelationKind playerRelationKind = mapParent.Faction.PlayerRelationKind;
			Faction.OfPlayer.TryAffectGoodwillWith(mapParent.Faction, Faction.OfPlayer.GoodwillToMakeHostile(mapParent.Faction), canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.AttackedSettlement);
			mapParent.Faction.TryAppendRelationKindChangedInfo(ref letterText, playerRelationKind, mapParent.Faction.PlayerRelationKind);
		}
	}
}
