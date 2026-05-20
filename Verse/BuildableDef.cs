using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public abstract class BuildableDef : Def, IEquatable<BuildableDef>
{
	public List<StatModifier> statBases;

	public Traversability passability;

	public int pathCost;

	public bool pathCostIgnoreRepeat = true;

	public float fertility = -1f;

	public List<ThingDefCountClass> costList;

	public int costStuffCount;

	public List<StuffCategoryDef> stuffCategories;

	[MustTranslate]
	public string stuffCategorySummary;

	public CostListForDifficulty costListForDifficulty;

	public DrawStyleCategoryDef drawStyleCategory;

	public bool clearBuildingArea = true;

	public Rot4 defaultPlacingRot = Rot4.North;

	public float resourcesFractionWhenDeconstructed = 0.5f;

	public List<AltitudeLayer> blocksAltitudes;

	public StyleCategoryDef dominantStyleCategory;

	public bool forcePassableByFlyingPawns;

	public bool forceMoveItemsBeforeConstruction;

	public bool isAltar;

	public bool useStuffTerrainAffordance;

	public TerrainAffordanceDef terrainAffordanceNeeded;

	public List<ThingDef> buildingPrerequisites;

	public List<ThingDef> discoveryPrerequisites;

	public List<ResearchProjectDef> researchPrerequisites;

	public int minMonolithLevel;

	public int constructionSkillPrerequisite;

	public int artisticSkillPrerequisite;

	public TechLevel minTechLevelToBuild;

	public TechLevel maxTechLevelToBuild;

	public bool requireInspectedGravEngine;

	public AltitudeLayer altitudeLayer = AltitudeLayer.Item;

	public EffecterDef repairEffect;

	public EffecterDef constructEffect;

	public List<ColorForStuff> colorPerStuff;

	public bool canGenerateDefaultDesignator = true;

	public bool ideoBuilding;

	public float specialDisplayRadius;

	public List<Type> placeWorkers;

	public DesignationCategoryDef designationCategory;

	public DesignatorDropdownGroupDef designatorDropdown;

	public KeyBindingDef designationHotKey;

	public float uiOrder = 2999f;

	[NoTranslate]
	public string uiIconPath;

	public List<IconForStuffAppearance> uiIconPathsStuff;

	public Vector2 uiIconOffset;

	public Color uiIconColor = Color.white;

	public Color uiIconColorTwo = Color.white;

	public int uiIconForStackCount = -1;

	[Unsaved(false)]
	public ThingDef blueprintDef;

	[Unsaved(false)]
	public ThingDef installBlueprintDef;

	[Unsaved(false)]
	public ThingDef frameDef;

	[Unsaved(false)]
	private List<PlaceWorker> placeWorkersInstantiatedInt;

	[Unsaved(false)]
	public Graphic graphic = BaseContent.BadGraphic;

	[Unsaved(false)]
	public Texture2D uiIcon = BaseContent.BadTex;

	[Unsaved(false)]
	public Material uiIconMaterial;

	[Unsaved(false)]
	public Dictionary<StuffAppearanceDef, Texture2D> stuffUiIcons;

	[Unsaved(false)]
	public float uiIconAngle;

	protected static List<string> tmpCostList = new List<string>();

	protected static List<Dialog_InfoCard.Hyperlink> tmpHyperlinks = new List<Dialog_InfoCard.Hyperlink>();

	public virtual IntVec2 Size => IntVec2.One;

	public bool MadeFromStuff => !stuffCategories.NullOrEmpty();

	public bool BuildableByPlayer => designationCategory != null;

	public Material DrawMatSingle => graphic?.MatSingle;

	public float Altitude => altitudeLayer.AltitudeFor();

	public bool AffectsFertility => fertility >= 0f;

	public List<PlaceWorker> PlaceWorkers
	{
		get
		{
			if (placeWorkers == null)
			{
				return null;
			}
			if (placeWorkersInstantiatedInt == null)
			{
				placeWorkersInstantiatedInt = new List<PlaceWorker>();
				foreach (Type placeWorker in placeWorkers)
				{
					placeWorkersInstantiatedInt.Add((PlaceWorker)Activator.CreateInstance(placeWorker));
				}
			}
			return placeWorkersInstantiatedInt;
		}
	}

	public bool IsResearchFinished
	{
		get
		{
			if (researchPrerequisites != null)
			{
				for (int i = 0; i < researchPrerequisites.Count; i++)
				{
					if (!researchPrerequisites[i].IsFinished)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public List<ThingDefCountClass> CostList
	{
		get
		{
			if (costListForDifficulty != null && costListForDifficulty.Applies)
			{
				return costListForDifficulty.costList;
			}
			return costList;
		}
	}

	public int CostStuffCount
	{
		get
		{
			if (costListForDifficulty != null && costListForDifficulty.Applies)
			{
				return costListForDifficulty.costStuffCount;
			}
			return costStuffCount;
		}
	}

	public bool ForceAllowPlaceOver(BuildableDef other)
	{
		if (PlaceWorkers == null)
		{
			return false;
		}
		for (int i = 0; i < PlaceWorkers.Count; i++)
		{
			if (PlaceWorkers[i].ForceAllowPlaceOver(other))
			{
				return true;
			}
		}
		return false;
	}

	public override void PostLoad()
	{
		base.PostLoad();
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (!uiIconPath.NullOrEmpty())
			{
				uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
			}
			else
			{
				ResolveIcon();
			}
			if (uiIconPathsStuff != null)
			{
				stuffUiIcons = new Dictionary<StuffAppearanceDef, Texture2D>();
				foreach (IconForStuffAppearance item in uiIconPathsStuff)
				{
					stuffUiIcons.Add(item.Appearance, ContentFinder<Texture2D>.Get(item.IconPath));
				}
			}
		});
	}

	protected virtual void ResolveIcon()
	{
		if (graphic == null || graphic == BaseContent.BadGraphic || this is ThingDef { mote: not null })
		{
			return;
		}
		Graphic outerGraphic = graphic;
		if (uiIconForStackCount >= 1 && this is ThingDef && graphic is Graphic_StackCount graphic_StackCount)
		{
			outerGraphic = graphic_StackCount.SubGraphicForStackCount(uiIconForStackCount, (ThingDef)this);
		}
		Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(defaultPlacingRot);
		if (ShaderDatabase.TryGetUIShader(material.shader, out var uiShader) && MaterialPool.TryGetRequestForMat(material, out var request))
		{
			request.shader = uiShader;
			if (request.colorTwo == Color.white)
			{
				request.colorTwo = uiIconColorTwo;
			}
			uiIconMaterial = MaterialPool.MatFrom(request);
		}
		uiIcon = (Texture2D)material.mainTexture;
		uiIconColor = material.color;
	}

	public Texture2D GetUIIconForStuff(ThingDef stuff)
	{
		if (stuffUiIcons == null || stuff?.stuffProps.appearance == null || !stuffUiIcons.TryGetValue(stuff.stuffProps.appearance, out var value))
		{
			return uiIcon;
		}
		return value;
	}

	public Color GetColorForStuff(ThingDef stuff)
	{
		if (colorPerStuff.NullOrEmpty())
		{
			return stuff.stuffProps.color;
		}
		for (int i = 0; i < colorPerStuff.Count; i++)
		{
			ColorForStuff colorForStuff = colorPerStuff[i];
			if (colorForStuff.Stuff == stuff)
			{
				return colorForStuff.Color;
			}
		}
		return stuff.stuffProps.color;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (useStuffTerrainAffordance && !MadeFromStuff)
		{
			yield return "useStuffTerrainAffordance is true but it's not made from stuff";
		}
		if (costListForDifficulty != null && costListForDifficulty.difficultyVar.NullOrEmpty())
		{
			yield return "costListForDifficulty is not referencing a difficulty.";
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (!BuildableByPlayer)
		{
			yield break;
		}
		IEnumerable<TerrainAffordanceDef> enumerable = Enumerable.Empty<TerrainAffordanceDef>();
		if (PlaceWorkers != null)
		{
			enumerable = enumerable.Concat(PlaceWorkers.SelectMany((PlaceWorker pw) => pw.DisplayAffordances()));
		}
		TerrainAffordanceDef terrainAffordanceNeed = this.GetTerrainAffordanceNeed(req.StuffDef);
		if (terrainAffordanceNeed != null)
		{
			enumerable = enumerable.Concat(terrainAffordanceNeed);
		}
		string[] array = (from ta in enumerable.Distinct()
			orderby ta.order
			select ta.label).ToArray();
		if (array.Length != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "TerrainRequirement".Translate(), array.ToCommaList().CapitalizeFirst(), "Stat_Thing_TerrainRequirement_Desc".Translate(), 1101);
		}
		tmpCostList.Clear();
		tmpHyperlinks.Clear();
		if (MadeFromStuff && costStuffCount > 0)
		{
			tmpCostList.Add(costStuffCount + "x " + stuffCategories.Select((StuffCategoryDef x) => x.label).ToCommaListOr());
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.IsStuff && !allDef.stuffProps.categories.NullOrEmpty() && allDef.stuffProps.categories.Any((StuffCategoryDef x) => stuffCategories.Contains(x)))
				{
					tmpHyperlinks.Add(new Dialog_InfoCard.Hyperlink(allDef));
				}
			}
		}
		List<ThingDefCountClass> list = CostList;
		if (!list.NullOrEmpty())
		{
			foreach (ThingDefCountClass c in list)
			{
				tmpCostList.Add(c.Summary);
				if (!tmpHyperlinks.Any((Dialog_InfoCard.Hyperlink x) => x.def == c.thingDef))
				{
					tmpHyperlinks.Add(new Dialog_InfoCard.Hyperlink(c.thingDef));
				}
			}
		}
		if (tmpCostList.Any())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "Stat_Building_ResourcesToMake".Translate(), tmpCostList.ToCommaList().CapitalizeFirst(), "Stat_Building_ResourcesToMakeDesc".Translate(), 4405, null, tmpHyperlinks);
		}
	}

	public override string ToString()
	{
		return defName;
	}

	public bool Equals(BuildableDef other)
	{
		return defName == other.defName;
	}
}
