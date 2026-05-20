using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_MechChargerFull : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_MechChargerFull()
	{
		defaultLabel = "AlertRechargerFull".Translate();
		defaultExplanation = "AlertRechargerFullDesc".Translate();
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}

	private void GetTargets()
	{
		targets.Clear();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Thing> list = maps[i].listerThings.ThingsInGroup(ThingRequestGroup.MechCharger);
			for (int j = 0; j < list.Count; j++)
			{
				Building_MechCharger building_MechCharger = (Building_MechCharger)list[j];
				if (building_MechCharger.Faction == Faction.OfPlayer && building_MechCharger.IsFullOfWaste)
				{
					targets.Add(building_MechCharger);
				}
			}
		}
	}
}
