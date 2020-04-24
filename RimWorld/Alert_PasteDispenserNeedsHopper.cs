using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_PasteDispenserNeedsHopper : Alert
	{
		private List<Thing> badDispensersResult = new List<Thing>();

		private List<Thing> BadDispensers
		{
			get
			{
				badDispensersResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Thing item in maps[i].listerThings.ThingsInGroup(ThingRequestGroup.FoodDispenser))
					{
						bool flag = false;
						ThingDef hopper = ThingDefOf.Hopper;
						foreach (IntVec3 adjCellsCardinalInBound in ((Building_NutrientPasteDispenser)item).AdjCellsCardinalInBounds)
						{
							Thing edifice = adjCellsCardinalInBound.GetEdifice(item.Map);
							if (edifice != null && edifice.def == hopper)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							badDispensersResult.Add(item);
						}
					}
				}
				return badDispensersResult;
			}
		}

		public Alert_PasteDispenserNeedsHopper()
		{
			defaultLabel = "NeedFoodHopper".Translate();
			defaultExplanation = "NeedFoodHopperDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadDispensers);
		}
	}
}
