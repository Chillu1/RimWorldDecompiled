using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_NeedMechChargers : Alert
{
	private List<Pawn> mechs = new List<Pawn>();

	private List<Building_MechCharger> chargers = new List<Building_MechCharger>();

	private List<ThingDef> allChargerDefs = new List<ThingDef>();

	private List<ThingDef> requiredChargerDefs = new List<ThingDef>();

	public Alert_NeedMechChargers()
	{
		defaultLabel = "AlertNeedMechChargers".Translate();
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		RecacheMechsAndChargers();
		return requiredChargerDefs.Count > 0;
	}

	private void RecacheMechsAndChargers()
	{
		chargers.Clear();
		mechs.Clear();
		List<Map> maps = Find.Maps;
		if (allChargerDefs == null)
		{
			allChargerDefs = new List<ThingDef>();
			allChargerDefs.AddRange(DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => ThingRequestGroup.MechCharger.Includes(t)));
		}
		for (int num = 0; num < maps.Count; num++)
		{
			List<Thing> list = maps[num].listerThings.ThingsInGroup(ThingRequestGroup.MechCharger);
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				Building_MechCharger building_MechCharger = (Building_MechCharger)list[num2];
				if (building_MechCharger.Faction == Faction.OfPlayer)
				{
					chargers.Add(building_MechCharger);
				}
			}
		}
		for (int num3 = 0; num3 < maps.Count; num3++)
		{
			List<Pawn> list2 = maps[num3].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				if (list2[num4].IsColonyMech)
				{
					mechs.Add(list2[num4]);
				}
			}
		}
		requiredChargerDefs.Clear();
		for (int num5 = 0; num5 < mechs.Count; num5++)
		{
			Pawn mech = mechs[num5];
			if (!chargers.Any((Building_MechCharger c) => c.IsCompatibleWithCharger(mech.kindDef)))
			{
				ThingDef thingDef = allChargerDefs.FirstOrDefault((ThingDef c) => Building_MechCharger.IsCompatibleWithCharger(c, mech.kindDef));
				if (thingDef != null && !requiredChargerDefs.Contains(thingDef))
				{
					requiredChargerDefs.Add(thingDef);
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		TaggedString result = "AlertNeedMechChargersDesc".Translate(requiredChargerDefs.Select((ThingDef c) => c.LabelCap.Resolve()).ToLineList("  - "));
		if (!ResearchProjectDefOf.BasicMechtech.IsFinished)
		{
			result += "\n\n" + "AlertNeedMechChargerBasicMechtech".Translate();
		}
		return result;
	}
}
