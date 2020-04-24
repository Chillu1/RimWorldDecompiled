using Verse;

namespace RimWorld
{
	public static class BlightUtility
	{
		public static Plant GetFirstBlightableNowPlant(IntVec3 c, Map map)
		{
			Plant plant = c.GetPlant(map);
			if (plant != null && plant.BlightableNow)
			{
				return plant;
			}
			return null;
		}

		public static Plant GetFirstBlightableEverPlant(IntVec3 c, Map map)
		{
			Plant plant = c.GetPlant(map);
			if (plant != null && plant.def.plant.Blightable)
			{
				return plant;
			}
			return null;
		}
	}
}
