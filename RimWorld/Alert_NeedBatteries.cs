using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedBatteries : Alert
{
	public Alert_NeedBatteries()
	{
		defaultLabel = "NeedBatteries".Translate();
		defaultExplanation = "NeedBatteriesDesc".Translate();
	}

	public override TaggedString GetExplanation()
	{
		string text = defaultExplanation + "\n\n";
		text = ((!ResearchProjectDefOf.Batteries.IsFinished) ? ((string)(text + "NeedBatteriesNotResearchedDesc".Translate())) : ((string)(text + "NeedBatteriesResearchedDesc".Translate())));
		return text;
	}

	public override AlertReport GetReport()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (NeedBatteries(maps[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool NeedBatteries(Map map)
	{
		if (!map.IsPlayerHome)
		{
			return false;
		}
		if (map.listerBuildings.ColonistsHaveBuilding((Thing building) => building is Building_Battery))
		{
			return false;
		}
		if (!map.listerBuildings.ColonistsHaveBuilding(ThingDefOf.SolarGenerator) && !map.listerBuildings.ColonistsHaveBuilding(ThingDefOf.WindTurbine))
		{
			return false;
		}
		if (map.listerBuildings.ColonistsHaveBuilding(ThingDefOf.GeothermalGenerator) || map.listerBuildings.ColonistsHaveBuilding(ThingDefOf.WatermillGenerator))
		{
			return false;
		}
		return true;
	}
}
