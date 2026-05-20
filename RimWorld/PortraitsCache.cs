using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class PortraitsCache
{
	private struct CachedPortrait
	{
		private const float CacheDuration = 1f;

		public RenderTexture RenderTexture { get; private set; }

		public bool Dirty { get; private set; }

		public float LastUseTime { get; private set; }

		public bool Expired => Time.time - LastUseTime > 1f;

		public CachedPortrait(RenderTexture renderTexture, bool dirty, float lastUseTime)
		{
			this = default(CachedPortrait);
			RenderTexture = renderTexture;
			Dirty = dirty;
			LastUseTime = lastUseTime;
		}
	}

	private class PortraitParamsEqualityComparer : IEqualityComparer<PortraitParams>
	{
		public bool Equals(PortraitParams x, PortraitParams y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(PortraitParams obj)
		{
			return obj.GetHashCode();
		}
	}

	private readonly struct PortraitParams : IEquatable<PortraitParams>
	{
		public readonly Vector2 size;

		public readonly Vector3 cameraOffset;

		public readonly float cameraZoom;

		public readonly Rot4 rotation;

		public readonly bool renderHeadgear;

		public readonly bool renderClothes;

		public readonly bool stylingStation;

		public readonly IReadOnlyDictionary<Apparel, Color> overrideApparelColors;

		public readonly Color? overrideHairColor;

		public readonly PawnHealthState? overrideHealthState;

		private readonly int cachedHashCode;

		public PortraitParams(Vector2 size, Vector3 cameraOffset, float cameraZoom, Rot4 rotation, bool renderHeadgear = true, bool renderClothes = true, IReadOnlyDictionary<Apparel, Color> overrideApparelColors = null, Color? overrideHairColor = null, bool stylingStation = false, PawnHealthState? overrideHealthState = null)
		{
			this.size = size;
			this.cameraOffset = cameraOffset;
			this.cameraZoom = cameraZoom;
			this.rotation = rotation;
			this.renderHeadgear = renderHeadgear;
			this.renderClothes = renderClothes;
			this.overrideApparelColors = overrideApparelColors;
			this.overrideHairColor = overrideHairColor;
			this.stylingStation = stylingStation;
			this.overrideHealthState = overrideHealthState;
			cachedHashCode = size.GetHashCode() ^ cameraOffset.GetHashCode() ^ cameraZoom.GetHashCode() ^ rotation.GetHashCode() ^ renderHeadgear.GetHashCode() ^ renderClothes.GetHashCode() ^ stylingStation.GetHashCode() ^ (overrideHairColor?.GetHashCode() ?? 0) ^ (overrideHealthState?.GetHashCode() ?? 0) ^ GenCollection.DictHashCode(overrideApparelColors);
		}

		public override bool Equals(object obj)
		{
			if (obj is PortraitParams other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(PortraitParams other)
		{
			if (other.size == size && other.cameraOffset == cameraOffset && other.cameraZoom == cameraZoom && other.rotation == rotation && other.renderHeadgear == renderHeadgear && other.renderClothes == renderClothes && other.stylingStation == stylingStation)
			{
				Color? color = other.overrideHairColor;
				Color? color2 = overrideHairColor;
				if (color.HasValue == color2.HasValue && (!color.HasValue || color.GetValueOrDefault() == color2.GetValueOrDefault()) && other.overrideHealthState == overrideHealthState)
				{
					return GenCollection.DictsEqual(other.overrideApparelColors, overrideApparelColors);
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return cachedHashCode;
		}

		public void RenderPortrait(Pawn pawn, RenderTexture renderTexture)
		{
			float angle = 0f;
			Vector3 positionOffset = default(Vector3);
			if ((overrideHealthState ?? pawn.health.State) != PawnHealthState.Mobile && !pawn.ageTracker.CurLifeStage.alwaysDowned)
			{
				angle = 85f;
				positionOffset.x -= 0.18f;
				positionOffset.z -= 0.18f;
			}
			Find.PawnCacheRenderer.RenderPawn(pawn, renderTexture, cameraOffset, cameraZoom, angle, rotation, pawn.health.hediffSet.HasHead, renderHeadgear, renderClothes, portrait: true, positionOffset, overrideApparelColors, overrideHairColor, stylingStation);
		}
	}

	private static List<RenderTexture> renderTexturesPool = new List<RenderTexture>();

	private static Dictionary<PortraitParams, Dictionary<Pawn, CachedPortrait>> cachedPortraits = new Dictionary<PortraitParams, Dictionary<Pawn, CachedPortrait>>(new PortraitParamsEqualityComparer());

	private const float SupersampleScale = 1.25f;

	private static List<Pawn> toRemove = new List<Pawn>();

	private static List<Pawn> toSetDirty = new List<Pawn>();

	public static RenderTexture Get(Pawn pawn, Vector2 size, Rot4 rotation, Vector3 cameraOffset = default(Vector3), float cameraZoom = 1f, bool supersample = true, bool compensateForUIScale = true, bool renderHeadgear = true, bool renderClothes = true, IReadOnlyDictionary<Apparel, Color> overrideApparelColors = null, Color? overrideHairColor = null, bool stylingStation = false, PawnHealthState? healthStateOverride = null)
	{
		if (supersample)
		{
			size *= 1.25f;
		}
		if (compensateForUIScale)
		{
			size *= Prefs.UIScale;
		}
		PortraitParams portraitParams = new PortraitParams(size, cameraOffset, cameraZoom, rotation, renderHeadgear, renderClothes, overrideApparelColors, overrideHairColor, stylingStation, healthStateOverride);
		Dictionary<Pawn, CachedPortrait> orCreateCachedPortraitsWithParams = GetOrCreateCachedPortraitsWithParams(portraitParams);
		if (orCreateCachedPortraitsWithParams.TryGetValue(pawn, out var value))
		{
			if (!value.RenderTexture.IsCreated())
			{
				value.RenderTexture.Create();
				portraitParams.RenderPortrait(pawn, value.RenderTexture);
			}
			else if (value.Dirty)
			{
				portraitParams.RenderPortrait(pawn, value.RenderTexture);
			}
			orCreateCachedPortraitsWithParams.Remove(pawn);
			orCreateCachedPortraitsWithParams.Add(pawn, new CachedPortrait(value.RenderTexture, dirty: false, Time.time));
			return value.RenderTexture;
		}
		RenderTexture renderTexture = NewRenderTexture(size);
		portraitParams.RenderPortrait(pawn, renderTexture);
		orCreateCachedPortraitsWithParams.Add(pawn, new CachedPortrait(renderTexture, dirty: false, Time.time));
		return renderTexture;
	}

	public static void SetDirty(Pawn pawn)
	{
		foreach (var (_, dictionary2) in cachedPortraits)
		{
			if (dictionary2.TryGetValue(pawn, out var value) && !value.Dirty)
			{
				dictionary2.Remove(pawn);
				dictionary2.Add(pawn, new CachedPortrait(value.RenderTexture, dirty: true, value.LastUseTime));
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
		foreach (KeyValuePair<PortraitParams, Dictionary<Pawn, CachedPortrait>> cachedPortrait in cachedPortraits)
		{
			cachedPortrait.Deconstruct(out var _, out var value);
			foreach (KeyValuePair<Pawn, CachedPortrait> item in value)
			{
				DestroyRenderTexture(item.Value.RenderTexture);
			}
		}
		cachedPortraits.Clear();
		for (int i = 0; i < renderTexturesPool.Count; i++)
		{
			DestroyRenderTexture(renderTexturesPool[i]);
		}
		renderTexturesPool.Clear();
	}

	private static Dictionary<Pawn, CachedPortrait> GetOrCreateCachedPortraitsWithParams(PortraitParams portraitParams)
	{
		if (!cachedPortraits.TryGetValue(portraitParams, out var value))
		{
			value = new Dictionary<Pawn, CachedPortrait>(new PawnEqualityComparer());
			cachedPortraits.Add(portraitParams, value);
		}
		return value;
	}

	private static void DestroyRenderTexture(RenderTexture rt)
	{
		rt.DiscardContents();
		UnityEngine.Object.Destroy(rt);
	}

	private static void RemoveExpiredCachedPortraits()
	{
		foreach (var (_, dictionary2) in cachedPortraits)
		{
			toRemove.Clear();
			foreach (KeyValuePair<Pawn, CachedPortrait> item in dictionary2)
			{
				if (item.Value.Expired)
				{
					toRemove.Add(item.Key);
					renderTexturesPool.Add(item.Value.RenderTexture);
				}
			}
			for (int i = 0; i < toRemove.Count; i++)
			{
				dictionary2.Remove(toRemove[i]);
			}
			toRemove.Clear();
		}
	}

	private static void SetAnimatedPortraitsDirty()
	{
		foreach (var (_, dictionary2) in cachedPortraits)
		{
			toSetDirty.Clear();
			foreach (KeyValuePair<Pawn, CachedPortrait> item in dictionary2)
			{
				if (IsAnimated(item.Key) && !item.Value.Dirty)
				{
					toSetDirty.Add(item.Key);
				}
			}
			for (int i = 0; i < toSetDirty.Count; i++)
			{
				CachedPortrait cachedPortrait = dictionary2[toSetDirty[i]];
				dictionary2.Remove(toSetDirty[i]);
				dictionary2.Add(toSetDirty[i], new CachedPortrait(cachedPortrait.RenderTexture, dirty: true, cachedPortrait.LastUseTime));
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
			name = "Portrait",
			useMipMap = false,
			filterMode = FilterMode.Bilinear
		};
	}

	private static bool IsAnimated(Pawn pawn)
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			return pawn.Drawer.renderer.flasher.FlashingNowOrRecently;
		}
		return false;
	}
}
