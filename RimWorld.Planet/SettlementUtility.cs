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
			AffectRelationsOnAttacked(settlement, ref letterText);
			if (num)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction);
			CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
		}

		public static void AffectRelationsOnAttacked(Settlement settlement, ref TaggedString letterText)
		{
			if (settlement.Faction == null || settlement.Faction == Faction.OfPlayer)
			{
				return;
			}
			FactionRelationKind playerRelationKind = settlement.Faction.PlayerRelationKind;
			if (!settlement.Faction.HostileTo(Faction.OfPlayer))
			{
				settlement.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: false);
			}
			else if (settlement.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -50, canSendMessage: false, canSendHostilityLetter: false))
			{
				if (!letterText.NullOrEmpty())
				{
					letterText += "\n\n";
				}
				letterText += "RelationsWith".Translate(settlement.Faction.Name.ApplyTag(settlement.Faction)) + ": " + (-50).ToStringWithSign();
			}
			settlement.Faction.TryAppendRelationKindChangedInfo(ref letterText, playerRelationKind, settlement.Faction.PlayerRelationKind);
		}
	}
}
