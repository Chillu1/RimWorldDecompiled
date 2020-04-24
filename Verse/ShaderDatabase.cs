using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ShaderDatabase
	{
		public static readonly Shader Cutout = LoadShader("Map/Cutout");

		public static readonly Shader CutoutPlant = LoadShader("Map/CutoutPlant");

		public static readonly Shader CutoutComplex = LoadShader("Map/CutoutComplex");

		public static readonly Shader CutoutSkin = LoadShader("Map/CutoutSkin");

		public static readonly Shader CutoutFlying = LoadShader("Map/CutoutFlying");

		public static readonly Shader Transparent = LoadShader("Map/Transparent");

		public static readonly Shader TransparentPostLight = LoadShader("Map/TransparentPostLight");

		public static readonly Shader TransparentPlant = LoadShader("Map/TransparentPlant");

		public static readonly Shader Mote = LoadShader("Map/Mote");

		public static readonly Shader MoteGlow = LoadShader("Map/MoteGlow");

		public static readonly Shader MoteGlowPulse = LoadShader("Map/MoteGlowPulse");

		public static readonly Shader MoteWater = LoadShader("Map/MoteWater");

		public static readonly Shader MoteGlowDistorted = LoadShader("Map/MoteGlowDistorted");

		public static readonly Shader MoteGlowDistortBG = LoadShader("Map/MoteGlowDistortBackground");

		public static readonly Shader MoteProximityScannerRadius = LoadShader("Map/MoteProximityScannerRadius");

		public static readonly Shader TerrainHard = LoadShader("Map/TerrainHard");

		public static readonly Shader TerrainFade = LoadShader("Map/TerrainFade");

		public static readonly Shader TerrainFadeRough = LoadShader("Map/TerrainFadeRough");

		public static readonly Shader TerrainWater = LoadShader("Map/TerrainWater");

		public static readonly Shader WorldTerrain = LoadShader("World/WorldTerrain");

		public static readonly Shader WorldOcean = LoadShader("World/WorldOcean");

		public static readonly Shader WorldOverlayCutout = LoadShader("World/WorldOverlayCutout");

		public static readonly Shader WorldOverlayTransparent = LoadShader("World/WorldOverlayTransparent");

		public static readonly Shader WorldOverlayTransparentLit = LoadShader("World/WorldOverlayTransparentLit");

		public static readonly Shader WorldOverlayAdditive = LoadShader("World/WorldOverlayAdditive");

		public static readonly Shader MetaOverlay = LoadShader("Map/MetaOverlay");

		public static readonly Shader MetaOverlayDesaturated = LoadShader("Map/MetaOverlayDesaturated");

		public static readonly Shader SolidColor = LoadShader("Map/SolidColor");

		public static readonly Shader VertexColor = LoadShader("Map/VertexColor");

		public static readonly Shader Invisible = LoadShader("Misc/Invisible");

		private static Dictionary<string, Shader> lookup;

		public static Shader DefaultShader => Cutout;

		public static Shader LoadShader(string shaderPath)
		{
			if (lookup == null)
			{
				lookup = new Dictionary<string, Shader>();
			}
			if (!lookup.ContainsKey(shaderPath))
			{
				lookup[shaderPath] = (Shader)Resources.Load("Materials/" + shaderPath, typeof(Shader));
			}
			Shader shader = lookup[shaderPath];
			if (shader == null)
			{
				Log.Warning("Could not load shader " + shaderPath);
				return DefaultShader;
			}
			return shader;
		}
	}
}
