using System;
using System.Collections.Generic;
using RimWorld;
using Unity.Mathematics;
using UnityEngine;

namespace Verse;

public static class SilhouetteUtility
{
	private readonly struct SilhouetteCacheKey
	{
		private readonly ThingDef thingDef;

		private readonly LifeStageDef lifeStageDef;

		private readonly int graphicIndex;

		private readonly Gender gender;

		private readonly RotStage rotMode;

		public SilhouetteCacheKey(Pawn pawn)
		{
			thingDef = pawn.def;
			lifeStageDef = pawn.ageTracker.CurLifeStage;
			graphicIndex = pawn.GetGraphicIndex();
			gender = pawn.gender;
			rotMode = pawn.mutant?.rotStage ?? RotStage.Fresh;
		}

		public SilhouetteCacheKey(Thing thing)
		{
			thingDef = thing.def;
			lifeStageDef = null;
			graphicIndex = thing.OverrideGraphicIndex ?? (-1);
			gender = Gender.None;
			rotMode = RotStage.Fresh;
		}

		public override int GetHashCode()
		{
			int seed = thingDef.GetHashCode();
			if (lifeStageDef != null)
			{
				seed = Gen.HashCombineInt(seed, lifeStageDef.GetHashCode());
			}
			seed = Gen.HashCombineInt(seed, graphicIndex);
			seed = Gen.HashCombineInt(seed, gender.GetHashCode());
			return Gen.HashCombineInt(seed, rotMode.GetHashCode());
		}
	}

	private class SilhouetteCacheValue : IDisposable
	{
		public readonly Material east;

		public readonly Material west;

		public SilhouetteCacheValue(Material east, Material west)
		{
			this.east = east;
			this.west = west;
		}

		public void Dispose()
		{
			UnityEngine.Object.Destroy(east);
			UnityEngine.Object.Destroy(west);
		}
	}

	private static readonly Dictionary<SilhouetteCacheKey, SilhouetteCacheValue> materialCache = new Dictionary<SilhouetteCacheKey, SilhouetteCacheValue>(512);

	private static readonly Dictionary<float3, MaterialPropertyBlock> materialPropertyBlockCache = new Dictionary<float3, MaterialPropertyBlock>();

	private static int lastCachedAlphaFrame = -1;

	private static float lastCachedAlpha = 0f;

	private const float DotHighlightSizeRatio = 0.01f;

	private const float DotAlpha = 0.75f;

	private const float SilhouetteAlpha = 0.9f;

	private static readonly Color ThingColor = new Color(0.56f, 0.62f, 0.9f);

	private static readonly Color UncontrolledMechDotColor = new Color(0.8f, 0.55f, 0.17f, 0.75f);

	public const float DotHighlightStartRange = 0.9f;

	private const int MaximumMaterialCache = 512;

	public static void DrawGraphicSilhouette(Thing thing, Vector3 pos)
	{
		if (ShouldDrawSilhouette(thing))
		{
			DrawSilhouette(thing, pos);
		}
	}

	public static void DrawGUISilhouette(Thing thing)
	{
		if (thing is Pawn thing2 && ShouldDrawPawnDotSilhouette(thing2))
		{
			DrawPawnDotSilhouette(thing2);
		}
	}

	private static SilhouetteCacheKey GetSilhouetteCacheKey(Thing thing)
	{
		if (thing is Pawn pawn)
		{
			return new SilhouetteCacheKey(pawn);
		}
		return new SilhouetteCacheKey(thing);
	}

	public static void NotifyGraphicDirty(Thing thing)
	{
		SilhouetteCacheKey silhouetteCacheKey = GetSilhouetteCacheKey(thing);
		if (materialCache.ContainsKey(silhouetteCacheKey))
		{
			materialCache.Remove(silhouetteCacheKey);
		}
	}

	private static void DrawPawnDotSilhouette(Thing thing)
	{
		Color color = GetColor(thing);
		color.a = GetAlpha();
		GUI.DrawTexture(GetAdjustedScreenspaceRect(thing), TexUI.DotHighlight, ScaleMode.ScaleToFit, alphaBlend: true, 0f, color, 0f, 0f);
	}

