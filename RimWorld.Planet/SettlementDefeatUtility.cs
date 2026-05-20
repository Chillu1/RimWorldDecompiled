using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet;

public static class SettlementDefeatUtility
{
	public static void CheckDefeated(Settlement factionBase)
	{
		if (factionBase.Faction == Faction.OfPlayer)
		{
			return;
		}
		Map map = factionBase.Map;
		if (map == null || !IsDefeated(map, factionBase.Faction))
		{
			return;
		}
		IdeoUtility.Notify_PlayerRaidedSomeone(map.mapPawns.FreeColonistsSpawned);
		DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(factionBase.Tile.LayerDef.DestroyedSettlementWorldObjectDef);
		destroyedSettlement.Tile = factionBase.Tile;
		destroyedSettlement.SetFaction(factionBase.Faction);
		Find.WorldObjects.Add(destroyedSettlement);
		StringBuilder stringBuilder = new StringBuilder();
		bool num = HasAnyOtherBase(factionBase);
		if (num && destroyedSettlement.TryGetComponent<TimedDetectionRaids>(out var comp))
		{
			comp.CopyFrom(factionBase.GetComponent<TimedDetectionRaids>());
			comp.SetNotifiedSilently();
			if (!string.IsNullOrEmpty(comp.DetectionCountdownTimeLeftString))
			{
				stringBuilder.Append("LetterFactionBaseDefeated".Translate(factionBase.Label, comp.DetectionCountdownTimeLeftString));
			}
			else
			{
				stringBuilder.Append("LetterFactionBaseDefeatedNoRaids".Translate(factionBase.Label));
			}
		}
		else
		{
			stringBuilder.Append("LetterFactionBaseDefeatedNoRaids".Translate(factionBase.Label));
		}
		if (!num)
		{
			factionBase.Faction.defeated = true;
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("LetterFactionBaseDefeated_FactionDestroyed".Translate(factionBase.Faction.Name));
		}
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (!allFaction.Hidden && !allFaction.IsPlayer && allFaction != factionBase.Faction && allFaction.HostileTo(factionBase.Faction))
			{
				FactionRelationKind playerRelationKind = allFaction.PlayerRelationKind;
				Faction.OfPlayer.TryAffectGoodwillWith(allFaction, 20, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.DestroyedEnemyBase);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("RelationsWith".Translate(allFaction.Name) + ": " + 20.ToStringWithSign());
				allFaction.TryAppendRelationKindChangedInfo(stringBuilder, playerRelationKind, allFaction.PlayerRelationKind);
			}
		}
		Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), stringBuilder.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(factionBase.Tile), factionBase.Faction);
		map.info.parent = destroyedSettlement;
		factionBase.Destroy();
		TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, map.mapPawns.FreeColonists.RandomElement());
	}

	public static bool IsDefeated(Map map, Faction faction)
	{
		List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
		for (int i = 0; i < list.Count; i++)
		{
			Pawn pawn = list[i];
			if (pawn.RaceProps.Humanlike && GenHostility.IsActiveThreatToPlayer(pawn, map.Parent is Settlement && map.Parent.Faction == faction))
			{
				return false;
			}
		}
		return true;
	}

	private static bool HasAnyOtherBase(Settlement defeatedFactionBase)
	{
		List<Settlement> settlements = Find.WorldObjects.Settlements;
		for (int i = 0; i < settlements.Count; i++)
		{
			Settlement settlement = settlements[i];
			if (settlement.Faction == defeatedFactionBase.Faction && settlement != defeatedFactionBase)
			{
				return true;
			}
		}
		return false;
	}
}
