using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_PredatorInPen : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public override string GetLabel()
	{
		return "AlertPredatorInAnimalPen".Translate(targets.First().Thing.Named("ANIMAL")).CapitalizeFirst();
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
				if (compAnimalPenMarker.PenState.Unenclosed)
				{
					continue;
				}
				foreach (Region connectedRegion in compAnimalPenMarker.PenState.ConnectedRegions)
				{
					foreach (Pawn item in connectedRegion.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
					{
						if (item.IsAnimal && item.RaceProps.predator && item.Faction == null && item.GetRegion() == connectedRegion)
						{
							targets.Add(item);
						}
					}
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertPredatorInAnimalPenDesc".Translate(targets.First().Thing.Named("ANIMAL")).CapitalizeFirst();
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
