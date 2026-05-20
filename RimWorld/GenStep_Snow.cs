using Verse;

namespace RimWorld;

public class GenStep_Snow : GenStep
{
	public override int SeedPart => 306693816;

	public override void Generate(Map map, GenStepParams parms)
	{
		int num = 0;
		for (int i = (int)(GenLocalDate.Twelfth(map) - 2); i <= (int)GenLocalDate.Twelfth(map); i++)
		{
			int num2 = i;
			if (num2 < 0)
			{
				num2 += 12;
			}
			Twelfth twelfth = (Twelfth)num2;
			if (GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth) < 0f)
			{
				num++;
			}
		}
		float num3 = 0f;
		switch (num)
		{
		case 0:
			return;
		case 1:
			num3 = 0.3f;
			break;
		case 2:
			num3 = 0.7f;
			break;
		case 3:
			num3 = 1f;
			break;
		}
		float seasonalTemp = map.mapTemperature.SeasonalTemp;
		if (seasonalTemp > 0f)
		{
			num3 *= 0.4f;
		}
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (ModsConfig.OdysseyActive && seasonalTemp < 0f && FreezeManager.CanEverFreeze(allCell, map))
			{
				map.terrainGrid.SetTempTerrain(allCell, TerrainDefOf.ThinIce);
			}
			if ((double)num3 > 0.4 && !allCell.Roofed(map))
			{
				map.steadyEnvironmentEffects.AddFallenSnowAt(allCell, num3);
			}
		}
		foreach (IntVec3 allCell2 in map.AllCells)
		{
			if (map.terrainGrid.TerrainAt(allCell2).meltSnowRadius > 0f)
			{
				WeatherBuildupUtility.AddSnowRadial(allCell2, map, map.terrainGrid.TerrainAt(allCell2).meltSnowRadius, -1f);
			}
		}
	}
}
