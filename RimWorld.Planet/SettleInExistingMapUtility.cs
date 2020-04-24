using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld.Planet
{
	public static class SettleInExistingMapUtility
	{
		private static List<Pawn> tmpPlayerPawns = new List<Pawn>();

		public static Command SettleCommand(Map map, bool requiresNoEnemies)
		{
			Command_Settle command_Settle = new Command_Settle();
			command_Settle.defaultLabel = "CommandSettle".Translate();
			command_Settle.defaultDesc = "CommandSettleDesc".Translate();
			command_Settle.icon = SettleUtility.SettleCommandTex;
			command_Settle.action = delegate
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				SettlementProximityGoodwillUtility.CheckConfirmSettle(map.Tile, delegate
				{
					Settle(map);
				});
			};
			if (SettleUtility.PlayerSettlementsCountLimitReached)
			{
				if (Prefs.MaxNumberOfPlayerSettlements > 1)
				{
					command_Settle.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
				}
				else
				{
					command_Settle.Disable("CommandSettleFailAlreadyHaveBase".Translate());
				}
			}
			if (!command_Settle.disabled)
			{
				if (map.mapPawns.FreeColonistsCount == 0)
				{
					command_Settle.Disable("CommandSettleFailNoColonists".Translate());
				}
				else if (requiresNoEnemies)
				{
					foreach (IAttackTarget item in map.attackTargetsCache.TargetsHostileToColony)
					{
						if (GenHostility.IsActiveThreatToPlayer(item))
						{
							command_Settle.Disable("CommandSettleFailEnemies".Translate());
							return command_Settle;
						}
					}
					return command_Settle;
				}
			}
			return command_Settle;
		}

		public static void Settle(Map map)
		{
			MapParent parent = map.Parent;
			Settlement settlement = SettleUtility.AddNewHome(map.Tile, Faction.OfPlayer);
			map.info.parent = settlement;
			parent?.Destroy();
			Messages.Message("MessageSettledInExistingMap".Translate(), settlement, MessageTypeDefOf.PositiveEvent, historical: false);
			tmpPlayerPawns.Clear();
			tmpPlayerPawns.AddRange(map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer));
			CaravanEnterMapUtility.DropAllInventory(tmpPlayerPawns);
			tmpPlayerPawns.Clear();
			List<Pawn> prisonersOfColonySpawned = map.mapPawns.PrisonersOfColonySpawned;
			for (int i = 0; i < prisonersOfColonySpawned.Count; i++)
			{
				prisonersOfColonySpawned[i].guest.WaitInsteadOfEscapingForDefaultTicks();
			}
		}
	}
}
