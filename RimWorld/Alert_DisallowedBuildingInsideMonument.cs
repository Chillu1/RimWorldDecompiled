using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_DisallowedBuildingInsideMonument : Alert_Critical
	{
		private List<Thing> disallowedBuildingsResult = new List<Thing>();

		private List<Thing> DisallowedBuildings
		{
			get
			{
				disallowedBuildingsResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.MonumentMarker);
					for (int j = 0; j < list.Count; j++)
					{
						MonumentMarker monumentMarker = (MonumentMarker)list[j];
						if (monumentMarker.AllDone)
						{
							Thing firstDisallowedBuilding = monumentMarker.FirstDisallowedBuilding;
							if (firstDisallowedBuilding != null)
							{
								disallowedBuildingsResult.Add(firstDisallowedBuilding);
							}
						}
					}
				}
				return disallowedBuildingsResult;
			}
		}

		private int MinTicksLeft
		{
			get
			{
				int num = int.MaxValue;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.MonumentMarker);
					for (int j = 0; j < list.Count; j++)
					{
						MonumentMarker monumentMarker = (MonumentMarker)list[j];
						if (monumentMarker.AllDone && monumentMarker.AnyDisallowedBuilding)
						{
							num = Mathf.Min(num, 60000 - monumentMarker.ticksSinceDisallowedBuilding);
						}
					}
				}
				return num;
			}
		}

		public Alert_DisallowedBuildingInsideMonument()
		{
			defaultLabel = "DisallowedBuildingInsideMonument".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(DisallowedBuildings);
		}

		public override TaggedString GetExplanation()
		{
			return "DisallowedBuildingInsideMonumentDesc".Translate(MinTicksLeft.ToStringTicksToPeriod());
		}
	}
}
