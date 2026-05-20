using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_ToxifierGeneratorStopped : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_ToxifierGeneratorStopped()
	{
		requireBiotech = true;
	}

	public override string GetLabel()
	{
		return defaultLabel ?? (defaultLabel = "AlertToxifierGeneratorStopped".Translate(ThingDefOf.ToxifierGenerator.LabelCap));
	}

	public override TaggedString GetExplanation()
	{
		return defaultExplanation ?? (defaultExplanation = "AlertToxifierGeneratorStoppedDesc".Translate(ThingDefOf.ToxifierGenerator.label));
	}

	private void GetTargets()
	{
		targets.Clear();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.ToxifierGenerator))
			{
				if (!item.TryGetComp<CompToxifier>().CanPolluteNow)
				{
					targets.Add(item);
				}
			}
		}
	}

	public override AlertReport GetReport()
	{
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
