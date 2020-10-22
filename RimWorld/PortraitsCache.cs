using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PortraitsCache
	{
		private struct CachedPortrait
		{
			private const float CacheDuration = 1f;

			public RenderTexture RenderTexture
			{
				get;
				private set;
			}

			public bool Dirty
			{
				get;
				private set;
			}

			public float LastUseTime
			{
				get;
				private set;
			}

			public bool Expired => Time.time - LastUseTime > 1f;

			public CachedPortrait(RenderTexture renderTexture, bool dirty, float lastUseTime)
			{
				this = default(CachedPortrait);
				RenderTexture = renderTexture;
				Dirty = dirty;
				LastUseTime = lastUseTime;
			}
		}

		private struct CachedPortraitsWithParams
		{
			public Dictionary<Pawn, CachedPortrait> CachedPortraits
			{
				get;
				private set;
			}

			public Vector2 Size
			{
				get;
				private set;
			}

			public Vector3 CameraOffset
			{
				get;
				private set;
			}

			public float CameraZoom
			{
				get;
				private set;
			}

			public CachedPortraitsWithParams(Vector2 size, Vector3 cameraOffset, float cameraZoom)
			{
				this = default(CachedPortraitsWithParams);
				CachedPortraits = new Dictionary<Pawn, CachedPortrait>();
				Size = size;
				CameraOffset = cameraOffset;
				CameraZoom = cameraZoom;
			}
		}

		private static List<RenderTexture> renderTexturesPool = new List<RenderTexture>();

		private static List<CachedPortraitsWithParams> cachedPortraits = new List<CachedPortraitsWithParams>();

		private const float SupersampleScale = 1.25f;

		private static List<Pawn> toRemove = new List<Pawn>();

		private static List<Pawn> toSetDirty = new List<Pawn>();

		public static RenderTexture Get(Pawn pawn, Vector2 size, Vector3 cameraOffset = default(Vector3), float cameraZoom = 1f, bool supersample = true, bool compensateForUIScale = true)
		{
			if (supersample)
			{
				size *= 1.25f;
			}
			if (compensateForUIScale)
			{
				size *= Prefs.UIScale;
			}
			Dictionary<Pawn, CachedPortrait> dictionary = GetOrCreateCachedPortraitsWithParams(size, cameraOffset, cameraZoom).CachedPortraits;
			if (dictionary.TryGetValue(pawn, out var value))
			{
				if (!value.RenderTexture.IsCreated())
				{
					value.RenderTexture.Create();
					RenderPortrait(pawn, value.RenderTexture, cameraOffset, cameraZoom);
				}
				else if (value.Dirty)
				{
					RenderPortrait(pawn, value.RenderTexture, cameraOffset, cameraZoom);
				}
				dictionary.Remove(pawn);
				dictionary.Add(pawn, new CachedPortrait(value.RenderTexture, dirty: false, Time.time));
				return value.RenderTexture;
			}
			RenderTexture renderTexture = NewRenderTexture(size);
			RenderPortrait(pawn, renderTexture, cameraOffset, cameraZoom);
			dictionary.Add(pawn, new CachedPortrait(renderTexture, dirty: false, Time.time));
			return renderTexture;
		}

		public static void SetDirty(Pawn pawn)
		{
			for (int i = 0; i < cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, CachedPortrait> dictionary = cachedPortraits[i].CachedPortraits;
				if (dictionary.TryGetValue(pawn, out var value) && !value.Dirty)
				{
					dictionary.Remove(pawn);
					dictionary.Add(pawn, new CachedPortrait(value.RenderTexture, dirty: true, value.LastUseTime));
				}
			}
		}

		public static void PortraitsCacheUpdate()
		{
			RemoveExpiredCachedPortraits();
			SetAnimatedPortraitsDirty();
		}

		public static void Clear()
		{
			for (int i = 0; i < cachedPortraits.Count; i++)
			{
				foreach (KeyValuePair<Pawn, CachedPortrait> cachedPortrait in cachedPortraits[i].CachedPortraits)
				{
					DestroyRenderTexture(cachedPortrait.Value.RenderTexture);
				}
			}
			cachedPortraits.Clear();
			for (int j = 0; j < renderTexturesPool.Count; j++)
			{
				DestroyRenderTexture(renderTexturesPool[j]);
			}
			renderTexturesPool.Clear();
		}

		private static CachedPortraitsWithParams GetOrCreateCachedPortraitsWithParams(Vector2 size, Vector3 cameraOffset, float cameraZoom)
		{
			for (int i = 0; i < cachedPortraits.Count; i++)
			{
				if (cachedPortraits[i].Size == size && cachedPortraits[i].CameraOffset == cameraOffset && cachedPortraits[i].CameraZoom == cameraZoom)
				{
					return cachedPortraits[i];
				}
			}
			CachedPortraitsWithParams cachedPortraitsWithParams = new CachedPortraitsWithParams(size, cameraOffset, cameraZoom);
			cachedPortraits.Add(cachedPortraitsWithParams);
			return cachedPortraitsWithParams;
		}

		private static void DestroyRenderTexture(RenderTexture rt)
		{
			rt.DiscardContents();
			Object.Destroy(rt);
		}

		private static void RemoveExpiredCachedPortraits()
		{
			for (int i = 0; i < cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, CachedPortrait> dictionary = cachedPortraits[i].CachedPortraits;
				toRemove.Clear();
				foreach (KeyValuePair<Pawn, CachedPortrait> item in dictionary)
				{
					if (item.Value.Expired)
					{
						toRemove.Add(item.Key);
						renderTexturesPool.Add(item.Value.RenderTexture);
					}
				}
				for (int j = 0; j < toRemove.Count; j++)
				{
					dictionary.Remove(toRemove[j]);
				}
				toRemove.Clear();
			}
		}

		private static void SetAnimatedPortraitsDirty()
		{
			for (int i = 0; i < cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, CachedPortrait> dictionary = cachedPortraits[i].CachedPortraits;
				toSetDirty.Clear();
				foreach (KeyValuePair<Pawn, CachedPortrait> item in dictionary)
				{
					if (IsAnimated(item.Key) && !item.Value.Dirty)
					{
						toSetDirty.Add(item.Key);
					}
				}
				for (int j = 0; j < toSetDirty.Count; j++)
				{
					CachedPortrait cachedPortrait = dictionary[toSetDirty[j]];
					dictionary.Remove(toSetDirty[j]);
					dictionary.Add(toSetDirty[j], new CachedPortrait(cachedPortrait.RenderTexture, dirty: true, cachedPortrait.LastUseTime));
				}
				toSetDirty.Clear();
			}
		}

		private static RenderTexture NewRenderTexture(Vector2 size)
		{
			int num = renderTexturesPool.FindLastIndex((RenderTexture x) => x.width == (int)size.x && x.height == (int)size.y);
			if (num != -1)
			{
				RenderTexture result = renderTexturesPool[num];
				renderTexturesPool.RemoveAt(num);
				return result;
			}
			return new RenderTexture((int)size.x, (int)size.y, 24)
			{
				filterMode = FilterMode.Bilinear
			};
		}

		private static void RenderPortrait(Pawn pawn, RenderTexture renderTexture, Vector3 cameraOffset, float cameraZoom)
		{
			Find.PortraitRenderer.RenderPortrait(pawn, renderTexture, cameraOffset, cameraZoom);
		}

		private static bool IsAnimated(Pawn pawn)
		{
			if (Current.ProgramState == ProgramState.Playing && pawn.Drawer.renderer.graphics.flasher.FlashingNowOrRecently)
			{
				return true;
			}
			return false;
		}
	}
}
