using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldRenderer
{
	public WorldRenderMode wantedMode;

	private bool asynchronousRegenerationActive;

	private bool ShouldRegenerateDirtyLayersInLongEvent
	{
		get
		{
			foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
			{
				if (!planetLayer2.Visible)
				{
					continue;
				}
				foreach (WorldDrawLayer worldDrawLayer in planetLayer2.WorldDrawLayers)
				{
					if (worldDrawLayer.Dirty && worldDrawLayer is WorldDrawLayer_Terrain)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public IEnumerable<WorldDrawLayerBase> AllDrawLayers
	{
		get
		{
			foreach (WorldDrawLayerBase globalLayer in Find.WorldGrid.GlobalLayers)
			{
				yield return globalLayer;
			}
			foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
			{
				foreach (WorldDrawLayer worldDrawLayer in planetLayer2.WorldDrawLayers)
				{
					yield return worldDrawLayer;
				}
			}
		}
	}

	public IEnumerable<WorldDrawLayerBase> AllVisibleDrawLayers
	{
		get
		{
			foreach (WorldDrawLayerBase globalLayer in Find.WorldGrid.GlobalLayers)
			{
				if (globalLayer.Visible)
				{
					yield return globalLayer;
				}
			}
			foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
			{
				if (!planetLayer2.Visible)
				{
					continue;
				}
				foreach (WorldDrawLayer worldDrawLayer in planetLayer2.WorldDrawLayers)
				{
					yield return worldDrawLayer;
				}
			}
		}
	}

	public void SetAllLayersDirty()
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			allDrawLayer.SetDirty();
		}
	}

	public void SetDirty<T>(PlanetLayer planetLayer) where T : WorldDrawLayer
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (allDrawLayer is WorldDrawLayer worldDrawLayer && planetLayer == worldDrawLayer.planetLayer && allDrawLayer is T)
			{
				allDrawLayer.SetDirty();
			}
		}
	}

	public bool HasGlobalLayer<T>() where T : WorldDrawLayer
	{
		T layer;
		return TryGetGlobalLayer<T>(out layer);
	}

	public bool HasLayer<T>(PlanetLayer planetLayer) where T : WorldDrawLayer
	{
		T layer;
		return TryGetLayer<T>(planetLayer, out layer);
	}

	public bool TryGetGlobalLayer<T>(out T layer) where T : WorldDrawLayer
	{
		layer = GetGlobalLayer<T>();
		return layer != null;
	}

	public bool TryGetLayer<T>(PlanetLayer planetLayer, out T layer) where T : WorldDrawLayer
	{
		layer = GetLayer<T>(planetLayer);
		return layer != null;
	}

	public T GetGlobalLayer<T>() where T : WorldDrawLayer
	{
		foreach (WorldDrawLayerBase globalLayer in Find.WorldGrid.GlobalLayers)
		{
			if (globalLayer is T result)
			{
				return result;
			}
		}
		return null;
	}

	public T GetLayer<T>(PlanetLayer planetLayer) where T : WorldDrawLayer
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (allDrawLayer is WorldDrawLayer worldDrawLayer && planetLayer == worldDrawLayer.planetLayer && allDrawLayer is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void RegenerateAllLayersNow()
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (allDrawLayer.Visible)
			{
				allDrawLayer.RegenerateNow();
			}
		}
	}

	private IEnumerable RegenerateDirtyLayersNow_Async()
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (!allDrawLayer.Dirty || !allDrawLayer.Visible)
			{
				continue;
			}
			{
				IEnumerator enumerator2 = allDrawLayer.Regenerate().GetEnumerator();
				try
				{
					while (true)
					{
						try
						{
							if (!enumerator2.MoveNext())
							{
								break;
							}
						}
						catch (Exception arg)
						{
							Log.Error($"Could not regenerate WorldLayer: {arg}");
							break;
						}
						yield return enumerator2.Current;
					}
				}
				finally
				{
					IDisposable disposable = enumerator2 as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			yield return null;
		}
		asynchronousRegenerationActive = false;
	}

	public void Notify_StaticWorldObjectPosChanged()
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (allDrawLayer is WorldDrawLayer_WorldObjects worldDrawLayer_WorldObjects)
			{
				worldDrawLayer_WorldObjects.SetDirty();
			}
		}
	}

	public void Notify_TilePollutionChanged(PlanetTile tileId)
	{
		foreach (WorldDrawLayerBase allDrawLayer in AllDrawLayers)
		{
			if (allDrawLayer is WorldDrawLayer_Pollution worldDrawLayer_Pollution)
			{
				worldDrawLayer_Pollution.Notify_TilePollutionChanged(tileId);
			}
		}
	}

	public void CheckActivateWorldCamera()
	{
		Find.WorldCamera.gameObject.SetActive(WorldRendererUtility.WorldRendered);
	}

	public bool RegenerateLayersIfDirtyInLongEvent()
	{
		if (ShouldRegenerateDirtyLayersInLongEvent)
		{
			asynchronousRegenerationActive = true;
			LongEventHandler.QueueLongEvent(RegenerateDirtyLayersNow_Async(), "GeneratingPlanet", null, showExtraUIInfo: false);
			return true;
		}
		return false;
	}

	public void DrawWorldLayers()
	{
		if (asynchronousRegenerationActive)
		{
			Log.Error("Called DrawWorldLayers() but already regenerating. This shouldn't ever happen because LongEventHandler should have stopped us.");
		}
		else
		{
			if (RegenerateLayersIfDirtyInLongEvent())
			{
				return;
			}
			GlobalRendererUtility.UpdateGlobalShadersParams();
			WorldRendererUtility.UpdateGlobalShadersParams();
			foreach (WorldDrawLayerBase globalLayer in Find.WorldGrid.GlobalLayers)
			{
				if (globalLayer.Visible)
				{
					try
					{
						globalLayer.Render();
					}
					catch (Exception arg)
					{
						Log.Error($"Error drawing global WorldLayer: {arg}");
					}
				}
			}
			foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
			{
				if (!planetLayer2.Visible)
				{
					continue;
				}
				foreach (WorldDrawLayer worldDrawLayer in planetLayer2.WorldDrawLayers)
				{
					try
					{
						worldDrawLayer.Render();
					}
					catch (Exception arg2)
					{
						Log.Error($"Error drawing planet layer WorldLayer {worldDrawLayer.GetType().Name}: {arg2} on layer {planetLayer2.Def.label}");
					}
				}
			}
		}
	}

	public PlanetTile GetTileFromRayHit(RaycastHit hit)
	{
		foreach (WorldDrawLayerBase allVisibleDrawLayer in AllVisibleDrawLayers)
		{
			if (allVisibleDrawLayer is WorldDrawLayer { Raycastable: not false } worldDrawLayer && worldDrawLayer.TryGetTileFromRayHit(hit, out var id))
			{
				return id;
			}
		}
		return PlanetTile.Invalid;
	}
}
