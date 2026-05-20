using System;

namespace Verse;

public static class MapExposeUtility
{
	public static void ExposeUshort(Map map, Func<IntVec3, ushort> shortReader, Action<IntVec3, ushort> shortWriter, string label)
	{
		byte[] arr = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			arr = MapSerializeUtility.SerializeUshort(map, shortReader);
		}
		DataExposeUtility.LookByteArray(ref arr, label);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			MapSerializeUtility.LoadUshort(arr, map, shortWriter);
		}
	}

	public static void ExposeInt(Map map, Func<IntVec3, int> intReader, Action<IntVec3, int> intWriter, string label)
	{
		byte[] arr = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			arr = MapSerializeUtility.SerializeInt(map, intReader);
		}
		DataExposeUtility.LookByteArray(ref arr, label);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			MapSerializeUtility.LoadInt(arr, map, intWriter);
		}
	}

	public static void ExposeUint(Map map, Func<IntVec3, uint> uintReader, Action<IntVec3, uint> uintWriter, string label)
	{
		byte[] arr = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			arr = MapSerializeUtility.SerializeUint(map, uintReader);
		}
		DataExposeUtility.LookByteArray(ref arr, label);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			MapSerializeUtility.LoadUint(arr, map, uintWriter);
		}
	}
}
