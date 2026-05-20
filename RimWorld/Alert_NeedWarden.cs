using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedWarden : Alert
{
	public Alert_NeedWarden()
	{
		defaultLabel = "NeedWarden".Translate();
		defaultExplanation = "NeedWardenDesc".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override AlertReport GetReport()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (!map.IsPlayerHome || !map.mapPawns.PrisonersOfColonySpawned.Any())
			{
				continue;
			}
			bool flag = false;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if ((item.Spawned || item.BrieflyDespawned()) && !item.Downed && item.workSettings != null && item.workSettings.GetPriority(WorkTypeDefOf.Warden) > 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return AlertReport.CulpritIs(map.mapPawns.PrisonersOfColonySpawned[0]);
			}
		}
		return false;
	}
}
