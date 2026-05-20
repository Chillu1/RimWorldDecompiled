using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRoleWorker_Kitchen : RoomRoleWorker
{
	public override float GetScore(Room room)
	{
		int num = 0;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			Thing thing = containedAndAdjacentThings[i];
			if (thing.def.designationCategory != DesignationCategoryDefOf.Production)
			{
				continue;
			}
			for (int j = 0; j < thing.def.AllRecipes.Count; j++)
			{
				RecipeDef recipeDef = thing.def.AllRecipes[j];
				int num2 = 0;
				while (num2 < recipeDef.products.Count)
				{
					ThingDef thingDef = recipeDef.products[num2].thingDef;
					if (!thingDef.IsNutritionGivingIngestible || !thingDef.ingestible.HumanEdible)
					{
						num2++;
						continue;
					}
					goto IL_0077;
				}
				continue;
				IL_0077:
				num++;
				break;
			}
		}
		return (float)num * 28f;
	}

	public override float GetScoreDeltaIfBuildingPlaced(Room room, ThingDef buildingDef)
	{
		if (buildingDef.designationCategory != DesignationCategoryDefOf.Production)
		{
			return 0f;
		}
		for (int i = 0; i < buildingDef.AllRecipes.Count; i++)
		{
			RecipeDef recipeDef = buildingDef.AllRecipes[i];
			for (int j = 0; j < recipeDef.products.Count; j++)
			{
				ThingDef thingDef = recipeDef.products[j].thingDef;
				if (thingDef.IsNutritionGivingIngestible && thingDef.ingestible.HumanEdible)
				{
					return 28f;
				}
			}
		}
		return 0f;
	}
}
