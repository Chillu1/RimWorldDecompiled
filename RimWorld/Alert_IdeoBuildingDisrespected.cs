using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_IdeoBuildingDisrespected : Alert_Precept
{
	public IdeoBuildingPresenceDemand demand;

	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> targetNames = new List<string>();

	private List<GlobalTargetInfo> Targets
	{
		get
		{
			targets.Clear();
			targetNames.Clear();
			Map currentMap = Find.CurrentMap;
			if (currentMap == null)
			{
				return null;
			}
			foreach (Pawn item in currentMap.mapPawns.FreeColonistsSpawned)
			{
				if (item.Ideo == demand.parent.ideo)
				{
					targetNames.Add(item.LabelShort);
				}
			}
			Thing thing = demand.BestBuilding(currentMap);
			if (thing != null)
			{
				targets.Add(thing);
			}
			return targets;
		}
	}

	public Alert_IdeoBuildingDisrespected()
	{
	}

	public Alert_IdeoBuildingDisrespected(IdeoBuildingPresenceDemand demand)
	{
		this.demand = demand;
		label = "IdeoBuildingDisrespected".Translate(demand.parent.LabelCap);
		sourcePrecept = demand.parent;
	}

	public override AlertReport GetReport()
	{
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		Map currentMap = Find.CurrentMap;
		if (currentMap == null || !demand.BuildingPresent(currentMap) || !demand.AppliesTo(currentMap) || demand.RequirementsSatisfied(currentMap))
		{
			return false;
		}
		if (!Faction.OfPlayer.ideos.Has(demand.parent.ideo))
		{
			return false;
		}
		return new AlertReport
		{
			active = true,
			culpritsTargets = Targets
		};
	}

	public override TaggedString GetExplanation()
	{
		return "IdeoBuildingDisrespectedDesc".Translate(demand.parent.ideo.name, demand.parent.Label.Colorize(ColoredText.NameColor), demand.RoomRequirementsInfo.ToLineList(" -  "), targetNames.ToLineList(" -  "));
	}
}
