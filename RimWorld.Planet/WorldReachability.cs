using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldReachability
{
	private readonly Dictionary<PlanetLayer, int[]> layerFields = new Dictionary<PlanetLayer, int[]>();

	private int nextFieldID;

	private int impassableFieldID;

	private int minValidFieldID;

	public WorldReachability()
	{
		foreach (var (_, layer) in Find.WorldGrid.PlanetLayers)
		{
			OnPlanetLayerAdded(layer);
		}
		nextFieldID = 1;
		InvalidateAllFields();
		Find.WorldGrid.OnPlanetLayerAdded += OnPlanetLayerAdded;
		Find.WorldGrid.OnPlanetLayerRemoved += OnPlanetLayerRemoved;
	}

	private void OnPlanetLayerAdded(PlanetLayer layer)
	{
		layerFields[layer] = new int[layer.TilesCount];
	}

	private void OnPlanetLayerRemoved(PlanetLayer layer)
	{
		if (layerFields.ContainsKey(layer))
		{
			layerFields.Remove(layer);
		}
	}

	public void ClearCache()
	{
		InvalidateAllFields();
	}

	public bool CanReach(Caravan c, PlanetTile tile)
	{
		return CanReach(c.Tile, tile);
	}

	public bool CanReach(PlanetTile startTile, PlanetTile destTile)
	{
		int[] array = layerFields[startTile.Layer];
		if (startTile.Layer != destTile.Layer)
		{
			return false;
		}
		if (!startTile.Valid || startTile.tileId >= array.Length || !destTile.Valid || destTile.tileId >= array.Length)
		{
			return false;
		}
		if (array[startTile.tileId] == impassableFieldID || array[destTile.tileId] == impassableFieldID)
		{
			return false;
		}
		if (IsValidField(array[startTile.tileId]) || IsValidField(array[destTile.tileId]))
		{
			return array[startTile.tileId] == array[destTile.tileId];
		}
		FloodFillAt(startTile);
		if (array[startTile.tileId] == impassableFieldID)
		{
			return false;
		}
		return array[startTile.tileId] == array[destTile.tileId];
	}

	public int GetLocalFieldId(PlanetTile tile)
	{
		return layerFields[tile.Layer][tile.tileId] - minValidFieldID;
	}

	private void InvalidateAllFields()
	{
		if (nextFieldID == 2147483646)
		{
			nextFieldID = 1;
		}
		minValidFieldID = nextFieldID;
		impassableFieldID = nextFieldID;
		nextFieldID++;
		PlanetLayer value;
		foreach (KeyValuePair<PlanetLayer, int[]> layerField in layerFields)
		{
			layerField.Deconstruct(out value, out var value2);
			PlanetLayer layer = value;
			int[] array = value2;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] < minValidFieldID)
				{
					FloodFillAt(new PlanetTile(i, layer));
				}
			}
		}
		foreach (KeyValuePair<int, PlanetLayer> planetLayer in Find.WorldGrid.PlanetLayers)
		{
			planetLayer.Deconstruct(out var _, out value);
			value.FastTileFinder.DirtyCache();
		}
	}

	private bool IsValidField(int fieldID)
	{
		return fieldID >= minValidFieldID;
	}

	private void FloodFillAt(PlanetTile tile)
	{
		World world = Find.World;
		int[] fields = layerFields[tile.Layer];
		if (world.Impassable(tile))
		{
			fields[tile.tileId] = impassableFieldID;
			return;
		}
		tile.Layer.Filler.FloodFill(tile, (PlanetTile x) => !world.Impassable(x), delegate(PlanetTile x)
		{
			fields[x.tileId] = nextFieldID;
		});
		nextFieldID++;
	}
}
