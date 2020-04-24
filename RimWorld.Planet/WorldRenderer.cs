using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldRenderer
	{
		private List<WorldLayer> layers = new List<WorldLayer>();

		public WorldRenderMode wantedMode;

		private bool asynchronousRegenerationActive;

		private bool ShouldRegenerateDirtyLayersInLongEvent
		{
			get
			{
				for (int i = 0; i < layers.Count; i++)
				{
					if (layers[i].Dirty && layers[i] is WorldLayer_Terrain)
					{
						return true;
					}
				}
				return false;
			}
		}

		public WorldRenderer()
		{
			foreach (Type item in typeof(WorldLayer).AllLeafSubclasses())
			{
				layers.Add((WorldLayer)Activator.CreateInstance(item));
			}
		}

		public void SetAllLayersDirty()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				layers[i].SetDirty();
			}
		}

		public void SetDirty<T>() where T : WorldLayer
		{
			for (int i = 0; i < layers.Count; i++)
			{
				if (layers[i] is T)
				{
					layers[i].SetDirty();
				}
			}
		}

		public void RegenerateAllLayersNow()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				layers[i].RegenerateNow();
			}
		}

		private IEnumerable RegenerateDirtyLayersNow_Async()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				if (layers[i].Dirty)
				{
					IEnumerator enumerator = layers[i].Regenerate().GetEnumerator();
					try
					{
						while (true)
						{
							try
							{
								if (!enumerator.MoveNext())
								{
									goto IL_00ca;
								}
							}
							catch (Exception arg)
							{
								Log.Error("Could not regenerate WorldLayer: " + arg);
								goto IL_00ca;
							}
							yield return enumerator.Current;
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				continue;
				IL_00ca:
				yield return null;
			}
			asynchronousRegenerationActive = false;
		}

		public void Notify_StaticWorldObjectPosChanged()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				(layers[i] as WorldLayer_WorldObjects)?.SetDirty();
			}
		}

		public void CheckActivateWorldCamera()
		{
			Find.WorldCamera.gameObject.SetActive(WorldRendererUtility.WorldRenderedNow);
		}

		public void DrawWorldLayers()
		{
			if (asynchronousRegenerationActive)
			{
				Log.Error("Called DrawWorldLayers() but already regenerating. This shouldn't ever happen because LongEventHandler should have stopped us.");
				return;
			}
			if (ShouldRegenerateDirtyLayersInLongEvent)
			{
				asynchronousRegenerationActive = true;
				LongEventHandler.QueueLongEvent(RegenerateDirtyLayersNow_Async(), "GeneratingPlanet", null, showExtraUIInfo: false);
				return;
			}
			WorldRendererUtility.UpdateWorldShadersParams();
			for (int i = 0; i < layers.Count; i++)
			{
				try
				{
					layers[i].Render();
				}
				catch (Exception arg)
				{
					Log.Error("Error drawing WorldLayer: " + arg);
				}
			}
		}

		public int GetTileIDFromRayHit(RaycastHit hit)
		{
			int i = 0;
			for (int count = layers.Count; i < count; i++)
			{
				WorldLayer_Terrain worldLayer_Terrain = layers[i] as WorldLayer_Terrain;
				if (worldLayer_Terrain != null)
				{
					return worldLayer_Terrain.GetTileIDFromRayHit(hit);
				}
			}
			return -1;
		}
	}
}
