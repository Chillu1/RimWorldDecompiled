using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class TerrainDefGenerator_Carpet
{
	public static IEnumerable<TerrainDef> ImpliedTerrainDefs(bool hotReload = false)
	{
		IEnumerable<ColorDef> enumerable = DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Structure);
		foreach (ColorDef c in enumerable)
		{
			int index = 0;
			foreach (TerrainTemplateDef allDef in DefDatabase<TerrainTemplateDef>.AllDefs)
			{
				yield return CarpetFromBlueprint(allDef, c, index, hotReload);
				index++;
			}
		}
	}

	public static TerrainDef CarpetFromBlueprint(TerrainTemplateDef tp, ColorDef colorDef, int index, bool hotReload = false)
	{
		string defName = tp.defName + colorDef.defName.Replace("Structure_", "");
		TerrainDef terrainDef = (hotReload ? (DefDatabase<TerrainDef>.GetNamed(defName, errorOnFail: false) ?? new TerrainDef()) : new TerrainDef());
		terrainDef.defName = defName;
		terrainDef.label = tp.label.Formatted(colorDef.label);
		terrainDef.texturePath = tp.texturePath;
		terrainDef.researchPrerequisites = tp.researchPrerequisites;
		terrainDef.burnedDef = tp.burnedDef;
		terrainDef.costList = tp.costList;
		terrainDef.description = tp.description;
		terrainDef.colorDef = colorDef;
		terrainDef.designatorDropdown = tp.designatorDropdown;
		terrainDef.uiOrder = tp.uiOrder;
		terrainDef.statBases = tp.statBases;
		terrainDef.renderPrecedence = tp.renderPrecedenceStart - index;
		terrainDef.constructionSkillPrerequisite = tp.constructionSkillPrerequisite;
		terrainDef.canGenerateDefaultDesignator = tp.canGenerateDefaultDesignator;
		terrainDef.tags = tp.tags;
		terrainDef.dominantStyleCategory = tp.dominantStyleCategory;
		terrainDef.layerable = true;
		terrainDef.affordances = new List<TerrainAffordanceDef>
		{
			TerrainAffordanceDefOf.Walkable,
			TerrainAffordanceDefOf.Light,
			TerrainAffordanceDefOf.Medium,
			TerrainAffordanceDefOf.Heavy
		};
		terrainDef.designationCategory = DesignationCategoryDefOf.Floors;
		terrainDef.fertility = 0f;
		terrainDef.constructEffect = EffecterDefOf.ConstructDirt;
		terrainDef.pollutionColor = new Color(1f, 1f, 1f, 0.8f);
		terrainDef.pollutionOverlayScale = new Vector2(0.75f, 0.75f);
		terrainDef.pollutionOverlayTexturePath = "Terrain/Surfaces/PollutionFloorSmooth";
		terrainDef.terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy;
		if (ModsConfig.BiotechActive)
		{
			terrainDef.pollutionShaderType = ShaderTypeDefOf.TerrainFadeRoughLinearBurn;
		}
		return terrainDef;
	}
}
