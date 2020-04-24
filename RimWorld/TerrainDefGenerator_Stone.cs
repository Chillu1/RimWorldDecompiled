using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TerrainDefGenerator_Stone
	{
		public static IEnumerable<TerrainDef> ImpliedTerrainDefs()
		{
			int i = 0;
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.building != null && def.building.isNaturalRock && !def.building.isResourceRock))
			{
				TerrainDef terrainDef = new TerrainDef();
				TerrainDef hewn = new TerrainDef();
				TerrainDef smooth = new TerrainDef();
				terrainDef.texturePath = "Terrain/Surfaces/RoughStone";
				terrainDef.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				terrainDef.pathCost = 2;
				StatUtility.SetStatValueInList(ref terrainDef.statBases, StatDefOf.Beauty, -1f);
				terrainDef.scatterType = "Rocky";
				terrainDef.affordances = new List<TerrainAffordanceDef>();
				terrainDef.affordances.Add(TerrainAffordanceDefOf.Light);
				terrainDef.affordances.Add(TerrainAffordanceDefOf.Medium);
				terrainDef.affordances.Add(TerrainAffordanceDefOf.Heavy);
				terrainDef.affordances.Add(TerrainAffordanceDefOf.SmoothableStone);
				terrainDef.fertility = 0f;
				terrainDef.filthAcceptanceMask = (FilthSourceFlags.Terrain | FilthSourceFlags.Unnatural);
				terrainDef.modContentPack = item.modContentPack;
				terrainDef.renderPrecedence = 190 + i;
				terrainDef.defName = item.defName + "_Rough";
				terrainDef.label = "RoughStoneTerrainLabel".Translate(item.label);
				terrainDef.description = "RoughStoneTerrainDesc".Translate(item.label);
				terrainDef.color = item.graphicData.color;
				item.building.naturalTerrain = terrainDef;
				hewn.texturePath = "Terrain/Surfaces/RoughHewnRock";
				hewn.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				hewn.pathCost = 1;
				StatUtility.SetStatValueInList(ref hewn.statBases, StatDefOf.Beauty, -1f);
				hewn.scatterType = "Rocky";
				hewn.affordances = new List<TerrainAffordanceDef>();
				hewn.affordances.Add(TerrainAffordanceDefOf.Light);
				hewn.affordances.Add(TerrainAffordanceDefOf.Medium);
				hewn.affordances.Add(TerrainAffordanceDefOf.Heavy);
				hewn.affordances.Add(TerrainAffordanceDefOf.SmoothableStone);
				hewn.fertility = 0f;
				hewn.filthAcceptanceMask = FilthSourceFlags.Any;
				hewn.modContentPack = item.modContentPack;
				hewn.renderPrecedence = 50 + i;
				hewn.defName = item.defName + "_RoughHewn";
				hewn.label = "RoughHewnStoneTerrainLabel".Translate(item.label);
				hewn.description = "RoughHewnStoneTerrainDesc".Translate(item.label);
				hewn.color = item.graphicData.color;
				item.building.leaveTerrain = hewn;
				smooth.texturePath = "Terrain/Surfaces/SmoothStone";
				smooth.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				smooth.pathCost = 0;
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.Beauty, 2f);
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.MarketValue, 8f);
				smooth.scatterType = "Rocky";
				smooth.affordances = new List<TerrainAffordanceDef>();
				smooth.affordances.Add(TerrainAffordanceDefOf.Light);
				smooth.affordances.Add(TerrainAffordanceDefOf.Medium);
				smooth.affordances.Add(TerrainAffordanceDefOf.Heavy);
				smooth.fertility = 0f;
				smooth.filthAcceptanceMask = FilthSourceFlags.Any;
				smooth.modContentPack = item.modContentPack;
				smooth.tags = new List<string>
				{
					"Floor"
				};
				smooth.renderPrecedence = 140 + i;
				smooth.defName = item.defName + "_Smooth";
				smooth.label = "SmoothStoneTerrainLabel".Translate(item.label);
				smooth.description = "SmoothStoneTerrainDesc".Translate(item.label);
				smooth.color = item.graphicData.color;
				terrainDef.smoothedTerrain = smooth;
				hewn.smoothedTerrain = smooth;
				yield return terrainDef;
				yield return hewn;
				yield return smooth;
				i++;
			}
		}
	}
}
