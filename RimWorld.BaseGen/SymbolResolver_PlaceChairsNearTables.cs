using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_PlaceChairsNearTables : SymbolResolver
{
	private static List<Thing> tables = new List<Thing>();

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		tables.Clear();
		foreach (IntVec3 item in rp.rect)
		{
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.IsTable && !tables.Contains(thingList[i]))
				{
					tables.Add(thingList[i]);
				}
			}
		}
		for (int j = 0; j < tables.Count; j++)
		{
			CellRect cellRect = tables[j].OccupiedRect().ExpandedBy(1);
			bool flag = false;
			foreach (IntVec3 item2 in cellRect.EdgeCells.InRandomOrder())
			{
				if (!cellRect.IsCorner(item2) && rp.rect.Contains(item2) && item2.Standable(map) && item2.GetEdifice(map) == null && (!flag || !Rand.Bool))
				{
					Rot4 value = ((item2.x == cellRect.minX) ? Rot4.East : ((item2.x == cellRect.maxX) ? Rot4.West : ((item2.z != cellRect.minZ) ? Rot4.South : Rot4.North)));
					ResolveParams resolveParams = rp;
					resolveParams.rect = CellRect.SingleCell(item2);
					resolveParams.singleThingDef = ThingDefOf.DiningChair;
					resolveParams.singleThingStuff = rp.singleThingStuff ?? ThingDefOf.WoodLog;
					resolveParams.thingRot = value;
					BaseGen.symbolStack.Push("thing", resolveParams);
					flag = true;
				}
			}
		}
		tables.Clear();
	}
}
