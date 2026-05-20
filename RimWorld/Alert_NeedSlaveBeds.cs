using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Alert_NeedSlaveBeds : Alert
{
	public Alert_NeedSlaveBeds()
	{
		defaultLabel = "NeedSlaveBeds".Translate();
		defaultExplanation = "NeedSlaveBedsDesc".Translate();
		defaultPriority = AlertPriority.High;
		requireIdeology = true;
	}

	public override AlertReport GetReport()
	{
		if (GenDate.DaysPassed > 30)
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.IsPlayerHome)
			{
				CheckSlaveBeds(map, out var enoughBeds, out var _);
				if (!enoughBeds)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void CheckSlaveBeds(Map map, out bool enoughBeds, out bool enoughBabyCribs)
	{
		int num = 0;
		int num2 = 0;
		List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			if (allBuildingsColonist[i] is Building_Bed { ForSlaves: not false, Medical: false } building_Bed && building_Bed.def.building.bed_humanlike)
			{
				if (building_Bed.ForHumanBabies)
				{
					num2 += building_Bed.TotalSleepingSlots;
				}
				else
				{
					num += building_Bed.TotalSleepingSlots;
				}
			}
		}
		int num3 = 0;
		int num4 = 0;
		foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
		{
			if ((item.Spawned || item.BrieflyDespawned()) && item.IsSlave)
			{
				if (item.DevelopmentalStage.Baby())
				{
					num4++;
				}
				else
				{
					num3++;
				}
			}
		}
		enoughBeds = num >= num3;
		int num5 = Mathf.Max(0, num - num3);
		enoughBabyCribs = num5 + num2 >= num4;
	}
}
