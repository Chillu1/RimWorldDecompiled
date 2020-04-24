using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Lakes : WorldGenStep
	{
		private const int LakeMaxSize = 15;

		public override int SeedPart => 401463656;

		public override void GenerateFresh(string seed)
		{
			GenerateLakes();
		}

		private void GenerateLakes()
		{
			WorldGrid grid = Find.WorldGrid;
			bool[] touched = new bool[grid.TilesCount];
			List<int> oceanChunk = new List<int>();
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (touched[i] || grid[i].biome != BiomeDefOf.Ocean)
				{
					continue;
				}
				Find.WorldFloodFiller.FloodFill(i, (int tid) => grid[tid].biome == BiomeDefOf.Ocean, delegate(int tid)
				{
					oceanChunk.Add(tid);
					touched[tid] = true;
				});
				if (oceanChunk.Count <= 15)
				{
					for (int j = 0; j < oceanChunk.Count; j++)
					{
						grid[oceanChunk[j]].biome = BiomeDefOf.Lake;
					}
				}
				oceanChunk.Clear();
			}
		}
	}
}
