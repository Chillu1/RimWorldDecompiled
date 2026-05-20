namespace Verse;

public class ScattererValidator_TerrainDef : ScattererValidator
{
	public int radius = 1;

	public TerrainDef terrainDef;

	public override bool Allows(IntVec3 c, Map map)
	{
		CellRect cellRect = CellRect.CenteredOn(c, radius);
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				IntVec3 c2 = new IntVec3(j, 0, i);
				if (!c2.InBounds(map))
				{
					return false;
				}
				if (c2.GetTerrain(map) != terrainDef)
				{
					return false;
				}
			}
		}
		return true;
	}
}