	public static void DrawSilhouetteJob(Thing thing, Matrix4x4 trs)
	{
		(Mesh mesh, Material material) cachedSilhouetteData = GetCachedSilhouetteData(thing);
		Mesh item = cachedSilhouetteData.mesh;
		Material item2 = cachedSilhouetteData.material;
		MaterialPropertyBlock cachedMaterialPropertyBlock = GetCachedMaterialPropertyBlock(GetColor(thing));
		GenDraw.DrawMeshNowOrLater(item, trs, item2, drawNow: false, cachedMaterialPropertyBlock);
	}

	private static void DrawSilhouette(Thing thing, Vector3 pos)
	{
		(Mesh mesh, Material material) cachedSilhouetteData = GetCachedSilhouetteData(thing);
		Mesh item = cachedSilhouetteData.mesh;
		Material item2 = cachedSilhouetteData.material;
		Graphic graphic = ((thing is Pawn pawn) ? pawn.Drawer.renderer.SilhouetteGraphic : thing.Graphic);
		MaterialPropertyBlock cachedMaterialPropertyBlock = GetCachedMaterialPropertyBlock(GetColor(thing));
		Vector3 vector = (thing.def.rotatable ? new Vector3(graphic.drawSize.y, 0f, graphic.drawSize.x) : new Vector3(graphic.drawSize.x, 0f, graphic.drawSize.y));
		Vector3 inverseFovScale = Find.CameraDriver.InverseFovScale;
		if (vector.x < 2.5f)
		{
			inverseFovScale.x *= vector.x + AdjustScale(vector.x);
		}
		else
		{
			inverseFovScale.x *= vector.x;
		}
		if (vector.z < 2.5f)
		{
			inverseFovScale.z *= vector.z + AdjustScale(vector.z);
		}
		else
		{
			inverseFovScale.z *= vector.z;
		}
		Matrix4x4 matrix = Matrix4x4.TRS(pos.SetToAltitude(AltitudeLayer.Silhouettes), Quaternion.AngleAxis(0f, Vector3.up), inverseFovScale);
		GenDraw.DrawMeshNowOrLater(item, matrix, item2, drawNow: false, cachedMaterialPropertyBlock);
	}

	public static bool ShouldDrawSilhouette(Thing thing)
	{
		if (WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			return false;
		}
		if (Find.ScreenshotModeHandler.Active)
		{
			return false;
		}
		if (Prefs.HighlightStyleMode != HighlightStyleMode.Silhouettes)
		{
			return false;
		}
		if (!thing.def.drawHighlight)
		{
			return false;
		}
		if (!ShouldHighlight(thing))
		{
			return false;
		}
		return true;
	}

	private static bool ShouldDrawPawnDotSilhouette(Thing thing)
	{
		if (Prefs.HighlightStyleMode == HighlightStyleMode.Dots && thing.def.drawHighlight)
		{
			return ShouldHighlight(thing);
		}
		return false;
	}

	public static bool CanHighlightAny()
	{
		if (Prefs.DotHighlightDisplayMode == DotHighlightDisplayMode.None)
		{
			return false;
		}
		CameraDriver cameraDriver = Find.CameraDriver;
		if (cameraDriver.ZoomRootSize < cameraDriver.config.sizeRange.max * 0.9f)
		{
			return false;
		}
		return true;
	}

	private static bool ShouldHighlight(Thing thing)
	{
		if (!CanHighlightAny())
		{
			return false;
		}
		if (!thing.Spawned)
		{
			return false;
		}
		if (thing.shouldHighlightCachedTick == GenTicks.TicksGame)
		{
			return thing.shouldHighlightCached;
		}
		thing.shouldHighlightCachedTick = GenTicks.TicksGame;
		return thing.shouldHighlightCached = ShouldHighlightInt(thing);
	}

