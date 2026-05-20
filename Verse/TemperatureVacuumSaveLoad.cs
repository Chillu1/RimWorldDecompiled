using System.Linq;
using UnityEngine;

namespace Verse;

public class TemperatureVacuumSaveLoad
{
	private readonly Map map;

	private ushort[] tempGrid;

	private byte[] vacuumGrid;

	private bool inVacuum;

	public TemperatureVacuumSaveLoad(Map map)
	{
		this.map = map;
	}

	public void DoExposeWork()
	{
		byte[] arr = null;
		byte[] arr2 = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			inVacuum = map.Biome.inVacuum;
			Scribe_Values.Look(ref inVacuum, "inVacuum", defaultValue: false);
			int num = Mathf.RoundToInt(map.mapTemperature.OutdoorTemp);
			ushort num2 = TempFloatToShort(num);
			byte b = VacuumFloatToByte(1f);
			ushort[] worldTempGrid = new ushort[map.cellIndices.NumGridCells];
			byte[] worldVacGrid = null;
			if (map.Biome.inVacuum)
			{
				worldVacGrid = new byte[map.cellIndices.NumGridCells];
			}
			for (int i = 0; i < map.cellIndices.NumGridCells; i++)
			{
				worldTempGrid[i] = num2;
				if (worldVacGrid != null)
				{
					worldVacGrid[i] = b;
				}
			}
			foreach (Region item in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
			{
				if (item.Room == null)
				{
					continue;
				}
				ushort num3 = TempFloatToShort(item.Room.Temperature);
				byte b2 = VacuumFloatToByte(item.Room.Vacuum);
				foreach (IntVec3 cell in item.Cells)
				{
					worldTempGrid[map.cellIndices.CellToIndex(cell)] = num3;
					if (worldVacGrid != null)
					{
						worldVacGrid[map.cellIndices.CellToIndex(cell)] = b2;
					}
				}
			}
			arr = MapSerializeUtility.SerializeUshort(map, (IntVec3 c) => worldTempGrid[map.cellIndices.CellToIndex(c)]);
			if (worldVacGrid != null)
			{
				arr2 = MapSerializeUtility.SerializeByte(map, (IntVec3 c) => worldVacGrid[map.cellIndices.CellToIndex(c)]);
			}
		}
		DataExposeUtility.LookByteArray(ref arr, "temperatures");
		DataExposeUtility.LookByteArray(ref arr2, "vacuum");
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			return;
		}
		Scribe_Values.Look(ref inVacuum, "inVacuum", defaultValue: false);
		tempGrid = new ushort[map.cellIndices.NumGridCells];
		if (inVacuum)
		{
			vacuumGrid = new byte[map.cellIndices.NumGridCells];
		}
		MapSerializeUtility.LoadUshort(arr, map, delegate(IntVec3 c, ushort val)
		{
			tempGrid[map.cellIndices.CellToIndex(c)] = val;
		});
		if (inVacuum)
		{
			MapSerializeUtility.LoadByte(arr2, map, delegate(IntVec3 c, byte val)
			{
				vacuumGrid[map.cellIndices.CellToIndex(c)] = val;
			});
		}
	}

	public void ApplyLoadedDataToRegions()
	{
		if (tempGrid == null && vacuumGrid == null)
		{
			return;
		}
		CellIndices cellIndices = map.cellIndices;
		foreach (Region item in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
		{
			if (item.Room != null)
			{
				int num = cellIndices.CellToIndex(item.Cells.First());
				if (tempGrid != null)
				{
					item.Room.Temperature = TempShortToFloat(tempGrid[num]);
				}
				if (vacuumGrid != null)
				{
					item.Room.Vacuum = VacuumByteToFloat(vacuumGrid[num]);
				}
			}
		}
		tempGrid = null;
		vacuumGrid = null;
	}

	private ushort TempFloatToShort(float temp)
	{
		temp = Mathf.Clamp(temp, -273.15f, 1000f);
		temp *= 16f;
		return (ushort)((int)temp + 32768);
	}

	private float TempShortToFloat(ushort temp)
	{
		return ((float)(int)temp - 32768f) / 16f;
	}

	private byte VacuumFloatToByte(float vacuum)
	{
		return (byte)(vacuum * 255f);
	}

	private float VacuumByteToFloat(byte vacuum)
	{
		return (float)(int)vacuum / 255f;
	}
}
