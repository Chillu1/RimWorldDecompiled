using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class LandmarkDef : Def
{
	public enum LandmarkDrawType
	{
		Standard,
		TerrainMask
	}

	public enum CoastRotateMode
	{
		None,
		ToCoast,
		AwayFromCoast
	}

	public Type workerClass = typeof(Landmark);

	public float commonality;

	public List<MutatorChance> mutatorChances = new List<MutatorChance>();

	public List<MutatorChance> comboLandmarkMutators = new List<MutatorChance>();

	public RulePackDef nameMaker;

	[NoTranslate]
	public string iconTexturePath;

	public IntVec2 atlasSize = IntVec2.One;

	public float drawScale = 1f;

	public LandmarkDrawType drawType;

	public CoastRotateMode coastRotateMode;

	public bool drawAboveRoads;

	[NoTranslate]
	public string category;

	public List<ShaderParameter> terrainParameters;

	private static readonly List<PlanetTile> neighbours = new List<PlanetTile>(8);

	private static Dictionary<PlanetTile, bool> cachedSettlementTiles = new Dictionary<PlanetTile, bool>();

	public Material Material => MaterialPool.MatFrom(iconTexturePath, ShaderDatabase.WorldOverlayTransparentLit, 3510);

	public Texture2D Texture => CachedTexture.Get(iconTexturePath);

	public bool EverValid()
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		foreach (MutatorChance mutatorChance in mutatorChances)
		{
			if (mutatorChance.required && !mutatorChance.mutator.EverValid())
			{
				return false;
			}
		}
		return true;
	}

	public static void ClearCache()
	{
		cachedSettlementTiles.Clear();
	}

	public bool IsValidTile(PlanetTile tile, PlanetLayer layer, bool canUseCache = false)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		bool flag = false;
		Tile tile2 = layer[tile];
		if (tile2.PrimaryBiome.impassable || tile2.hilliness == Hilliness.Impassable)
		{
			return false;
		}
		if (Find.World.landmarks[tile] != null)
		{
			return false;
		}
		if (tile2.Mutators.Any((TileMutatorDef m) => m.preventsLandmarks))
		{
			return false;
		}
		foreach (MutatorChance mutatorChance in mutatorChances)
		{
			if (mutatorChance.required)
			{
				if (!mutatorChance.mutator.IsValidTile(tile, layer))
				{
					return false;
				}
				flag = true;
			}
			if (!flag && mutatorChance.chance >= 1f && mutatorChance.mutator.IsValidTile(tile, layer))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (coastRotateMode != CoastRotateMode.None && !IsValidRotatableTile(tile, layer, coastRotateMode))
		{
			return false;
		}
		if (canUseCache)
		{
			if (!cachedSettlementTiles.ContainsKey(tile))
			{
				cachedSettlementTiles[tile] = Find.World.worldObjects.AnySettlementAt(tile);
			}
			if (cachedSettlementTiles[tile])
			{
				return false;
			}
		}
		else if (Find.World.worldObjects.AnySettlementAt(tile))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidRotatableTile(PlanetTile tile, PlanetLayer layer, CoastRotateMode mode)
	{
		layer.GetTileNeighbors(tile, neighbours);
		bool flag = false;
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (layer[neighbours[i]].Landmark != null)
			{
				return false;
			}
			if (IsValidRotatableNeighbour(layer[neighbours[i]].PrimaryBiome, mode))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidRotatableNeighbour(BiomeDef biome, CoastRotateMode mode)
	{
		if (mode == CoastRotateMode.ToCoast)
		{
			return !biome.isBackgroundBiome;
		}
		return biome.isBackgroundBiome;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (!mutatorChances.Any((MutatorChance mc) => mc.chance >= 1f))
		{
			yield return "LandmarkDef " + defName + " has no mutators with a chance of 1 or higher.";
		}
	}
}
