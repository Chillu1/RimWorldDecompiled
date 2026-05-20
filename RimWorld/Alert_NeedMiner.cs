using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_NeedMiner : Alert
{
	private static List<Designation> tmpDesignations = new List<Designation>();

	public Alert_NeedMiner()
	{
		defaultLabel = "NeedMiner".Translate();
		defaultExplanation = "NeedMinerDesc".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override AlertReport GetReport()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (!map.IsPlayerHome)
			{
				continue;
			}
			tmpDesignations.Clear();
			tmpDesignations.AddRange(map.designationManager.designationsByDef[DesignationDefOf.Mine]);
			tmpDesignations.AddRange(map.designationManager.designationsByDef[DesignationDefOf.MineVein]);
			if (tmpDesignations.NullOrEmpty())
			{
				continue;
			}
			bool flag = false;
			foreach (Pawn item in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
			{
				if ((item.Spawned || item.BrieflyDespawned()) && !item.Downed)
				{
					if (item.IsFreeColonist && item.workSettings != null && item.workSettings.GetPriority(WorkTypeDefOf.Mining) > 0)
					{
						flag = true;
						break;
					}
					if (item.IsColonyMechPlayerControlled && item.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDefOf.Mining) && item.RaceProps.mechFixedSkillLevel > 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return AlertReport.CulpritIs(new GlobalTargetInfo(tmpDesignations[0].target.Cell, map));
			}
		}
		return false;
	}
}