	private static bool ShouldHighlightInt(Thing thing)
	{
		if (thing is Pawn pawn)
		{
			if (!pawn.IsPlayerControlled && pawn.Fogged())
			{
				return false;
			}
			if (Prefs.DotHighlightDisplayMode == DotHighlightDisplayMode.HighlightHostiles && !pawn.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (pawn.IsHiddenFromPlayer())
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && pawn.Map.gameConditionManager.MapBrightness < 0.1f && thing.Map.glowGrid.GroundGlowAt(thing.Position) <= 0f && pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
		}
		else
		{
			if (thing.Fogged())
			{
				return false;
			}
			if (thing.def.drawHighlightOnlyForHostile && !thing.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
		}
		return true;
	}

	private static MaterialPropertyBlock GetCachedMaterialPropertyBlock(Color color)
	{
		float3 key = new float3(color.r, color.g, color.b);
		color.a = GetAlpha();
		if (materialPropertyBlockCache.TryGetValue(key, out var value))
		{
			value.SetColor(ShaderPropertyIDs.Color, color);
		}
		else
		{
			value = new MaterialPropertyBlock();
			value.SetColor(ShaderPropertyIDs.Color, color);
			materialPropertyBlockCache[key] = value;
		}
		return value;
	}

	private static Color GetColor(Thing thing)
	{
		if (thing.highlightColorCachedTick == GenTicks.TicksGame)
		{
			return thing.highlightColorCached;
		}
		thing.highlightColorCachedTick = GenTicks.TicksGame;
		return thing.highlightColorCached = GetColorInt(thing);
	}

	private static Color GetColorInt(Thing thing)
	{
		if (thing.def.highlightColor.HasValue)
		{
			return thing.def.highlightColor.Value;
		}
		if (thing is Pawn pawn)
		{
			if (pawn.IsColonyMech && !pawn.IsColonyMechPlayerControlled)
			{
				return UncontrolledMechDotColor;
			}
			return PawnNameColorUtility.PawnNameColorOf(pawn);
		}
		if (thing.HostileTo(Faction.OfPlayer))
		{
			return PawnNameColorUtility.ColorBaseHostile;
		}
		return ThingColor;
	}

	private static float GetAlpha()
	{
		if (lastCachedAlphaFrame == RealTime.frameCount)
		{
			return lastCachedAlpha;
		}
		lastCachedAlphaFrame = RealTime.frameCount;
		bool num = Prefs.HighlightStyleMode == HighlightStyleMode.Silhouettes;
		CameraDriver cameraDriver = Find.CameraDriver;
		float num2 = Mathf.Clamp01(Mathf.InverseLerp(cameraDriver.config.sizeRange.max * 0.84999996f, cameraDriver.config.sizeRange.max, cameraDriver.ZoomRootSize));
		if (num)
		{
			return lastCachedAlpha = 0.9f * num2;
		}
		return lastCachedAlpha = 0.75f * num2;
	}

	private static (Mesh mesh, Material material) GetCachedSilhouetteData(Thing thing)
	{
		if (materialCache.Count > 512)
		{
			materialCache.Clear();
		}
		SilhouetteCacheKey silhouetteCacheKey = GetSilhouetteCacheKey(thing);
		if (!materialCache.ContainsKey(silhouetteCacheKey))
		{
			Graphic coloredVersion = ((thing is Pawn pawn) ? pawn.Drawer.renderer.SilhouetteGraphic : thing.Graphic).GetColoredVersion(ShaderDatabase.Silhouette, Color.white, Color.white);
			materialCache[silhouetteCacheKey] = new SilhouetteCacheValue(coloredVersion.MatEast, coloredVersion.MatWest);
		}
		Material item;
		Mesh item2;
		if (thing.Rotation == Rot4.West)
		{
			item = materialCache[silhouetteCacheKey].west;
			item2 = MeshPool.GridPlaneFlip(Vector2.one);
		}
		else
		{
			item = materialCache[silhouetteCacheKey].east;
			item2 = MeshPool.GridPlane(Vector2.one);
		}
		return (mesh: item2, material: item);
	}

	public static float AdjustScale(float scale)
	{
		return Mathf.InverseLerp(2.5f, 0f, scale) * 0.75f;
	}

	public static Rect GetAdjustedScreenspaceRect(Thing thing, float screenSizeRatio = 0.01f)
	{
		Vector2 vector = (Vector2)Find.Camera.WorldToScreenPoint(thing.DrawPos) / Prefs.UIScale;
		vector.y = (float)UI.screenHeight - vector.y;
		float num = screenSizeRatio * (float)Mathf.Min(Screen.width, Screen.height) / Prefs.UIScale;
		return new Rect(vector.x - num, vector.y - num / 2f, num * 2f, num);
	}
}
