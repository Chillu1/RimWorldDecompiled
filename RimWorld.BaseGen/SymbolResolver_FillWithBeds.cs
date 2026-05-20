using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_FillWithBeds : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		ThingDef thingDef = ((rp.singleThingDef != null) ? rp.singleThingDef : ((rp.faction == null || (int)rp.faction.def.techLevel < 3) ? Rand.Element(ThingDefOf.Bed, ThingDefOf.Bedroll, ThingDefOf.SleepingSpot) : ThingDefOf.Bed));
		ThingDef singleThingStuff = ((rp.singleThingStuff == null || !rp.singleThingStuff.stuffProps.CanMake(thingDef)) ? BaseGenUtility.CheapStuffFor(thingDef, rp.faction) : rp.singleThingStuff);
		bool flag = Rand.Bool;
		int num = rp.bedCount ?? int.MaxValue;
		int num2 = 0;
		foreach (IntVec3 item in rp.rect)
		{
			if (flag)
			{
				if (item.x % 3 != 0 || item.z % 2 != 0)
				{
					continue;
				}
			}
			else if (item.x % 2 != 0 || item.z % 3 != 0)
			{
				continue;
			}
			Rot4 rot = (flag ? Rot4.West : Rot4.North);
			if (!GenSpawn.WouldWipeAnythingWith(item, rot, thingDef, map, (Thing x) => x.def.category == ThingCategory.Building) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(GenAdj.OccupiedRect(item, rot, thingDef.Size), map))
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = GenAdj.OccupiedRect(item, rot, thingDef.size);
				resolveParams.singleThingDef = thingDef;
				resolveParams.singleThingStuff = singleThingStuff;
				resolveParams.thingRot = rot;
				BaseGen.symbolStack.Push("bed", resolveParams);
				num2++;
				if (num2 >= num)
				{
					break;
				}
			}
		}
	}
}
