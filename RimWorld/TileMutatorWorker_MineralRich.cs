using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_MineralRich : TileMutatorWorker
{
	private Perlin perlin;

	private const int SeedPart = 525214866;

	private List<ThingDef> cachedMineables;

	private List<ThingDef> Mineables
	{
		get
		{
			if (cachedMineables == null)
			{
				cachedMineables = new List<ThingDef>();
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (allDef.building?.mineableThing != null && allDef.building.isResourceRock && !def.resourceBlacklist.Contains(allDef))
					{
						cachedMineables.Add(allDef);
					}
				}
			}
			return cachedMineables;
		}
	}

	private Perlin Perlin
	{
		get
		{
			if (perlin == null)
			{
				perlin = new Perlin(0.4000000059604645, 0.5, 0.5, 4, normalized: true, invert: false, Gen.HashCombineInt(Find.World.info.Seed, 525214866), QualityMode.Medium);
			}
			return perlin;
		}
	}

	public TileMutatorWorker_MineralRich(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return "MineralRich".Translate(NamedArgumentUtility.Named(GetMineableThingDefForTile(tile), "MINERAL"));
	}

	public override string GetDescription(PlanetTile tile)
	{
		return "MineralRichDescription".Translate(NamedArgumentUtility.Named(GetMineableThingDefForTile(tile), "MINERAL"));
	}

	public override void GeneratePostTerrain(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			float resourceBlotchesPer10KCellsForMap = GenStep_RocksFromGrid.GetResourceBlotchesPer10KCellsForMap(map);
			ThingDef mineableThingDefForTile = GetMineableThingDefForTile(map.Tile);
			float num = Mineables.Sum((ThingDef d) => d.building.mineableScatterCommonality);
			float num2 = mineableThingDefForTile.building.mineableScatterCommonality / num;
			resourceBlotchesPer10KCellsForMap *= num2;
			int num3 = Mathf.RoundToInt(10000f / resourceBlotchesPer10KCellsForMap);
			int x = map.Size.x;
			if (Mathf.RoundToInt((float)(x * x) / (float)num3) < 2)
			{
				resourceBlotchesPer10KCellsForMap = 10000f / ((float)(x * x) * 0.5f);
			}
			GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable
			{
				maxValue = float.MaxValue,
				countPer10kCellsRange = new FloatRange(resourceBlotchesPer10KCellsForMap, resourceBlotchesPer10KCellsForMap),
				forcedDefToScatter = mineableThingDefForTile
			};
			for (int num4 = 0; num4 < 2; num4++)
			{
				genStep_ScatterLumpsMineable.Generate(map, default(GenStepParams));
			}
		}
	}

	private ThingDef GetMineableThingDefForTile(PlanetTile tile)
	{
		List<ThingDef> mineables = Mineables;
		Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
		float value = Perlin.GetValue(tileCenter);
		int count = mineables.Count;
		int num = (int)(value * (float)count);
		if (num >= count)
		{
			num = count - 1;
		}
		return mineables[num];
	}
}
