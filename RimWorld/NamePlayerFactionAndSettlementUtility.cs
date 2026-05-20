using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class NamePlayerFactionAndSettlementUtility
{
	private const float MinDaysPassedToNameFaction = 4.3f;

	private const float MinDaysPassedToNameSettlement = 4.3f;

	private const int SoonTicks = 30000;

	public static bool CanNameFactionNow()
	{
		return CanNameFaction(Find.TickManager.TicksGame);
	}

	public static bool CanNameSettlementNow(Settlement factionBase)
	{
		return CanNameSettlement(factionBase, Find.TickManager.TicksGame - factionBase.creationGameTicks);
	}

	public static bool CanNameFactionSoon()
	{
		return CanNameFaction(Find.TickManager.TicksGame + 30000);
	}

	public static bool CanNameSettlementSoon(Settlement factionBase)
	{
		return CanNameSettlement(factionBase, Find.TickManager.TicksGame - factionBase.creationGameTicks + 30000);
	}

	private static bool CanNameFaction(int ticksPassed)
	{
		if (!Faction.OfPlayer.HasName && (float)ticksPassed / 60000f >= 4.3f)
		{
			return CanNameAnythingNow();
		}
		return false;
	}

	private static bool CanNameSettlement(Settlement factionBase, int ticksPassed)
	{
		if (factionBase.Faction == Faction.OfPlayer && !factionBase.namedByPlayer && (float)ticksPassed / 60000f >= 4.3f && factionBase.HasMap && factionBase.Map.dangerWatcher.DangerRating != StoryDanger.High && factionBase.Map.mapPawns.FreeColonistsSpawnedCount != 0)
		{
			return CanNameAnythingNow();
		}
		return false;
	}

	private static bool CanNameAnythingNow()
	{
		if (Find.AnyPlayerHomeMap == null || Find.CurrentMap == null || !Find.CurrentMap.IsPlayerHome || Find.GameEnder.gameEnding)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome)
			{
				if (maps[i].mapPawns.FreeColonistsSpawnedCount >= 2)
				{
					flag = true;
				}
				if (!maps[i].attackTargetsCache.TargetsHostileToColony.Any((IAttackTarget x) => GenHostility.IsActiveThreatToPlayer(x)))
				{
					flag2 = true;
				}
			}
		}
		if (!flag || !flag2)
		{
			return false;
		}
		return true;
	}
}
