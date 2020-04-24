using System.Linq;
using UnityEngine;

namespace Verse
{
	public class TemperatureSaveLoad
	{
		private Map map;

		private ushort[] tempGrid;

		public TemperatureSaveLoad(Map map)
		{
			this.map = map;
		}

		public void DoExposeWork()
		{
			byte[] arr = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				int num = Mathf.RoundToInt(map.mapTemperature.OutdoorTemp);
				ushort num2 = TempFloatToShort(num);
				ushort[] tempGrid = new ushort[map.cellIndices.NumGridCells];
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					tempGrid[i] = num2;
				}
				foreach (Region item in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (item.Room != null)
					{
						ushort num3 = TempFloatToShort(item.Room.Temperature);
						foreach (IntVec3 cell in item.Cells)
						{
							tempGrid[map.cellIndices.CellToIndex(cell)] = num3;
						}
					}
				}
				arr = MapSerializeUtility.SerializeUshort(map, (IntVec3 c) => tempGrid[map.cellIndices.CellToIndex(c)]);
			}
			DataExposeUtility.ByteArray(ref arr, "temperatures");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.tempGrid = new ushort[map.cellIndices.NumGridCells];
				MapSerializeUtility.LoadUshort(arr, map, delegate(IntVec3 c, ushort val)
				{
					this.tempGrid[map.cellIndices.CellToIndex(c)] = val;
				});
			}
		}

		public void ApplyLoadedDataToRegions()
		{
			if (tempGrid != null)
			{
				CellIndices cellIndices = map.cellIndices;
				foreach (Region item in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (item.Room != null)
					{
						item.Room.Group.Temperature = TempShortToFloat(tempGrid[cellIndices.CellToIndex(item.Cells.First())]);
					}
				}
				tempGrid = null;
			}
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
	}
}
