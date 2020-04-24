using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MapMeshFlagUtility
	{
		public static List<MapMeshFlag> allFlags;

		static MapMeshFlagUtility()
		{
			allFlags = new List<MapMeshFlag>();
			foreach (MapMeshFlag value in Enum.GetValues(typeof(MapMeshFlag)))
			{
				if (value != 0)
				{
					allFlags.Add(value);
				}
			}
		}
	}
}
