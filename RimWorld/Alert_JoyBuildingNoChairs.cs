using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Alert_JoyBuildingNoChairs : Alert
	{
		private List<Thing> badBuildingsResult = new List<Thing>();

		protected abstract JoyGiverDef JoyGiver { get; }

		private List<Thing> BadBuildings
		{
			get
			{
				badBuildingsResult.Clear();
				List<Map> maps = Find.Maps;
				foreach (ThingDef thingDef in JoyGiver.thingDefs)
				{
					for (int i = 0; i < maps.Count; i++)
					{
						foreach (Thing item in maps[i].listerThings.ThingsOfDef(thingDef))
						{
							if (item.Faction == Faction.OfPlayer && !JoyBuildingUsable(item))
							{
								badBuildingsResult.Add(item);
							}
						}
					}
				}
				return badBuildingsResult;
			}
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadBuildings);
		}

		private bool JoyBuildingUsable(Thing t)
		{
			List<IntVec3> list = GenAdjFast.AdjacentCellsCardinal(t);
			for (int i = 0; i < list.Count; i++)
			{
				Building edifice = list[i].GetEdifice(t.Map);
				if (edifice != null && edifice.def.building.isSittable)
				{
					return true;
				}
			}
			return false;
		}
	}
}
