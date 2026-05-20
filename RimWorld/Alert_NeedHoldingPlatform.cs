using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedHoldingPlatform : Alert
{
	public Alert_NeedHoldingPlatform()
	{
		defaultLabel = "AlertHoldingPlatform".Translate();
		defaultExplanation = "AlertHoldingPlatformDesc".Translate();
		requireAnomaly = true;
	}

	public override AlertReport GetReport()
	{
		if ((float)Find.TickManager.TicksGame < 240000f)
		{
			return false;
		}
		if (Find.Anomaly.hasBuiltHoldingPlatform)
		{
			return false;
		}
		if (Find.CurrentMap == null)
		{
			return false;
		}
		if (!Find.Anomaly.AnomalyStudyEnabled)
		{
			return false;
		}
		if (Find.Anomaly.Level == 1 && Find.Anomaly.TicksSinceLastLevelChange < 7200)
		{
			return false;
		}
		List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Blueprint blueprint && blueprint.def.entityDefToBuild is ThingDef def && ThingRequestGroup.EntityHolder.Includes(def))
			{
				return false;
			}
		}
		List<Thing> list2 = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j] is Frame frame && frame.def.entityDefToBuild is ThingDef def2 && ThingRequestGroup.EntityHolder.Includes(def2))
			{
				return false;
			}
		}
		return true;
	}
}
