using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldPathGrid
{
	public readonly Dictionary<PlanetLayer, float[]> layerMovementDifficulty = new Dictionary<PlanetLayer, float[]>();

	private readonly Dictionary<PlanetLayer, int> layerAllPathCostsRecalculatedDayOfYear = new Dictionary<PlanetLayer, int>();

	private const float ImpassableMovementDifficulty = 1000f;

	public const float WinterMovementDifficultyOffset = 2f;

	public const float MaxTempForWinterOffset = 5f;

	private static int DayOfYearAt0Long => GenDate.DayOfYear(GenTicks.TicksAbs, 0f);

	public WorldPathGrid()
	{
		foreach (var (_, layer) in Find.WorldGrid.PlanetLayers)
		{
			OnPlanetLayerAdded(layer);
		}
		Find.WorldGrid.OnPlanetLayerAdded += OnPlanetLayerAdded;
		Find.WorldGrid.OnPlanetLayerRemoved += OnPlanetLayerRemoved;
	}

	public void WorldPathGridTick()
	{
		foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
		{
			if (layerAllPathCostsRecalculatedDayOfYear[planetLayer2] != DayOfYearAt0Long)
			{
				RecalculateLayerPerceivedPathCosts(planetLayer2);
			}
		}
	}

	private void OnPlanetLayerAdded(PlanetLayer layer)
	{
		layerMovementDifficulty[layer] = new float[layer.TilesCount];
		layerAllPathCostsRecalculatedDayOfYear[layer] = -1;
	}

	private void OnPlanetLayerRemoved(PlanetLayer layer)
	{
		layerMovementDifficulty.Remove(layer);
		layerAllPathCostsRecalculatedDayOfYear.Remove(layer);
	}

	public bool Passable(PlanetTile tile)
	{
		if (!Find.WorldGrid.InBounds(tile))
		{
			return false;
		}
		return layerMovementDifficulty[tile.Layer][tile.tileId] < 1000f;
	}

	public bool PassableFast(PlanetTile tile)
	{
		return layerMovementDifficulty[tile.Layer][tile.tileId] < 1000f;
	}

	public float PerceivedMovementDifficultyAt(PlanetTile tile)
	{
		return layerMovementDifficulty[tile.Layer][tile.tileId];
	}

	public void RecalculatePerceivedMovementDifficultyAt(PlanetTile tile, out bool needsRecache, int? ticksAbs = null)
	{
		needsRecache = false;
		if (Find.WorldGrid.InBounds(tile))
		{
			bool num = PassableFast(tile);
			layerMovementDifficulty[tile.Layer][tile.tileId] = CalculatedMovementDifficultyAt(tile, perceivedStatic: true, ticksAbs);
			if (num != PassableFast(tile))
			{
				needsRecache = true;
			}
		}
	}

	public void RecalculateAllLayersPathCosts()
	{
		foreach (var (layer, _) in layerMovementDifficulty)
		{
			RecalculateLayerPerceivedPathCosts(layer);
		}
	}

	public void RecalculateLayerPerceivedPathCosts(PlanetLayer layer)
	{
		RecalculateLayerPerceivedPathCosts(layer, null);
		layerAllPathCostsRecalculatedDayOfYear[layer] = DayOfYearAt0Long;
	}

	public void RecalculateLayerPerceivedPathCosts(PlanetLayer layer, int? ticksAbs)
	{
		bool flag = false;
		layerAllPathCostsRecalculatedDayOfYear[layer] = -1;
		float[] array = layerMovementDifficulty[layer];
		for (int i = 0; i < array.Length; i++)
		{
			RecalculatePerceivedMovementDifficultyAt(new PlanetTile(i, layer), out var needsRecache, ticksAbs);
			flag = flag || needsRecache;
		}
		if (flag)
		{
			Find.WorldReachability.ClearCache();
		}
	}

	public static float CalculatedMovementDifficultyAt(PlanetTile tile, bool perceivedStatic, int? ticksAbs = null, StringBuilder explanation = null)
	{
		Tile tile2 = Find.WorldGrid[tile];
		if (explanation != null && explanation.Length > 0)
		{
			explanation.AppendLine();
		}
		if (tile2.PrimaryBiome.impassable || tile2.hilliness == Hilliness.Impassable)
		{
			explanation?.Append("Impassable".Translate());
			return 1000f;
		}
		float num = 0f + tile2.PrimaryBiome.movementDifficulty;
		explanation?.Append(tile2.PrimaryBiome.LabelCap + ": " + tile2.PrimaryBiome.movementDifficulty.ToStringWithSign("0.#"));
		float num2 = HillinessMovementDifficultyOffset(tile2.hilliness);
		float num3 = num + num2;
		if (explanation != null && num2 != 0f)
		{
			explanation.AppendLine();
			explanation.Append(tile2.hilliness.GetLabelCap() + ": " + num2.ToStringWithSign("0.#"));
		}
		return num3 + GetCurrentWinterMovementDifficultyOffset(tile, ticksAbs ?? GenTicks.TicksAbs, explanation);
	}

	public static float GetCurrentWinterMovementDifficultyOffset(PlanetTile tile, int? ticksAbs = null, StringBuilder explanation = null)
	{
		if (!ticksAbs.HasValue)
		{
			ticksAbs = GenTicks.TicksAbs;
		}
		Vector2 vector = Find.WorldGrid.LongLatOf(tile);
		SeasonUtility.GetSeason(GenDate.YearPercent(ticksAbs.Value, vector.x), vector.y, out var _, out var _, out var _, out var winter, out var _, out var permanentWinter);
		float num = winter + permanentWinter;
		num *= Mathf.InverseLerp(5f, 0f, GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs.Value, tile));
		if (num > 0.01f)
		{
			float num2 = 2f * num;
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.Append("Winter".Translate());
				if (num < 0.999f)
				{
					explanation.Append(" (" + num.ToStringPercent("F0") + ")");
				}
				explanation.Append(": ");
				explanation.Append(num2.ToStringWithSign("0.#"));
			}
			return num2;
		}
		return 0f;
	}

	public static bool WillWinterEverAffectMovementDifficulty(PlanetTile tile)
	{
		int ticksAbs = GenTicks.TicksAbs;
		for (int i = 0; i < 3600000; i += 60000)
		{
			if (GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs + i, tile) < 5f)
			{
				return true;
			}
		}
		return false;
	}

	private static float HillinessMovementDifficultyOffset(Hilliness hilliness)
	{
		return hilliness switch
		{
			Hilliness.Flat => 0f, 
			Hilliness.SmallHills => 0.5f, 
			Hilliness.LargeHills => 1.5f, 
			Hilliness.Mountainous => 3f, 
			Hilliness.Impassable => 1000f, 
			_ => 0f, 
		};
	}
}
