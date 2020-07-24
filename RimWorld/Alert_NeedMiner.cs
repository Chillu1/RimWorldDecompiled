using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMiner : Alert
	{
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
				Designation designation = null;
				List<Designation> allDesignations = map.designationManager.allDesignations;
				for (int j = 0; j < allDesignations.Count; j++)
				{
					if (allDesignations[j].def == DesignationDefOf.Mine)
					{
						designation = allDesignations[j];
						break;
					}
				}
				if (designation == null)
				{
					continue;
				}
				bool flag = false;
				foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
				{
					if (!item.Downed && item.workSettings != null && item.workSettings.GetPriority(WorkTypeDefOf.Mining) > 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return AlertReport.CulpritIs(designation.target.Thing);
				}
			}
			return false;
		}
	}
}
