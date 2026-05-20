using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_AnimalPenNotEnclosed : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_AnimalPenNotEnclosed()
	{
		defaultLabel = "AlertAnimalPenNotEnclosed".Translate();
		defaultExplanation = "AlertAnimalPenNotEnclosedDesc".Translate();
	}

	private void CalculateTargets()
	{
		targets.Clear();
		foreach (Map map in Find.Maps)
		{
			if (!map.IsPlayerHome)
			{
				continue;
			}
			foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
			{
				CompAnimalPenMarker compAnimalPenMarker = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
				if (!allBuildingsAnimalPenMarker.IsForbidden(Faction.OfPlayer) && compAnimalPenMarker.PenState.Unenclosed)
				{
					targets.Add(allBuildingsAnimalPenMarker);
				}
			}
		}
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
