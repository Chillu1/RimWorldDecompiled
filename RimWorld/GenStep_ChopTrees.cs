using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_ChopTrees : GenStep
{
	public int radialRange = 35;

	public override int SeedPart => 874575948;

	public override void Generate(Map map, GenStepParams parms)
	{
		IntVec3 center;
		if (MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var var))
		{
			center = var.CenterCell;
		}
		else
		{
			int num = 0;
			int num2 = 0;
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			foreach (Thing item in list)
			{
				num += item.Position.x;
				num2 += item.Position.z;
			}
			center = new IntVec3(num / list.Count, 0, num2 / list.Count);
		}
		foreach (IntVec3 item2 in GenRadial.RadialCellsAround(center, radialRange, useCenter: true))
		{
			if (!item2.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item2.GetThingList(map);
			for (int num3 = thingList.Count - 1; num3 >= 0; num3--)
			{
				if (thingList[num3] is Plant plant && plant.def.plant.IsTree)
				{
					plant.TrySpawnStump(PlantDestructionMode.Chop);
					plant.Destroy();
				}
			}
		}
	}
}
