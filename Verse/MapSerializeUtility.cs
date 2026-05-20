using System;

namespace Verse;

public static class MapSerializeUtility
{
	public static byte[] SerializeByte(Map map, Func<IntVec3, byte> shortReader)
	{
		return DataSerializeUtility.SerializeByte(map.info.NumCells, (int idx) => shortReader(map.cellIndices.IndexToCell(idx)));
	}

	public static void LoadByte(byte[] arr, Map map, Action<IntVec3, byte> shortWriter)
	{
		DataSerializeUtility.LoadByte(arr, map.info.NumCells, delegate(int idx, byte data)
		{
			shortWriter(map.cellIndices.IndexToCell(idx), data);
		});
	}

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

	public static byte[] SerializeInt(Map map, Func<IntVec3, int> intReader)
	{
		return DataSerializeUtility.SerializeInt(map.info.NumCells, (int idx) => intReader(map.cellIndices.IndexToCell(idx)));
	}

	public static void LoadInt(byte[] arr, Map map, Action<IntVec3, int> intWriter)
	{
		DataSerializeUtility.LoadInt(arr, map.info.NumCells, delegate(int idx, int data)
		{
			intWriter(map.cellIndices.IndexToCell(idx), data);
		});
	}

	public static byte[] SerializeUint(Map map, Func<IntVec3, uint> uintReader)
	{
		return DataSerializeUtility.SerializeUint(map.info.NumCells, (int idx) => uintReader(map.cellIndices.IndexToCell(idx)));
	}

	public static void LoadUint(byte[] arr, Map map, Action<IntVec3, uint> uintWriter)
	{
		DataSerializeUtility.LoadUint(arr, map.info.NumCells, delegate(int idx, uint data)
		{
			uintWriter(map.cellIndices.IndexToCell(idx), data);
		});
	}
}
