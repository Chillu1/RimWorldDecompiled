using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class SettlementDefeatUtility
	{
		public static void CheckDefeated(Settlement factionBase)
		{
			if (factionBase.Faction == Faction.OfPlayer)
			{
				return;
			}
			Map map = factionBase.Map;
			if (map != null && IsDefeated(map, factionBase.Faction))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LetterFactionBaseDefeated".Translate(factionBase.Label, TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000)));
				if (!HasAnyOtherBase(factionBase))
				{
					factionBase.Faction.defeated = true;
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("LetterFactionBaseDefeated_FactionDestroyed".Translate(factionBase.Faction.Name));
				}
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					if (!allFaction.def.hidden && !allFaction.IsPlayer && allFaction != factionBase.Faction && allFaction.HostileTo(factionBase.Faction))
					{
						FactionRelationKind playerRelationKind = allFaction.PlayerRelationKind;
						if (allFaction.TryAffectGoodwillWith(Faction.OfPlayer, 20, canSendMessage: false, canSendHostilityLetter: false))
						{
							stringBuilder.AppendLine();
							stringBuilder.AppendLine();
							stringBuilder.Append("RelationsWith".Translate(allFaction.Name) + ": " + 20.ToStringWithSign());
							allFaction.TryAppendRelationKindChangedInfo(stringBuilder, playerRelationKind, allFaction.PlayerRelationKind);
						}
					}
				}
				Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), stringBuilder.ToString(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(factionBase.Tile), factionBase.Faction);
				DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
				destroyedSettlement.Tile = factionBase.Tile;
				Find.WorldObjects.Add(destroyedSettlement);
				map.info.parent = destroyedSettlement;
				factionBase.Destroy();
				destroyedSettlement.GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown();
				TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, map.mapPawns.FreeColonists.RandomElement());
			}
		}

		private static bool IsDefeated(Map map, Faction faction)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.RaceProps.Humanlike && GenHostility.IsActiveThreatToPlayer(pawn))
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
}
