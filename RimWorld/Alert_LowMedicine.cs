using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_LowMedicine : Alert
{
	private Map mapWithNoMedicine;

	private int medicineCount;

	private const float MedicinePerColonistThreshold = 2f;

	public Alert_LowMedicine()
	{
		defaultLabel = "LowMedicine".Translate();
		defaultExplanation = "NoMedicineDesc".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override TaggedString GetExplanation()
	{
		if (mapWithNoMedicine == null)
		{
			return string.Empty;
		}
		if (medicineCount == 0)
		{
			return defaultExplanation;
		}
		return "LowMedicineDesc".Translate(medicineCount);
	}

	public override AlertReport GetReport()
	{
		if ((float)Find.TickManager.TicksGame < 150000f)
		{
			return false;
		}
		return MapWithLowMedicine() != null;
	}

	private Map MapWithLowMedicine()
	{
		mapWithNoMedicine = null;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (map.IsPlayerHome && map.mapPawns.AnyColonistSpawned && (float)MedicineCount(map) < 2f * (float)map.mapPawns.FreeColonistsSpawnedCount)
			{
				mapWithNoMedicine = map;
				break;
			}
		}
		return mapWithNoMedicine;
	}

	private int MedicineCount(Map map)
	{
		medicineCount = map.resourceCounter.GetCountIn(ThingRequestGroup.Medicine);
		return medicineCount;
	}
}
