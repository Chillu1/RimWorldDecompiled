using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_GenebankUnpowered : Alert
{
	public List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_GenebankUnpowered()
	{
		defaultLabel = "AlertGenebankUnpowered".Translate();
		defaultExplanation = "AlertGenebankUnpoweredDesc".Translate();
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
			foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GeneBank))
			{
				CompGenepackContainer compGenepackContainer = item.TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer != null && compGenepackContainer.innerContainer.Any() && !compGenepackContainer.PowerOn)
				{
					targets.Add(item);
				}
			}
		}
	}
}
