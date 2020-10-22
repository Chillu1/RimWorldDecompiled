using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
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
				Settlement settlement = maps[i].info.parent as Settlement;
				if (settlement != null && settlement.Faction == faction)
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
			AffectRelationsOnAttacked_NewTmp(settlement, ref letterText);
			if (num)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction);
			CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
		}

		[Obsolete("Only used for mod compatibility. Will be removed in a future version.")]
		public static void AffectRelationsOnAttacked(Settlement settlement, ref TaggedString letterText)
		{
			AffectRelationsOnAttacked_NewTmp(settlement, ref letterText);
		}

		public static void AffectRelationsOnAttacked_NewTmp(MapParent mapParent, ref TaggedString letterText)
		{
			if (mapParent.Faction == null || mapParent.Faction == Faction.OfPlayer)
			{
				return;
			}
			FactionRelationKind playerRelationKind = mapParent.Faction.PlayerRelationKind;
			if (!mapParent.Faction.HostileTo(Faction.OfPlayer))
			{
				mapParent.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: false);
			}
			else if (mapParent.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -50, canSendMessage: false, canSendHostilityLetter: false))
			{
				if (!letterText.NullOrEmpty())
				{
					letterText += "\n\n";
				}
				letterText += "RelationsWith".Translate(mapParent.Faction.Name.ApplyTag(mapParent.Faction)) + ": " + (-50).ToStringWithSign();
			}
			mapParent.Faction.TryAppendRelationKindChangedInfo(ref letterText, playerRelationKind, mapParent.Faction.PlayerRelationKind);
		}
	}
}
