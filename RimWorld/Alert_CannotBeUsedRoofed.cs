using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_CannotBeUsedRoofed : Alert
	{
		private List<ThingDef> thingDefsToCheck;

		private List<Thing> unusableBuildingsResult = new List<Thing>();

		private List<Thing> UnusableBuildings
		{
			get
			{
				unusableBuildingsResult.Clear();
				if (thingDefsToCheck == null)
				{
					thingDefsToCheck = new List<ThingDef>();
					foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
					{
						if (!item.canBeUsedUnderRoof)
						{
							thingDefsToCheck.Add(item);
						}
					}
				}
				List<Map> maps = Find.Maps;
				Faction ofPlayer = Faction.OfPlayer;
				for (int i = 0; i < thingDefsToCheck.Count; i++)
				{
					for (int j = 0; j < maps.Count; j++)
					{
						List<Thing> list = maps[j].listerThings.ThingsOfDef(thingDefsToCheck[i]);
						for (int k = 0; k < list.Count; k++)
						{
							if (list[k].Faction == ofPlayer && RoofUtility.IsAnyCellUnderRoof(list[k]))
							{
								unusableBuildingsResult.Add(list[k]);
							}
						}
					}
				}
				return unusableBuildingsResult;
			}
		}

		public Alert_CannotBeUsedRoofed()
		{
			defaultLabel = "BuildingCantBeUsedRoofed".Translate();
			defaultExplanation = "BuildingCantBeUsedRoofedDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(UnusableBuildings);
		}
	}
}
