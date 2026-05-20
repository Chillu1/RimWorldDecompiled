using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet;

public static class WorldTerrainColliderManager
{
	private static readonly Dictionary<PlanetLayer, Dictionary<int, GameObject>> layerWorldTerrainColliders = new Dictionary<PlanetLayer, Dictionary<int, GameObject>>();

	public static void ClearCache()
	{
		foreach (KeyValuePair<PlanetLayer, Dictionary<int, GameObject>> layerWorldTerrainCollider in layerWorldTerrainColliders)
		{
			layerWorldTerrainCollider.Deconstruct(out var _, out var value);
			foreach (KeyValuePair<int, GameObject> item in value)
			{
				item.Deconstruct(out var _, out var value2);
				Object.Destroy(value2);
			}
		}
		layerWorldTerrainColliders.Clear();
	}

	public static void EnsureRaycastCollidersUpdated()
	{
		foreach (var (planetLayer2, dictionary2) in layerWorldTerrainColliders)
		{
			foreach (KeyValuePair<int, GameObject> item in dictionary2)
			{
				item.Deconstruct(out var _, out var value);
				value.SetActive(planetLayer2.Raycastable);
			}
		}
	}

	private static GameObject CreateGameObject(PlanetLayer planetLayer, int layer)
	{
		GameObject gameObject = new GameObject($"{planetLayer} WorldTerrainCollider layer {layer}");
		Object.DontDestroyOnLoad(gameObject);
		gameObject.layer = layer;
		return gameObject;
	}

	public static GameObject Get(PlanetLayer planetLayer, int layer)
	{
		if (!layerWorldTerrainColliders.TryGetValue(planetLayer, out var value))
		{
			value = (layerWorldTerrainColliders[planetLayer] = new Dictionary<int, GameObject>());
		}
		if (!value.TryGetValue(layer, out var value2))
		{
			value2 = (value[layer] = CreateGameObject(planetLayer, layer));
		}
		value2.SetActive(value: false);
		return value2;
	}
}
