using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class TerrainDefGenerator_Stone
{
	public static IEnumerable<TerrainDef> ImpliedTerrainDefs(bool hotReload = false)
	{
		int i = 0;
		IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.building != null && def.building.isNaturalRock && !def.building.isResourceRock);
		foreach (ThingDef item in enumerable)
		{
			string defName = item.defName + "_Rough";
			string defName2 = item.defName + "_RoughHewn";
			string defName3 = item.defName + "_Smooth";
			TerrainDef smooth = null;
			bool flag = item.building.naturalTerrain != null;
			bool hewnExists = item.building.leaveTerrain != null;
			TerrainDef terrainDef;
			if (item.building.naturalTerrain != null)
			{
				terrainDef = item.building.naturalTerrain;
			}
			else
			{
				terrainDef = (hotReload ? (DefDatabase<TerrainDef>.GetNamed(defName, errorOnFail: false) ?? new TerrainDef()) : new TerrainDef());
				terrainDef.texturePath = "Terrain/Surfaces/RoughStone";
				terrainDef.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				terrainDef.pathCost = 2;
				StatUtility.SetStatValueInList(ref terrainDef.statBases, StatDefOf.Beauty, -1f);
				terrainDef.scatterType = "Rocky";
				terrainDef.affordances = new List<TerrainAffordanceDef>
				{
					TerrainAffordanceDefOf.Light,
					TerrainAffordanceDefOf.Medium,
					TerrainAffordanceDefOf.Heavy,
					TerrainAffordanceDefOf.SmoothableStone,
					TerrainAffordanceDefOf.Walkable
				};
				terrainDef.fertility = 0f;
				terrainDef.filthAcceptanceMask = FilthSourceFlags.Terrain | FilthSourceFlags.Unnatural;
				terrainDef.modContentPack = item.modContentPack;
				terrainDef.categoryType = TerrainDef.TerrainCategoryType.Stone;
				terrainDef.tags = new List<string> { "NaturalRock" };
				terrainDef.renderPrecedence = 190 + i;
				terrainDef.defName = defName;
				terrainDef.label = "RoughStoneTerrainLabel".Translate(item.label);
				terrainDef.description = "RoughStoneTerrainDesc".Translate(item.label);
				terrainDef.color = item.graphicData.color;
				terrainDef.natural = true;
				terrainDef.pollutedTexturePath = "Terrain/Surfaces/RoughStonePolluted";
				terrainDef.pollutionOverlayTexturePath = "Terrain/Surfaces/RoughStonePollutionOverlay";
				terrainDef.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearAdd;
				terrainDef.pollutionColor = new Color(1f, 1f, 1f, 1f);
				SetAsteroidEdgeData(terrainDef);
				if (item == ThingDefOf.Vacstone)
				{
					terrainDef.scatterType = "Craters";
				}
				item.building.naturalTerrain = terrainDef;
			}
			TerrainDef hewn;
			if (item.building.leaveTerrain != null)
			{
				hewn = item.building.leaveTerrain;
			}
			else
			{
				hewn = (hotReload ? (DefDatabase<TerrainDef>.GetNamed(defName2, errorOnFail: false) ?? new TerrainDef()) : new TerrainDef());
				hewn.texturePath = "Terrain/Surfaces/RoughHewnRock";
				hewn.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				hewn.pathCost = 1;
				StatUtility.SetStatValueInList(ref hewn.statBases, StatDefOf.Beauty, -1f);
				hewn.scatterType = "Rocky";
				hewn.affordances = new List<TerrainAffordanceDef>
				{
					TerrainAffordanceDefOf.Light,
					TerrainAffordanceDefOf.Medium,
					TerrainAffordanceDefOf.Heavy,
					TerrainAffordanceDefOf.SmoothableStone,
					TerrainAffordanceDefOf.Walkable
				};
				hewn.fertility = 0f;
				hewn.filthAcceptanceMask = FilthSourceFlags.Any;
				hewn.modContentPack = item.modContentPack;
				hewn.pollutedTexturePath = "Terrain/Surfaces/RoughHewnRockPolluted";
				hewn.pollutionOverlayTexturePath = "Terrain/Surfaces/RoughStonePollutionOverlay";
				hewn.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearAdd;
				hewn.pollutionColor = new Color(1f, 1f, 1f, 1f);
				hewn.categoryType = TerrainDef.TerrainCategoryType.Stone;
				hewn.renderPrecedence = 50 + i;
				hewn.defName = defName2;
				hewn.label = "RoughHewnStoneTerrainLabel".Translate(item.label);
				hewn.description = "RoughHewnStoneTerrainDesc".Translate(item.label);
				hewn.color = item.graphicData.color;
				if (item == ThingDefOf.Vacstone)
				{
					hewn.scatterType = "Craters";
				}
				item.building.leaveTerrain = hewn;
			}
			if (!flag || !hewnExists)
			{
				smooth = (hotReload ? (DefDatabase<TerrainDef>.GetNamed(defName3, errorOnFail: false) ?? new TerrainDef()) : new TerrainDef());
				smooth.texturePath = "Terrain/Surfaces/SmoothStone";
				smooth.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				smooth.pathCost = 0;
				smooth.isPaintable = true;
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.Beauty, 2f);
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.MarketValue, 5f);
				smooth.scatterType = "Rocky";
				smooth.affordances = new List<TerrainAffordanceDef>
				{
					TerrainAffordanceDefOf.Light,
					TerrainAffordanceDefOf.Medium,
					TerrainAffordanceDefOf.Heavy,
					TerrainAffordanceDefOf.Walkable
				};
				smooth.fertility = 0f;
				smooth.filthAcceptanceMask = FilthSourceFlags.Any;
				smooth.modContentPack = item.modContentPack;
				smooth.tags = new List<string> { "Floor" };
				smooth.pollutedTexturePath = "Terrain/Surfaces/SmoothStonePolluted";
				smooth.pollutionOverlayTexturePath = "Terrain/Surfaces/RoughStonePollutionOverlay";
				smooth.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearAdd;
				smooth.pollutionColor = new Color(1f, 1f, 1f, 0.8f);
				smooth.categoryType = TerrainDef.TerrainCategoryType.Stone;
				smooth.renderPrecedence = 140 + i;
				smooth.defName = defName3;
				smooth.label = "SmoothStoneTerrainLabel".Translate(item.label);
				smooth.description = "SmoothStoneTerrainDesc".Translate(item.label);
				smooth.color = item.graphicData.color;
				if (!flag)
				{
					terrainDef.smoothedTerrain = smooth;
				}
				if (!hewnExists)
				{
					hewn.smoothedTerrain = smooth;
				}
			}
			if (!flag)
			{
				yield return terrainDef;
			}
			if (!hewnExists)
			{
				yield return hewn;
			}
			if (smooth != null)
			{
				yield return smooth;
			}
			i++;
		}
	}

	private static void SetAsteroidEdgeData(TerrainDef terrain)
	{
		if (ModsConfig.OdysseyActive)
		{
			if (terrain.spaceEdgeGraphicData == null)
			{
				terrain.spaceEdgeGraphicData = new TerrainDef.SpaceEdgeGraphicData
				{
					OShapeTexPath = "Terrain/Surfaces/Asteroid/BorderInside/AsteroidBorderInside_OShape",
					UShapeTexPath = "Terrain/Surfaces/Asteroid/BorderInside/AsteroidBorderInside_UShape",
					CornerInnerTexPath = "Terrain/Surfaces/Asteroid/BorderInside/AsteroidBorderInside_CornerInner",
					CornerOuterTexPath = "Terrain/Surfaces/Asteroid/BorderInside/AsteroidBorderInside_CornerOuter",
					FlatTexPath = "Terrain/Surfaces/Asteroid/BorderInside/AsteroidBorderInside_Flat",
					LoopLeftTexPath = "Terrain/Surfaces/Asteroid/AsteroidEdge_Left",
					LoopRightTexPath = "Terrain/Surfaces/Asteroid/AsteroidEdge_Right",
					LoopSingleTexPath = "Terrain/Surfaces/Asteroid/AsteroidEdge_Single",
					LoopTexPaths = new List<string> { "Terrain/Surfaces/Asteroid/Loop/AsteroidEdge_Loop1", "Terrain/Surfaces/Asteroid/Loop/AsteroidEdge_Loop2", "Terrain/Surfaces/Asteroid/Loop/AsteroidEdge_Loop3", "Terrain/Surfaces/Asteroid/Loop/AsteroidEdge_Loop4", "Terrain/Surfaces/Asteroid/Loop/AsteroidEdge_Loop5" }
				};
			}
		}
	}
}
