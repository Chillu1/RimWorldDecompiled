using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_MixedBiome : TileMutatorWorker
{
	private const float DisplaceMacroFrequency = 0.006f;

	private const float DisplaceMacroStrength = 40f;

	private const float DisplaceFrequency = 0.015f;

	private const float DisplaceStrength = 20f;

	private static readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	public TileMutatorWorker_MixedBiome(TileMutatorDef def)
		: base(def)
	{
	}

	public override bool IsValidTile(PlanetTile tile, PlanetLayer layer)
	{
		if (!def.biomeWhitelist.Contains(layer[tile].PrimaryBiome))
		{
			return false;
		}
		return GetNeighbourTile(tile, layer).HasValue;
	}

	public override void Init(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		base.Init(map);
		PlanetTile? neighbourTile = GetNeighbourTile(map.Tile, map.Tile.Layer);
		if (!neighbourTile.HasValue)
		{
			Log.Error("No neighbour tile found for mixed biome tile");
			return;
		}
		ModuleBase input = new DistFromAxis_Directional((float)map.Size.x / 2f);
		float angle = GetAngle(map.Tile, neighbourTile.Value.Tile.PrimaryBiome);
		input = new Rotate(0.0, angle, 0.0, input);
		input = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.006f, 40f, 2);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.015f, 20f, 6);
		NoiseDebugUI.StoreNoiseRender(input, "biomeVal");
		bool[] array = new bool[map.Size.x * map.Size.z];
		for (int i = 0; i < array.Length; i++)
		{
			IntVec3 coordinate = map.cellIndices.IndexToCell(i);
			array[i] = input.GetValue(coordinate) < 0f;
		}
		MixedBiomeMapComponent obj = map.components.First((MapComponent c) => c is MixedBiomeMapComponent) as MixedBiomeMapComponent;
		obj.biomeGrid = array;
		obj.secondaryBiome = neighbourTile.Value.Tile.PrimaryBiome;
	}

	private float GetAngle(PlanetTile tile, BiomeDef otherBiome)
	{
		tmpNeighbors.Clear();
		Find.World.grid.GetTileNeighbors(tile, tmpNeighbors);
		float num = GenMath.MeanAngle((from t in tmpNeighbors
			where t.Tile.PrimaryBiome == otherBiome
			select Find.WorldGrid.GetHeadingFromTo(t, tile)).ToList());
		return (450f - num) % 360f;
	}

	private PlanetTile? GetNeighbourTile(PlanetTile tile, PlanetLayer layer)
	{
		BiomeDef primaryBiome = layer[tile].PrimaryBiome;
		tmpNeighbors.Clear();
		Rand.PushState(tile.tileId);
		layer.GetTileNeighbors(tile, tmpNeighbors);
		tmpNeighbors.Shuffle();
		Rand.PopState();
		foreach (PlanetTile tmpNeighbor in tmpNeighbors)
		{
			BiomeDef primaryBiome2 = layer[tmpNeighbor].PrimaryBiome;
			if (def.biomeWhitelist.Contains(primaryBiome2) && primaryBiome2 != primaryBiome)
			{
				return tmpNeighbor;
			}
		}
		return null;
	}

	public BiomeDef SecondaryBiome(PlanetTile tile, PlanetLayer layer)
	{
		PlanetTile? neighbourTile = GetNeighbourTile(tile, layer);
		if (!neighbourTile.HasValue)
		{
			return null;
		}
		return layer[neighbourTile.Value].PrimaryBiome;
	}
}
