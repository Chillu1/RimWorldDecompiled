using System.Collections.Generic;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class ShaderDatabase
{
	public static readonly Shader Cutout = LoadShader("Map/Cutout");

	public static readonly Shader CutoutHair = LoadShader("Map/CutoutHair");

	public static readonly Shader CutoutPlant = LoadShader("Map/CutoutPlant");

	public static readonly Shader CutoutComplex = LoadShader("Map/CutoutComplex");

	public static readonly Shader CutoutComplexUI = LoadShader("Map/CutoutComplexUI");

	public static readonly Shader ExpandingIconUI = LoadShader("Map/ExpandingIconUI");

	public static readonly Shader CutoutComplexBlend = LoadShader("Map/CutoutComplexBlend");

	public static readonly Shader CutoutSkinOverlay = LoadShader("Map/CutoutSkinOverlay");

	public static readonly Shader CutoutSkin = LoadShader("Map/CutoutSkin");

	public static readonly Shader Wound = LoadShader("Map/Wound");

	public static readonly Shader WoundSkin = LoadShader("Map/WoundSkin");

	public static readonly Shader CutoutSkinColorOverride = LoadShader("Map/CutoutSkinOverride");

	public static readonly Shader CutoutFlying = LoadShader("Map/CutoutFlying");

	public static readonly Shader CutoutFlying01 = LoadShader("Map/CutoutFlying01");

	public static readonly Shader FirefoamOverlay = LoadShader("Map/FirefoamOverlay");

	public static readonly Shader CutoutWithOverlay = LoadShader("Map/CutoutWithOverlay");

	public static readonly Shader CutoutOverlay = LoadShader("Map/CutoutOverlay");

	public static readonly Shader TerrainEdge = LoadShader("Map/TerrainEdge");

	public static readonly Shader Transparent = LoadShader("Map/Transparent");

	public static readonly Shader TransparentPostLight = LoadShader("Map/TransparentPostLight");

	public static readonly Shader TransparentPlant = LoadShader("Map/TransparentPlant");

	public static readonly Shader Mote = LoadShader("Map/Mote");

	public static readonly Shader MotePulse = LoadShader("Map/MotePulse");

	public static readonly Shader MoteGlow = LoadShader("Map/MoteGlow");

	public static readonly Shader MoteGlowPulse = LoadShader("Map/MoteGlowPulse");

	public static readonly Shader MoteWater = LoadShader("Map/MoteWater");

	public static readonly Shader MoteGlowDistorted = LoadShader("Map/MoteGlowDistorted");

	public static readonly Shader MoteGlowDistortBG = LoadShader("Map/MoteGlowDistortBackground");

	public static readonly Shader MoteProximityScannerRadius = LoadShader("Map/MoteProximityScannerRadius");

	public static readonly Shader GasRotating = LoadShader("Map/GasRotating");

	public static readonly Shader TerrainHard = LoadShader("Map/TerrainHard");

	public static readonly Shader TerrainFade = LoadShader("Map/TerrainFade");

	public static readonly Shader TerrainFadeRough = LoadShader("Map/TerrainFadeRough");

	public static readonly Shader TerrainWater = LoadShader("Map/TerrainWater");

	public static readonly Shader TerrainHardPolluted = LoadShader("Map/TerrainHardLinearBurn");

	public static readonly Shader TerrainFadePolluted = LoadShader("Map/TerrainFadeLinearBurn");

	public static readonly Shader TerrainFadeRoughPolluted = LoadShader("Map/TerrainFadeRoughLinearBurn");

	public static readonly Shader PollutionCloud = LoadShader("Map/PollutionCloud");

	public static readonly Shader MapEdgeTerrain = LoadShader("Map/MapEdgeTerrain");

	public static readonly Shader WorldTerrain = LoadShader("World/WorldTerrain");

	public static readonly Shader WorldOcean = LoadShader("World/WorldOcean");

	public static readonly Shader WorldOverlayCutout = LoadShader("World/WorldOverlayCutout");

	public static readonly Shader WorldOverlayTransparent = LoadShader("World/WorldOverlayTransparent");

	public static readonly Shader WorldOverlayTransparentLit = LoadShader("World/WorldOverlayTransparentLit");

	public static readonly Shader WorldOverlayTransparentLitPollution = LoadShader("World/WorldOverlayTransparentLitPollution");

	public static readonly Shader WorldOverlayAdditive = LoadShader("World/WorldOverlayAdditive");

	public static readonly Shader WorldOverlayAdditiveTwoSided = LoadShader("World/WorldOverlayAdditiveTwoSided");

	public static readonly Shader PlanetGlow = LoadShader("World/PlanetGlow");

	public static readonly Shader Clouds = LoadShader("World/Clouds");

	public static readonly Shader MetaOverlay = LoadShader("Map/MetaOverlay");

	public static readonly Shader MetaOverlayDesaturated = LoadShader("Map/MetaOverlayDesaturated");

	public static readonly Shader SolidColor = LoadShader("Map/SolidColor");

	public static readonly Shader SolidColorBehind = LoadShader("Map/SolidColorBehind");

	public static readonly Shader VertexColor = LoadShader("Map/VertexColor");

	public static readonly Shader RitualStencil = LoadShader("Map/RitualStencil");

	public static readonly Shader Invisible = LoadShader("Misc/Invisible");

	public static readonly Shader Silhouette = LoadShader("Misc/Silhouette");

	public static readonly Shader Metalblood = LoadShader("Misc/Metalblood");

	public static readonly Shader CaveExitRope = LoadShader("Map/CaveExitRope");

	public static readonly Shader BioferriteHarvester = LoadShader("Map/BioferriteHarvester");

	public static readonly Shader GrayscaleGUI = LoadShader("Misc/GrayscaleGUI");

	public static readonly Shader GravshipMaskMasked = LoadShader("Map/GravshipMaskMasked");

	public static readonly Shader IndoorMaskMasked = LoadShader("Map/IndoorMaskMasked");

	private static Dictionary<string, Shader> lookup;

	private static Dictionary<Shader, Shader> uiLookup;

	public static Shader DefaultShader => Cutout;

	public static Shader LoadShader(string shaderPath)
	{
		if (lookup == null)
		{
			lookup = new Dictionary<string, Shader>();
		}
		if (uiLookup == null)
		{
			uiLookup = new Dictionary<Shader, Shader>();
		}
		TryLoadShader(shaderPath, out var result);
		return result;
	}

	public static Shader LoadShader(ShaderTypeDef shaderDef)
	{
		LoadShader(shaderDef, out var result, out var _);
		return result;
	}

	public static void LoadShader(ShaderTypeDef shaderDef, out Shader result, out Shader uiShader)
	{
		if (shaderDef == null)
		{
			Log.Warning("Tried to LoadShader with a null ShaderTypeDef. Returning default shader instead.");
			result = DefaultShader;
			uiShader = null;
		}
		else
		{
			LoadShader(shaderDef.shaderPath, shaderDef.uiShaderPath, out result, out uiShader);
		}
	}

	public static void LoadShader(string shaderPath, string uiShaderPath, out Shader result, out Shader uiShader)
	{
		uiShader = null;
		if (!string.IsNullOrEmpty(uiShaderPath))
		{
			TryLoadShader(uiShaderPath, out uiShader);
		}
		if (TryLoadShader(shaderPath, out result) && uiShader != null)
		{
			uiLookup[result] = uiShader;
		}
	}

	public static bool TryGetUIShader(Shader shader, out Shader uiShader)
	{
		return uiLookup.TryGetValue(shader, out uiShader);
	}

	private static bool TryLoadShader(string shaderPath, out Shader result)
	{
		if (!lookup.ContainsKey(shaderPath))
		{
			if (Resources.Load("Materials/" + shaderPath, typeof(Shader)) is Shader value)
			{
				lookup[shaderPath] = value;
			}
			else
			{
				Shader shader = ContentFinder<Shader>.TryFindAssetInModBundles(shaderPath);
				if ((object)shader != null)
				{
					lookup[shaderPath] = shader;
				}
				else
				{
					lookup[shaderPath] = null;
				}
			}
		}
		result = lookup[shaderPath];
		if (result == null)
		{
			Log.Warning("Could not load shader " + shaderPath + " in resources or mod bundles. Using default shader instead.");
			result = DefaultShader;
			return false;
		}
		return true;
	}
}
