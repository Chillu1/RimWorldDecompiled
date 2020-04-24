using System;

namespace Verse
{
	public static class MapSerializeUtility
	{
		public static byte[] SerializeUshort(Map map, Func<IntVec3, ushort> shortReader)
		{
			return DataSerializeUtility.SerializeUshort(map.info.NumCells, (int idx) => shortReader(map.cellIndices.IndexToCell(idx)));
		}

		public static void LoadUshort(byte[] arr, Map map, Action<IntVec3, ushort> shortWriter)
		{
			DataSerializeUtility.LoadUshort(arr, map.info.NumCells, delegate(int idx, ushort data)
			{
				shortWriter(map.cellIndices.IndexToCell(idx), data);
			});
		}
	}
}
