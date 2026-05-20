namespace Verse;

public static class RoofCollapseUtility
{
	public const float RoofMaxSupportDistance = 6.9f;

	public static readonly int RoofSupportRadialCellsCount = GenRadial.NumCellsInRadius(6.9f);

	public static bool WithinRangeOfRoofHolder(IntVec3 c, Map map, bool assumeNonNoRoofCellsAreRoofed = false)
	{
		bool connected = false;
		map.floodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed(map) || x == c || (assumeNonNoRoofCellsAreRoofed && !map.areaManager.NoRoof[x])) && x.InHorDistOf(c, 6.9f), delegate(IntVec3 x)
		{
			for (int i = 0; i < 5; i++)
			{
				IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
				if (c2.InBounds(map) && c2.InHorDistOf(c, 6.9f))
				{
					Building edifice = c2.GetEdifice(map);
					if (edifice != null && edifice.def.holdsRoof)
					{
						connected = true;
						return true;
					}
				}
			}
			return false;
		});
		return connected;
	}

	public static bool ConnectedToRoofHolder(IntVec3 c, Map map, bool assumeRoofAtRoot)
	{
		bool connected = false;
		map.floodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed(map) || (x == c && assumeRoofAtRoot)) && !connected, delegate(IntVec3 x)
		{
			for (int i = 0; i < 5; i++)
			{
				IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
				if (c2.InBounds(map))
				{
					Building edifice = c2.GetEdifice(map);
					if (edifice != null && edifice.def.holdsRoof)
					{
						connected = true;
						break;
					}
				}
			}
		});
		return connected;
	}
}
