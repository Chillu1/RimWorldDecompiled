using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_InhibitorBlocked : Alert
{
	public readonly List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_InhibitorBlocked()
	{
		defaultLabel = "AlertInhibitorBlocked".Translate();
		defaultExplanation = "AlertInhibitorBlockedDesc".Translate();
		requireAnomaly = true;
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
			foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.ElectricInhibitor))
			{
				if (item.HasComp<CompFacility>() && ContainmentUtility.IsLinearBuildingBlocked(item.def, item.Position, item.Rotation, item.Map))
				{
					targets.Add(item);
				}
			}
		}
	}
}
