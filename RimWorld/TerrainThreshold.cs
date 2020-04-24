using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TerrainThreshold
	{
		public TerrainDef terrain;

		public float min = -1000f;

		public float max = 1000f;

		public static TerrainDef TerrainAtValue(List<TerrainThreshold> threshes, float val)
		{
			for (int i = 0; i < threshes.Count; i++)
			{
				if (threshes[i].min <= val && threshes[i].max >= val)
				{
					return threshes[i].terrain;
				}
			}
			return null;
		}
	}
}
