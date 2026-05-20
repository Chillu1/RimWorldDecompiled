using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_GauranlenTreeWithoutProductionMode : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_GauranlenTreeWithoutProductionMode()
	{
		defaultLabel = "AlertGauranlenTreeWithoutDryadTypeLabel".Translate();
		defaultExplanation = "AlertGauranlenTreeWithoutDryadTypeDesc".Translate("ChangeMode".Translate());
		requireIdeology = true;
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
			if (!maps[i].IsPlayerHome)
			{
				continue;
			}
			List<Thing> list = maps[i].listerThings.ThingsInGroup(ThingRequestGroup.DryadSpawner);
			for (int j = 0; j < list.Count; j++)
			{
				CompTreeConnection compTreeConnection = list[j].TryGetComp<CompTreeConnection>();
				if (compTreeConnection != null && compTreeConnection.Connected && !compTreeConnection.HasProductionMode)
				{
					targets.Add(list[j]);
				}
			}
		}
	}
}
