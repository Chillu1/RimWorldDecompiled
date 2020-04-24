using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class WorldMaterials
	{
		public static readonly Material WorldTerrain;

		public static readonly Material WorldIce;

		public static readonly Material WorldOcean;

		public static readonly Material UngeneratedPlanetParts;

		public static readonly Material Rivers;

		public static readonly Material RiversBorder;

		public static readonly Material Roads;

		public static int DebugTileRenderQueue;

		public static int WorldObjectRenderQueue;

		public static int WorldLineRenderQueue;

		public static int DynamicObjectRenderQueue;

		public static int FeatureNameRenderQueue;

		public static readonly Material MouseTile;

		public static readonly Material SelectedTile;

		public static readonly Material CurrentMapTile;

		public static readonly Material Stars;

		public static readonly Material Sun;

		public static readonly Material PlanetGlow;

		public static readonly Material SmallHills;

		public static readonly Material LargeHills;

		public static readonly Material Mountains;

		public static readonly Material ImpassableMountains;

		public static readonly Material VertexColor;

		private static readonly Material TargetSquareMatSingle;

		private static int NumMatsPerMode;

		public static Material OverlayModeMatOcean;

		private static Material[] matsFertility;

		private static readonly Color[] FertilitySpectrum;

		private const float TempRange = 50f;

		private static Material[] matsTemperature;

		private static readonly Color[] TemperatureSpectrum;

		private const float ElevationMax = 5000f;

		private static Material[] matsElevation;

		private static readonly Color[] ElevationSpectrum;

		private const float RainfallMax = 5000f;

		private static Material[] matsRainfall;

		private static readonly Color[] RainfallSpectrum;

		public static Material CurTargetingMat
		{
			get
			{
				TargetSquareMatSingle.color = GenDraw.CurTargetingColor;
				return TargetSquareMatSingle;
			}
		}

		static WorldMaterials()
		{
			WorldTerrain = MatLoader.LoadMat("World/WorldTerrain", 3500);
			WorldIce = MatLoader.LoadMat("World/WorldIce", 3500);
			WorldOcean = MatLoader.LoadMat("World/WorldOcean", 3500);
			UngeneratedPlanetParts = MatLoader.LoadMat("World/UngeneratedPlanetParts", 3500);
			Rivers = MatLoader.LoadMat("World/Rivers", 3530);
			RiversBorder = MatLoader.LoadMat("World/RiversBorder", 3520);
			Roads = MatLoader.LoadMat("World/Roads", 3540);
			DebugTileRenderQueue = 3510;
			WorldObjectRenderQueue = 3550;
			WorldLineRenderQueue = 3590;
			DynamicObjectRenderQueue = 3600;
			FeatureNameRenderQueue = 3610;
			MouseTile = MaterialPool.MatFrom("World/MouseTile", ShaderDatabase.WorldOverlayAdditive, 3560);
			SelectedTile = MaterialPool.MatFrom("World/SelectedTile", ShaderDatabase.WorldOverlayAdditive, 3560);
			CurrentMapTile = MaterialPool.MatFrom("World/CurrentMapTile", ShaderDatabase.WorldOverlayTransparent, 3560);
			Stars = MatLoader.LoadMat("World/Stars");
			Sun = MatLoader.LoadMat("World/Sun");
			PlanetGlow = MatLoader.LoadMat("World/PlanetGlow");
			SmallHills = MaterialPool.MatFrom("World/Hills/SmallHills", ShaderDatabase.WorldOverlayTransparentLit, 3510);
			LargeHills = MaterialPool.MatFrom("World/Hills/LargeHills", ShaderDatabase.WorldOverlayTransparentLit, 3510);
			Mountains = MaterialPool.MatFrom("World/Hills/Mountains", ShaderDatabase.WorldOverlayTransparentLit, 3510);
			ImpassableMountains = MaterialPool.MatFrom("World/Hills/Impassable", ShaderDatabase.WorldOverlayTransparentLit, 3510);
			VertexColor = MatLoader.LoadMat("World/WorldVertexColor");
			TargetSquareMatSingle = MaterialPool.MatFrom("UI/Overlays/TargetHighlight_Square", ShaderDatabase.Transparent, 3560);
			NumMatsPerMode = 50;
			OverlayModeMatOcean = SolidColorMaterials.NewSolidColorMaterial(new Color(0.09f, 0.18f, 0.2f), ShaderDatabase.Transparent);
			FertilitySpectrum = new Color[2]
			{
				new Color(0f, 1f, 0f, 0f),
				new Color(0f, 1f, 0f, 0.5f)
			};
			TemperatureSpectrum = new Color[8]
			{
				new Color(1f, 1f, 1f),
				new Color(0f, 0f, 1f),
				new Color(0.25f, 0.25f, 1f),
				new Color(0.6f, 0.6f, 1f),
				new Color(0.5f, 0.5f, 0.5f),
				new Color(0.5f, 0.3f, 0f),
				new Color(1f, 0.6f, 0.18f),
				new Color(1f, 0f, 0f)
			};
			ElevationSpectrum = new Color[4]
			{
				new Color(0.224f, 0.18f, 0.15f),
				new Color(0.447f, 0.369f, 0.298f),
				new Color(0.6f, 0.6f, 0.6f),
				new Color(1f, 1f, 1f)
			};
			RainfallSpectrum = new Color[12]
			{
				new Color(0.9f, 0.9f, 0.9f),
				GenColor.FromBytes(190, 190, 190),
				new Color(0.58f, 0.58f, 0.58f),
				GenColor.FromBytes(196, 112, 110),
				GenColor.FromBytes(200, 179, 150),
				GenColor.FromBytes(255, 199, 117),
				GenColor.FromBytes(255, 255, 84),
				GenColor.FromBytes(145, 255, 253),
				GenColor.FromBytes(0, 255, 0),
				GenColor.FromBytes(63, 198, 55),
				GenColor.FromBytes(13, 150, 5),
				GenColor.FromBytes(5, 112, 94)
			};
			GenerateMats(ref matsFertility, FertilitySpectrum, NumMatsPerMode);
			GenerateMats(ref matsTemperature, TemperatureSpectrum, NumMatsPerMode);
			GenerateMats(ref matsElevation, ElevationSpectrum, NumMatsPerMode);
			GenerateMats(ref matsRainfall, RainfallSpectrum, NumMatsPerMode);
		}

		private static void GenerateMats(ref Material[] mats, Color[] colorSpectrum, int numMats)
		{
			mats = new Material[numMats];
			for (int i = 0; i < numMats; i++)
			{
				mats[i] = MatsFromSpectrum.Get(colorSpectrum, (float)i / (float)numMats);
			}
		}

		public static Material MatForFertilityOverlay(float fert)
		{
			int value = Mathf.FloorToInt(fert * (float)NumMatsPerMode);
			return matsFertility[Mathf.Clamp(value, 0, NumMatsPerMode - 1)];
		}

		public static Material MatForTemperature(float temp)
		{
			int value = Mathf.FloorToInt(Mathf.InverseLerp(-50f, 50f, temp) * (float)NumMatsPerMode);
			return matsTemperature[Mathf.Clamp(value, 0, NumMatsPerMode - 1)];
		}

		public static Material MatForElevation(float elev)
		{
			int value = Mathf.FloorToInt(Mathf.InverseLerp(0f, 5000f, elev) * (float)NumMatsPerMode);
			return matsElevation[Mathf.Clamp(value, 0, NumMatsPerMode - 1)];
		}

		public static Material MatForRainfallOverlay(float rain)
		{
			int value = Mathf.FloorToInt(Mathf.InverseLerp(0f, 5000f, rain) * (float)NumMatsPerMode);
			return matsRainfall[Mathf.Clamp(value, 0, NumMatsPerMode - 1)];
		}
	}
}
