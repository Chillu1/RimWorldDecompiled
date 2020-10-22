using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DeepDrillUtility
	{
		public const int NumCellsToScan = 21;

		public static ThingDef GetNextResource(IntVec3 p, Map map)
		{
			GetNextResource(p, map, out var resDef, out var _, out var _);
			return resDef;
		}

		public static bool GetNextResource(IntVec3 p, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			for (int i = 0; i < 21; i++)
			{
				IntVec3 intVec = p + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					ThingDef thingDef = map.deepResourceGrid.ThingDefAt(intVec);
					if (thingDef != null)
					{
						resDef = thingDef;
						countPresent = map.deepResourceGrid.CountAt(intVec);
						cell = intVec;
						return true;
					}
				}
			}
			resDef = GetBaseResource(map, p);
			countPresent = int.MaxValue;
			cell = p;
			return false;
		}

		public static ThingDef GetBaseResource(Map map, IntVec3 cell)
		{
			if (!map.Biome.hasBedrock)
			{
				return null;
			}
			Rand.PushState();
			Rand.Seed = cell.GetHashCode();
			ThingDef result = (from rock in Find.World.NaturalRockTypesIn(map.Tile)
				select rock.building.mineableThing).RandomElement();
			Rand.PopState();
			return result;
		}
	}
}
