using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Landmarks : WorldGenStep
{
	private const float LandmarkDensity = 0.0022222223f;

	private static readonly Dictionary<LandmarkDef, HashSet<int>> landmarkCandidates = new Dictionary<LandmarkDef, HashSet<int>>();

	private static readonly HashSet<int> usedTiles = new HashSet<int>();

	public override int SeedPart => 471293987;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		landmarkCandidates.Clear();
		usedTiles.Clear();
		using (ProfilerBlock.Scope("Generate Landmarks"))
		{
			float num = 0.0022222223f * Find.World.info.landmarkDensity.GetScaleFactor();
			int num2 = Mathf.RoundToInt((float)layer.TilesCount * num);
			int num3 = 0;
			List<LandmarkDef> list = DefDatabase<LandmarkDef>.AllDefsListForReading.ToList();
			List<int> list2 = Enumerable.Range(0, layer.TilesCount - 1).ToList();
			list2.Shuffle();
			int num4 = 0;
			HashSet<int> hashSet = new HashSet<int>();
			foreach (MapParent mapParent in Find.WorldObjects.MapParents)
			{
				if (mapParent.Tile.Layer == layer)
				{
					hashSet.Add(mapParent.Tile.tileId);
				}
			}
			foreach (LandmarkDef item in list)
			{
				if (!item.EverValid())
				{
					continue;
				}
				landmarkCandidates[item] = new HashSet<int>();
				for (int i = 0; i < list2.Count; i++)
				{
					num4 = (num4 + 1) % list2.Count;
					int num5 = list2[num4];
					if (!hashSet.Contains(num5))
					{
						if (item.IsValidTile(new PlanetTile(num5, layer), layer, canUseCache: true))
						{
							landmarkCandidates[item].Add(num5);
						}
						if (landmarkCandidates[item].Count >= num2)
						{
							break;
						}
					}
				}
			}
			list.RemoveAll((LandmarkDef ld) => !landmarkCandidates.ContainsKey(ld) || landmarkCandidates[ld].Count == 0);
			LandmarkDef.ClearCache();
			int num6 = 999999;
			while (num3 < num2 && num6-- > 0)
			{
				LandmarkDef landmarkDef = list.RandomElementByWeight((LandmarkDef landmark) => landmark.commonality);
				int num7 = landmarkCandidates[landmarkDef].FirstOrFallback(-1);
				if (num7 != -1)
				{
					landmarkCandidates[landmarkDef].Remove(num7);
					if (!usedTiles.Contains(num7))
					{
						num3++;
						Find.World.landmarks.AddLandmark(landmarkDef, new PlanetTile(num7, layer), layer);
						usedTiles.Add(num7);
					}
				}
			}
		}
	}
}
