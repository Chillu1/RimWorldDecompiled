using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class BuildRelatedCommandUtility
{
	public static IEnumerable<Command> RelatedBuildCommands(BuildableDef building)
	{
		foreach (Command item in BuildFacilityCommandUtility.BuildFacilityCommands(building))
		{
			yield return item;
		}
		if (!(building is ThingDef buildingDef))
		{
			yield break;
		}
		List<ThingDef> list = buildingDef.building?.relatedBuildCommands;
		if (list != null)
		{
			foreach (ThingDef item2 in list)
			{
				if (ModsConfig.IdeologyActive && building.ideoBuilding && item2.ideoBuilding)
				{
					Ideo ideo = Find.FactionManager.OfPlayer.ideos.AllIdeos.FirstOrDefault((Ideo i) => IdeoHasBuilding(i, (ThingDef)building));
					if (ideo != null)
					{
						bool flag = false;
						foreach (Ideo allIdeo in Find.FactionManager.OfPlayer.ideos.AllIdeos)
						{
							if (allIdeo == ideo && IdeoHasBuilding(allIdeo, item2))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
				}
				Designator_Build designator_Build = BuildCopyCommandUtility.FindAllowedDesignator(item2);
				if (designator_Build != null)
				{
					yield return designator_Build;
				}
			}
		}
		List<TerrainDef> list2 = buildingDef.building?.relatedTerrain;
		if (list2 == null)
		{
			yield break;
		}
		foreach (TerrainDef item3 in list2)
		{
			Designator_Build designator_Build2 = BuildCopyCommandUtility.FindAllowedDesignator(item3);
			if (designator_Build2 != null)
			{
				yield return designator_Build2;
			}
		}
		static bool IdeoHasBuilding(Ideo ideo2, ThingDef td)
		{
			if (!ideo2.HasPreceptForBuilding(td))
			{
				return ideo2.PreceptsListForReading.Any((Precept p) => p is Precept_RitualSeat precept_RitualSeat && precept_RitualSeat.ThingDef == td);
			}
			return true;
		}
	}
}
