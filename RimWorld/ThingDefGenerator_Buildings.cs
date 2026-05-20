using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ThingDefGenerator_Buildings
{
	public static readonly string BlueprintDefNamePrefix = "Blueprint_";

	public static readonly string InstallBlueprintDefNamePrefix = "Install_";

	public static readonly string BuildingFrameDefNamePrefix = "Frame_";

	private static readonly string TerrainBlueprintGraphicPath = "Things/Special/TerrainBlueprint";

	public static readonly Color BlueprintColor = new Color(0.8235294f, 47f / 51f, 1f, 0.6f);

	public static IEnumerable<ThingDef> ImpliedBlueprintAndFrameDefs(bool hotReload = false)
	{
		foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.ToList())
		{
			ThingDef blueprint = null;
			if (def.BuildableByPlayer)
			{
				blueprint = NewBlueprintDef_Thing(def, isInstallBlueprint: false, null, hotReload);
				yield return blueprint;
				yield return NewFrameDef_Thing(def, hotReload);
			}
			if (def.Minifiable)
			{
				yield return NewBlueprintDef_Thing(def, isInstallBlueprint: true, blueprint, hotReload);
			}
		}
		foreach (TerrainDef terrDef in DefDatabase<TerrainDef>.AllDefs)
		{
			if (terrDef.BuildableByPlayer)
			{
				yield return NewBlueprintDef_Terrain(terrDef, hotReload);
				yield return NewFrameDef_Terrain(terrDef, hotReload);
			}
		}
	}

	private static ThingDef BaseBlueprintDef()
	{
		return new ThingDef
		{
			category = ThingCategory.Ethereal,
			label = "Unspecified blueprint",
			altitudeLayer = AltitudeLayer.Blueprint,
			useHitPoints = false,
			selectable = true,
			seeThroughFog = true,
			comps = 
			{
				(CompProperties)new CompProperties_Forbiddable(),
				(CompProperties)new CompProperties_Styleable()
			},
			drawerType = DrawerType.MapMeshOnly
		};
	}

	private static ThingDef BaseFrameDef()
	{
		return new ThingDef
		{
			isFrameInt = true,
			category = ThingCategory.Building,
			label = "Unspecified building frame",
			thingClass = typeof(Frame),
			altitudeLayer = AltitudeLayer.BuildingOnTop,
			useHitPoints = true,
			selectable = true,
			drawerType = DrawerType.RealtimeOnly,
			building = new BuildingProperties(),
			comps = 
			{
				(CompProperties)new CompProperties_Forbiddable(),
				(CompProperties)new CompProperties_Styleable()
			},
			scatterableOnMapGen = false,
			leaveResourcesWhenKilled = true
		};
	}

	private static ThingDef NewBlueprintDef_Thing(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null, bool hotReload = false)
	{
		string defName = BlueprintDefNamePrefix + def.defName;
		if (isInstallBlueprint)
		{
			defName = BlueprintDefNamePrefix + InstallBlueprintDefNamePrefix + def.defName;
		}
		ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? BaseBlueprintDef()) : BaseBlueprintDef());
		thingDef.defName = defName;
		thingDef.label = def.label + "BlueprintLabelExtra".Translate();
		thingDef.size = def.size;
		thingDef.clearBuildingArea = def.clearBuildingArea;
		thingDef.modContentPack = def.modContentPack;
		thingDef.rotatable = def.rotatable;
		thingDef.replaceTags = def.replaceTags;
		if (!isInstallBlueprint)
		{
			thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
			thingDef.artisticSkillPrerequisite = def.artisticSkillPrerequisite;
		}
		thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
		if (def.placeWorkers != null)
		{
			thingDef.placeWorkers = new List<Type>(def.placeWorkers);
		}
		if (isInstallBlueprint && normalBlueprint != null)
		{
			thingDef.graphicData = normalBlueprint.graphicData;
		}
		else
		{
			thingDef.graphicData = new GraphicData();
			if (def.building != null && def.building.blueprintGraphicData != null)
			{
				thingDef.graphicData.CopyFrom(def.building.blueprintGraphicData);
				if (thingDef.graphicData.graphicClass == null)
				{
					thingDef.graphicData.graphicClass = typeof(Graphic_Single);
				}
				if (thingDef.graphicData.shaderType == null)
				{
					thingDef.graphicData.shaderType = ShaderTypeDefOf.Transparent;
				}
				if (def.graphicData != null)
				{
					if (def.building.blueprintGraphicData.drawSize == Vector2.one)
					{
						thingDef.graphicData.drawSize = def.graphicData.drawSize;
					}
					thingDef.graphicData.linkFlags = def.graphicData.linkFlags;
					thingDef.graphicData.linkType = def.graphicData.linkType;
					thingDef.graphicData.asymmetricLink = def.graphicData.asymmetricLink;
				}
				thingDef.graphicData.color = BlueprintColor;
			}
			else
			{
				thingDef.graphicData.CopyFrom(def.graphicData);
				thingDef.graphicData.shaderType = ShaderTypeDefOf.EdgeDetect;
				thingDef.graphicData.color = BlueprintColor;
				thingDef.graphicData.colorTwo = Color.white;
				thingDef.graphicData.shadowData = null;
			}
		}
		thingDef.graphicData.renderQueue = 2950;
		thingDef.defaultPlacingRot = def.defaultPlacingRot;
		if (thingDef.graphicData.shadowData != null)
		{
			Log.Error("Blueprint has shadow: " + def);
		}
		if (isInstallBlueprint)
		{
			thingDef.thingClass = typeof(Blueprint_Install);
		}
		else if (def.building != null)
		{
			thingDef.thingClass = def.building.blueprintClass;
		}
		else
		{
			Log.Error("Tried creating build blueprint for thing that has no blueprint class assigned!");
		}
		if (def.thingClass == typeof(Building_Door))
		{
			thingDef.drawerType = DrawerType.RealtimeOnly;
		}
		else
		{
			thingDef.drawerType = def.drawerType;
		}
		thingDef.forceMoveItemsBeforeConstruction = def.forceMoveItemsBeforeConstruction;
		thingDef.entityDefToBuild = def;
		if (isInstallBlueprint)
		{
			def.installBlueprintDef = thingDef;
		}
		else
		{
			def.blueprintDef = thingDef;
		}
		return thingDef;
	}

	private static ThingDef NewFrameDef_Thing(ThingDef def, bool hotReload = false)
	{
		string defName = BuildingFrameDefNamePrefix + def.defName;
		ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? BaseFrameDef()) : BaseFrameDef());
		thingDef.defName = defName;
		thingDef.label = def.label + "FrameLabelExtra".Translate();
		thingDef.size = def.size;
		thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)def.BaseMaxHitPoints * 0.25f);
		thingDef.SetStatBaseValue(StatDefOf.Beauty, -8f);
		thingDef.SetStatBaseValue(StatDefOf.Flammability, def.BaseFlammability);
		thingDef.fillPercent = 0.2f;
		thingDef.pathCost = 14;
		thingDef.description = def.description;
		thingDef.passability = def.passability;
		thingDef.altitudeLayer = def.altitudeLayer;
		if ((int)thingDef.passability > 1)
		{
			thingDef.passability = Traversability.PassThroughOnly;
		}
		thingDef.selectable = def.selectable;
		thingDef.constructEffect = def.constructEffect;
		thingDef.building.isEdifice = def.building.isEdifice;
		thingDef.building.watchBuildingInSameRoom = def.building.watchBuildingInSameRoom;
		thingDef.building.watchBuildingStandDistanceRange = def.building.watchBuildingStandDistanceRange;
		thingDef.building.watchBuildingStandRectWidth = def.building.watchBuildingStandRectWidth;
		thingDef.building.artificialForMeditationPurposes = def.building.artificialForMeditationPurposes;
		thingDef.constructionSkillPrerequisite = def.constructionSkillPrerequisite;
		thingDef.artisticSkillPrerequisite = def.artisticSkillPrerequisite;
		thingDef.clearBuildingArea = def.clearBuildingArea;
		thingDef.forceMoveItemsBeforeConstruction = def.forceMoveItemsBeforeConstruction;
		thingDef.modContentPack = def.modContentPack;
		thingDef.blocksAltitudes = def.blocksAltitudes;
		thingDef.replaceTags = def.replaceTags;
		thingDef.rotatable = def.rotatable;
		thingDef.drawPlaceWorkersWhileSelected = def.drawPlaceWorkersWhileSelected;
		if (def.placeWorkers != null)
		{
			thingDef.placeWorkers = new List<Type>(def.placeWorkers);
		}
		if (def.BuildableByPlayer)
		{
			thingDef.stuffCategories = def.stuffCategories;
			thingDef.costListForDifficulty = def.costListForDifficulty;
		}
		thingDef.entityDefToBuild = def;
		def.frameDef = thingDef;
		return thingDef;
	}

	private static ThingDef NewBlueprintDef_Terrain(TerrainDef terrDef, bool hotReload = false)
	{
		string defName = BlueprintDefNamePrefix + terrDef.defName;
		ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? BaseBlueprintDef()) : BaseBlueprintDef());
		thingDef.thingClass = typeof(Blueprint_Build);
		thingDef.defName = defName;
		thingDef.label = terrDef.label + "BlueprintLabelExtra".Translate();
		thingDef.entityDefToBuild = terrDef;
		thingDef.graphicData = new GraphicData();
		thingDef.graphicData.shaderType = ShaderTypeDefOf.MetaOverlay;
		thingDef.graphicData.texPath = TerrainBlueprintGraphicPath;
		thingDef.graphicData.renderQueue = 2950;
		thingDef.graphicData.graphicClass = typeof(Graphic_Single);
		thingDef.constructionSkillPrerequisite = terrDef.constructionSkillPrerequisite;
		thingDef.artisticSkillPrerequisite = terrDef.artisticSkillPrerequisite;
		thingDef.clearBuildingArea = false;
		thingDef.forceMoveItemsBeforeConstruction = terrDef.forceMoveItemsBeforeConstruction;
		thingDef.modContentPack = terrDef.modContentPack;
		thingDef.entityDefToBuild = terrDef;
		terrDef.blueprintDef = thingDef;
		return thingDef;
	}

	private static ThingDef NewFrameDef_Terrain(TerrainDef terrDef, bool hotReload = false)
	{
		string defName = BuildingFrameDefNamePrefix + terrDef.defName;
		ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? BaseFrameDef()) : BaseFrameDef());
		thingDef.building.artificialForMeditationPurposes = false;
		thingDef.defName = defName;
		thingDef.label = terrDef.label + "FrameLabelExtra".Translate();
		thingDef.entityDefToBuild = terrDef;
		thingDef.useHitPoints = false;
		thingDef.fillPercent = 0f;
		thingDef.description = "Terrain building in progress.";
		thingDef.passability = Traversability.Standable;
		thingDef.selectable = true;
		thingDef.constructEffect = terrDef.constructEffect;
		thingDef.building.isEdifice = false;
		thingDef.constructionSkillPrerequisite = terrDef.constructionSkillPrerequisite;
		thingDef.artisticSkillPrerequisite = terrDef.artisticSkillPrerequisite;
		thingDef.clearBuildingArea = false;
		thingDef.forceMoveItemsBeforeConstruction = terrDef.forceMoveItemsBeforeConstruction;
		thingDef.modContentPack = terrDef.modContentPack;
		thingDef.category = ThingCategory.Ethereal;
		thingDef.entityDefToBuild = terrDef;
		terrDef.frameDef = thingDef;
		if (!thingDef.IsFrame)
		{
			Log.Error("Framedef is not frame: " + thingDef);
		}
		return thingDef;
	}
}
