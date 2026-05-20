using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class ThingDef : BuildableDef
{
	public Type thingClass;

	public ThingCategory category;

	public TickerType tickerType;

	public int stackLimit = 1;

	public IntVec2 size = IntVec2.One;

	public bool destroyable = true;

	public bool rotatable = true;

	public bool smallVolume;

	public bool useHitPoints = true;

	public bool receivesSignals;

	public List<CompProperties> comps = new List<CompProperties>();

	public List<ThingDef> virtualDefs = new List<ThingDef>();

	public ThingDef virtualDefParent;

	[NoTranslate]
	public string devNote;

	public List<ThingDefCountRangeClass> killedLeavingsRanges;

	public List<ThingDefCountClass> killedLeavings;

	public List<ThingDefCountClass> killedLeavingsPlayerHostile;

	public float killedLeavingsChance = 1f;

	public bool forceLeavingsAllowed;

	public List<ThingDefCountClass> butcherProducts;

	public List<ThingDefCountClass> smeltProducts;

	public bool smeltable;

	public bool burnableByRecipe;

	public bool randomizeRotationOnSpawn;

	public List<DamageMultiplier> damageMultipliers;

	public bool isTechHediff;

	public RecipeMakerProperties recipeMaker;

	public ThingDef minifiedDef;

	public bool isUnfinishedThing;

	public bool leaveResourcesWhenKilled;

	public ThingDef slagDef;

	public bool isFrameInt;

	public List<IntVec3> multipleInteractionCellOffsets;

	public IntVec3 interactionCellOffset = IntVec3.Zero;

	public bool hasInteractionCell;

	public ThingDef interactionCellIcon;

	public bool interactionCellIconReverse;

	public ThingDef filthLeaving;

	public bool forceDebugSpawnable;

	public bool intricate;

	public bool scatterableOnMapGen = true;

	public float deepCommonality;

	public int deepCountPerCell = 300;

	public int deepCountPerPortion = -1;

	public IntRange deepLumpSizeRange = IntRange.Zero;

	public float generateCommonality = 1f;

	public float generateAllowChance = 1f;

	private bool canOverlapZones = true;

	public FloatRange startingHpRange = FloatRange.One;

	[NoTranslate]
	public List<string> thingSetMakerTags;

	public bool alwaysFlee;

	public List<RecipeDef> recipes;

	public bool messageOnDeteriorateInStorage = true;

	public bool deteriorateFromEnvironmentalEffects = true;

	public bool canDeteriorateUnspawned;

	public bool canLoadIntoCaravan = true;

	public bool isMechClusterThreat;

	public FloatRange displayNumbersBetweenSameDefDistRange = FloatRange.Zero;

	public int minRewardCount = 1;

	public bool preventSkyfallersLandingOn;

	public FactionDef requiresFactionToAcquire;

	public float relicChance;

	public OrderedTakeGroupDef orderedTakeGroup;

	public int allowedArchonexusCount;

	public int possessionCount;

	public bool notifyMapRemoved;

	public bool canScatterOver = true;

	public bool genericMarketSellable = true;

	public bool drawHighlight;

	public Color? highlightColor;

	public bool drawHighlightOnlyForHostile;

	public bool autoTargetNearbyIdenticalThings;

	public bool preventDroppingThingsOn;

	public bool hiddenWhileUndiscovered;

	public bool disableImpassableShotOverConfigError;

	public bool showInSearch = true;

	public bool bringAlongOnGravship = true;

	public ThingDef dropPodFaller;

	public ThingDef dropPodActive;

	public bool preventSpawningInResourcePod;

	public bool pathfinderDangerous;

	public bool noRightClickDraftAttack;

	public int gravshipSpawnPriority = 1;

	public List<string> replaceTags;

	public bool preventGravshipLandingOn;

	public bool canInteractThroughCorners;

	public GraphicData graphicData;

	public DrawerType drawerType = DrawerType.RealtimeOnly;

	public bool drawOffscreen;

	public ColorGenerator colorGenerator;

	public float hideAtSnowOrSandDepth = 99999f;

	public bool drawDamagedOverlay = true;

	public bool castEdgeShadows;

	public float staticSunShadowHeight;

	public bool useSameGraphicForGhost;

	public bool useBlueprintGraphicAsGhost;

	public List<ThingStyleChance> randomStyle;

	public float randomStyleChance;

	public bool canEditAnyStyle;

	public bool dontPrint;

	public ThingDef defaultStuff;

	public int killedLeavingsExpandRect;

	public bool minifiedManualDraw;

	public float minifiedDrawScale = 1f;

	public Rot4 overrideMinifiedRot = Rot4.Invalid;

	public Vector3 minifiedDrawOffset = Vector3.zero;

	public float deselectedSelectionBracketFactor = 1f;

	public bool selectable;

	public bool containedPawnsSelectable;

	public bool containedItemsSelectable;

	public bool neverMultiSelect;

	public bool isAutoAttackableMapObject;

	public bool hasTooltip;

	public List<Type> inspectorTabs;

	[Unsaved(false)]
	public List<InspectTabBase> inspectorTabsResolved;

	public bool seeThroughFog;

	public bool drawGUIOverlay;

	public bool drawGUIOverlayQuality = true;

	public ResourceCountPriority resourceReadoutPriority;

	public bool resourceReadoutAlwaysShow;

	public bool drawPlaceWorkersWhileSelected;

	public bool drawPlaceWorkersWhileInstallBlueprintSelected;

	public ConceptDef storedConceptLearnOpportunity;

	public float uiIconScale = 1f;

	public bool hasCustomRectForSelector;

	public bool hideStats;

	public bool hideInspect;

	public bool onlyShowInspectString;

	public bool hideMainDesc;

	public bool alwaysHaulable;

	public bool designateHaulable;

	public List<ThingCategoryDef> thingCategories;

	public bool mineable;

	public bool socialPropernessMatters;

	public bool stealable = true;

	public SoundDef soundSpawned;

	public SoundDef soundDrop;

	public SoundDef soundPickup;

	public SoundDef soundInteract;

	public SoundDef soundImpactDefault;

	public SoundDef soundPlayInstrument;

	public SoundDef soundOpen;

	public bool saveCompressible;

	public bool isSaveable = true;

	public bool holdsRoof;

	public float fillPercent;

	public bool coversFloor;

	public bool neverOverlapFloors;

	public SurfaceType surfaceType;

	public bool wipesPlants;

	public bool blockPlants;

	public bool blockLight;

	public bool blockWind;

	public bool blockWeather;

	public Tradeability tradeability = Tradeability.All;

	[NoTranslate]
	public List<string> tradeTags;

	public bool tradeNeverStack;

	public bool tradeNeverGenerateStacked;

	public bool healthAffectsPrice = true;

	public ColorGenerator colorGeneratorInTraderStock;

	private List<VerbProperties> verbs;

	public List<Tool> tools;

	public float equippedAngleOffset;

	public float equippedDistanceOffset;

	public EquipmentType equipmentType;

	public TechLevel techLevel;

	public List<WeaponClassDef> weaponClasses;

	[NoTranslate]
	public List<string> weaponTags;

	[NoTranslate]
	public List<string> techHediffsTags;

	public bool violentTechHediff;

	public bool destroyOnDrop;

	public List<StatModifier> equippedStatOffsets;

	public SoundDef meleeHitSound;

	public float recoilPower = 1f;

	public float recoilRelaxation = 10f;

	public bool rotateInShelves = true;

	public bool mergeVerbGizmos = true;

	public BuildableDef entityDefToBuild;

	public ThingDef projectileWhenLoaded;

	public RulePackDef ideoBuildingNamerBase;

	public EntityCodexEntryDef entityCodexEntry;

	public IngestibleProperties ingestible;

	public FilthProperties filth;

	public GasProperties gas;

	public BuildingProperties building;

	public RaceProperties race;

	public ApparelProperties apparel;

	public MoteProperties mote;

	public PlantProperties plant;

	public ProjectileProperties projectile;

	public StuffProperties stuffProps;

	public SkyfallerProperties skyfaller;

	public PawnFlyerProperties pawnFlyer;

	public RitualFocusProperties ritualFocus;

	public IngredientProperties ingredient;

	public MapPortalProperties portal;

	public bool canBeUsedUnderRoof = true;

	[Unsaved(false)]
	private string descriptionDetailedCached;

	[Unsaved(false)]
	public Graphic interactionCellGraphic;

	[Unsaved(false)]
	private bool? isNaturalOrganCached;

	[Unsaved(false)]
	private bool? hasSunShadowsCached;

	[Unsaved(false)]
	private List<StyleCategoryDef> cachedRelevantStyleCategories;

	public const int SmallUnitPerVolume = 10;

	public const float SmallVolumePerUnit = 0.1f;

	public const float ArchonexusMaxItemStackMass = 5f;

	public const int ArchonexusMaxItemStackCount = 25;

	public const float ArchonexusMaxItemStackValue = 2000f;

	public const int ArchonexusAutoCalculateValue = -1;

	private List<RecipeDef> allRecipesCached;

	private static List<VerbProperties> EmptyVerbPropertiesList = new List<VerbProperties>();

	private Dictionary<ThingDef, Thing> concreteExamplesInt;

	public bool EverHaulable
	{
		get
		{
			if (!alwaysHaulable)
			{
				return designateHaulable;
			}
			return true;
		}
	}

	public bool EverPollutable => !building.isNaturalRock;

	public float VolumePerUnit
	{
		get
		{
			if (smallVolume)
			{
				return 0.1f;
			}
			return 1f;
		}
	}

	public override IntVec2 Size => size;

	public bool DiscardOnDestroyed => race == null;

	public int BaseMaxHitPoints => Mathf.RoundToInt(this.GetStatValueAbstract(StatDefOf.MaxHitPoints));

	public float BaseFlammability => this.GetStatValueAbstract(StatDefOf.Flammability);

	public float BaseMarketValue
	{
		get
		{
			return this.GetStatValueAbstract(StatDefOf.MarketValue);
		}
		set
		{
			this.SetStatBaseValue(StatDefOf.MarketValue, value);
		}
	}

	public float BaseMass => this.GetStatValueAbstract(StatDefOf.Mass);

	public int ArchonexusMaxAllowedCount
	{
		get
		{
			if (allowedArchonexusCount == -1)
			{
				return Mathf.Min(stackLimit, 25, (BaseMass > 0f) ? ((int)(5f / BaseMass)) : 0, (BaseMarketValue > 0f) ? ((int)(2000f / BaseMarketValue)) : 0);
			}
			return allowedArchonexusCount;
		}
	}

	public bool PlayerAcquirable
	{
		get
		{
			if (destroyOnDrop)
			{
				return false;
			}
			if (this == ThingDefOf.ReinforcedBarrel && Find.Storyteller != null && Find.Storyteller.difficulty.classicMortars)
			{
				return false;
			}
			if (requiresFactionToAcquire != null && Find.World != null && Find.World.factionManager != null)
			{
				return Find.FactionManager.FirstFactionOfDef(requiresFactionToAcquire) != null;
			}
			return true;
		}
	}

	public bool EverTransmitsPower
	{
		get
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i] is CompProperties_Power { transmitsPower: not false })
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool Minifiable => minifiedDef != null;

	public bool HasThingIDNumber => category != ThingCategory.Mote;

	public List<RecipeDef> AllRecipes
	{
		get
		{
			if (allRecipesCached == null)
			{
				allRecipesCached = new List<RecipeDef>();
				if (recipes != null)
				{
					for (int i = 0; i < recipes.Count; i++)
					{
						allRecipesCached.Add(recipes[i]);
					}
				}
				List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					if (allDefsListForReading[j].recipeUsers != null && allDefsListForReading[j].recipeUsers.Contains(this))
					{
						allRecipesCached.Add(allDefsListForReading[j]);
					}
				}
			}
			return allRecipesCached;
		}
	}

	public bool ConnectToPower
	{
		get
		{
			if (EverTransmitsPower)
			{
				return false;
			}
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].compClass == typeof(CompPowerBattery))
				{
					return true;
				}
				if (comps[i].compClass == typeof(CompPowerTrader))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool CoexistsWithFloors
	{
		get
		{
			if (!neverOverlapFloors)
			{
				return !coversFloor;
			}
			return false;
		}
	}

	public FillCategory Fillage
	{
		get
		{
			if (fillPercent < 0.01f)
			{
				return FillCategory.None;
			}
			if (fillPercent > 0.99f)
			{
				return FillCategory.Full;
			}
			return FillCategory.Partial;
		}
	}

	public bool MakeFog => Fillage == FillCategory.Full;

	public bool CanOverlapZones
	{
		get
		{
			if (building != null && building.SupportsPlants)
			{
				return false;
			}
			if (passability == Traversability.Impassable && category != ThingCategory.Plant && !HasComp(typeof(CompTransporter)))
			{
				return false;
			}
			if ((int)surfaceType >= 1)
			{
				return false;
			}
			if (typeof(ISlotGroupParent).IsAssignableFrom(thingClass))
			{
				return false;
			}
			if (!canOverlapZones)
			{
				return false;
			}
			if ((IsBlueprint || IsFrame) && entityDefToBuild is ThingDef thingDef)
			{
				return thingDef.CanOverlapZones;
			}
			return true;
		}
	}

	public bool CountAsResource => resourceReadoutPriority != ResourceCountPriority.Uncounted;

	public List<VerbProperties> Verbs
	{
		get
		{
			if (verbs != null)
			{
				return verbs;
			}
			return EmptyVerbPropertiesList;
		}
	}

	public bool CanHaveFaction
	{
		get
		{
			if (IsBlueprint || IsFrame)
			{
				return true;
			}
			return category switch
			{
				ThingCategory.Pawn => true, 
				ThingCategory.Building => true, 
				_ => false, 
			};
		}
	}

	public bool Claimable
	{
		get
		{
			if (building != null && building.claimable)
			{
				return !building.isNaturalRock;
			}
			return false;
		}
	}

	public ThingCategoryDef FirstThingCategory
	{
		get
		{
			if (thingCategories.NullOrEmpty())
			{
				return null;
			}
			return thingCategories[0];
		}
	}

	public float MedicineTendXpGainFactor => Mathf.Clamp(this.GetStatValueAbstract(StatDefOf.MedicalPotency) * 0.7f, 0.5f, 1f);

	public bool CanEverDeteriorate
	{
		get
		{
			if (!useHitPoints)
			{
				return false;
			}
			if (category != ThingCategory.Item)
			{
				if (plant != null)
				{
					return plant.canDeteriorate;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanInteractThroughCorners
	{
		get
		{
			if (canInteractThroughCorners)
			{
				return true;
			}
			if (category != ThingCategory.Building)
			{
				return false;
			}
			if (!holdsRoof)
			{
				return false;
			}
			if (building != null && building.isNaturalRock && !IsSmoothed)
			{
				return false;
			}
			return true;
		}
	}

	public bool AffectsRegions
	{
		get
		{
			if (passability != Traversability.Impassable && !IsDoor)
			{
				return IsFence;
			}
			return true;
		}
	}

	public bool AffectsReachability
	{
		get
		{
			if (AffectsRegions)
			{
				return true;
			}
			if (passability == Traversability.Impassable || IsDoor)
			{
				return true;
			}
			if (TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(this))
			{
				return true;
			}
			return false;
		}
	}

	public string DescriptionDetailed
	{
		get
		{
			if (descriptionDetailedCached == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(description);
				if (IsApparel)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(string.Format("{0}: {1}", "Layer".Translate(), apparel.GetLayersString()));
					stringBuilder.Append(string.Format("{0}: {1}", "Covers".Translate(), apparel.GetCoveredOuterPartsString(BodyDefOf.Human)));
					if (equippedStatOffsets != null && equippedStatOffsets.Count > 0)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
						for (int i = 0; i < equippedStatOffsets.Count; i++)
						{
							if (i > 0)
							{
								stringBuilder.AppendLine();
							}
							StatModifier statModifier = equippedStatOffsets[i];
							stringBuilder.Append($"{statModifier.stat.LabelCap}: {statModifier.ValueToStringAsOffset}");
						}
					}
				}
				descriptionDetailedCached = stringBuilder.ToString();
			}
			return descriptionDetailedCached;
		}
	}

	public bool CanBenefitFromCover
	{
		get
		{
			if (category == ThingCategory.Pawn)
			{
				return true;
			}
			if (building != null && building.IsTurret)
			{
				return true;
			}
			return false;
		}
	}

	public bool PotentiallySmeltable
	{
		get
		{
			if (!smeltable)
			{
				return false;
			}
			if (base.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(this))
				{
					if (item.smeltable)
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
	}

	public bool HasSingleOrMultipleInteractionCells
	{
		get
		{
			if (!hasInteractionCell)
			{
				return !multipleInteractionCellOffsets.NullOrEmpty();
			}
			return true;
		}
	}

	public bool IsApparel => apparel != null;

	public bool IsBed => typeof(Building_Bed).IsAssignableFrom(thingClass);

	public bool IsWall
	{
		get
		{
			if (building != null)
			{
				return building.isWall;
			}
			return false;
		}
	}

	public bool IsCorpse => typeof(Corpse).IsAssignableFrom(thingClass);

	public bool IsFrame => isFrameInt;

	public bool IsBlueprint
	{
		get
		{
			if (entityDefToBuild != null)
			{
				return category == ThingCategory.Ethereal;
			}
			return false;
		}
	}

	public bool IsStuff => stuffProps != null;

	public bool IsMedicine => statBases.StatListContains(StatDefOf.MedicalPotency);

	public bool IsDoor => typeof(Building_Door).IsAssignableFrom(thingClass);

	public bool IsFence
	{
		get
		{
			if (building != null)
			{
				return building.isFence;
			}
			return false;
		}
	}

	public bool IsFilth => filth != null;

	public bool IsIngestible => ingestible != null;

	public bool IsNutritionGivingIngestible
	{
		get
		{
			if (IsIngestible)
			{
				return ingestible.CachedNutrition > 0f;
			}
			return false;
		}
	}

	public bool IsNutritionGivingIngestibleForHumanlikeBabies
	{
		get
		{
			if (IsNutritionGivingIngestible && ingestible.HumanEdible)
			{
				return ingestible.babiesCanIngest;
			}
			return false;
		}
	}

	public bool IsWeapon
	{
		get
		{
			if (category == ThingCategory.Item && (!verbs.NullOrEmpty() || !tools.NullOrEmpty()))
			{
				return !IsApparel;
			}
			return false;
		}
	}

	public bool IsCommsConsole => typeof(Building_CommsConsole).IsAssignableFrom(thingClass);

	public bool IsOrbitalTradeBeacon => typeof(Building_OrbitalTradeBeacon).IsAssignableFrom(thingClass);

	public bool IsFoodDispenser => typeof(Building_NutrientPasteDispenser).IsAssignableFrom(thingClass);

	public bool IsDrug
	{
		get
		{
			if (ingestible != null)
			{
				return ingestible.drugCategory != DrugCategory.None;
			}
			return false;
		}
	}

	public bool IsPleasureDrug
	{
		get
		{
			if (IsDrug)
			{
				return ingestible.joy > 0f;
			}
			return false;
		}
	}

	public bool IsNonMedicalDrug
	{
		get
		{
			if (IsDrug)
			{
				return ingestible.drugCategory != DrugCategory.Medical;
			}
			return false;
		}
	}

	public bool IsTable
	{
		get
		{
			if (surfaceType == SurfaceType.Eat)
			{
				return HasComp(typeof(CompGatherSpot));
			}
			return false;
		}
	}

	public bool IsWorkTable => typeof(Building_WorkTable).IsAssignableFrom(thingClass);

	public bool IsShell => projectileWhenLoaded != null;

	public bool IsArt => IsWithinCategory(ThingCategoryDefOf.BuildingsArt);

	public bool IsSmoothable => building?.smoothedThing != null;

	public bool IsSmoothed => building?.unsmoothedThing != null;

	public bool IsMetal
	{
		get
		{
			if (stuffProps != null)
			{
				return stuffProps.categories.Contains(StuffCategoryDefOf.Metallic);
			}
			return false;
		}
	}

	public bool IsCryptosleepCasket => typeof(Building_CryptosleepCasket).IsAssignableFrom(thingClass);

	public bool IsGibbetCage => typeof(Building_GibbetCage).IsAssignableFrom(thingClass);

	public bool IsMechGestator => typeof(Building_MechGestator).IsAssignableFrom(thingClass);

	public bool IsMechRecharger => typeof(Building_MechCharger).IsAssignableFrom(thingClass);

	public bool IsAddictiveDrug
	{
		get
		{
			CompProperties_Drug compProperties = GetCompProperties<CompProperties_Drug>();
			if (compProperties != null)
			{
				return compProperties.addictiveness > 0f;
			}
			return false;
		}
	}

	public bool IsMeat
	{
		get
		{
			if (category == ThingCategory.Item && thingCategories != null)
			{
				return thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
			}
			return false;
		}
	}

	public bool IsEgg
	{
		get
		{
			if (category == ThingCategory.Item && thingCategories != null)
			{
				if (!thingCategories.Contains(ThingCategoryDefOf.EggsFertilized))
				{
					return thingCategories.Contains(ThingCategoryDefOf.EggsUnfertilized);
				}
				return true;
			}
			return false;
		}
	}

	public bool IsLeather
	{
		get
		{
			if (category == ThingCategory.Item && thingCategories != null)
			{
				return thingCategories.Contains(ThingCategoryDefOf.Leathers);
			}
			return false;
		}
	}

	public bool IsWool
	{
		get
		{
			if (category == ThingCategory.Item && thingCategories != null)
			{
				return thingCategories.Contains(ThingCategoryDefOf.Wools);
			}
			return false;
		}
	}

	public bool IsRangedWeapon
	{
		get
		{
			if (!IsWeapon)
			{
				return false;
			}
			if (!verbs.NullOrEmpty())
			{
				for (int i = 0; i < verbs.Count; i++)
				{
					if (!verbs[i].IsMeleeAttack)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool IsMeleeWeapon
	{
		get
		{
			if (IsWeapon)
			{
				return !IsRangedWeapon;
			}
			return false;
		}
	}

	public bool IsWeaponUsingProjectiles
	{
		get
		{
			if (!IsWeapon)
			{
				return false;
			}
			if (!verbs.NullOrEmpty())
			{
				for (int i = 0; i < verbs.Count; i++)
				{
					if (verbs[i].LaunchesProjectile)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool IsShieldThatBlocksRanged
	{
		get
		{
			if (HasComp(typeof(CompShield)))
			{
				return GetCompProperties<CompProperties_Shield>().blocksRangedWeapons;
			}
			return false;
		}
	}

	public bool IsBuildingArtificial
	{
		get
		{
			if (category == ThingCategory.Building || IsFrame)
			{
				if (building != null)
				{
					if (!building.isNaturalRock)
					{
						return !building.isResourceRock;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsNonResourceNaturalRock
	{
		get
		{
			if (category == ThingCategory.Building && building.isNaturalRock && !building.isResourceRock && !building.mineablePreventNaturalRockOnSurface)
			{
				return !IsSmoothed;
			}
			return false;
		}
	}

	public bool HasSunShadows
	{
		get
		{
			if (!hasSunShadowsCached.HasValue)
			{
				hasSunShadowsCached = typeof(Pawn).IsAssignableFrom(thingClass);
			}
			return hasSunShadowsCached.Value;
		}
	}

	public bool IsNaturalOrgan
	{
		get
		{
			if (!isNaturalOrganCached.HasValue)
			{
				if (category != ThingCategory.Item)
				{
					isNaturalOrganCached = false;
				}
				else
				{
					List<BodyPartDef> allDefsListForReading = DefDatabase<BodyPartDef>.AllDefsListForReading;
					isNaturalOrganCached = false;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].spawnThingOnRemoved == this)
						{
							isNaturalOrganCached = true;
							break;
						}
					}
				}
			}
			return isNaturalOrganCached.Value;
		}
	}

	public bool IsFungus
	{
		get
		{
			if (ingestible != null)
			{
				return ingestible.foodType.HasFlag(FoodTypeFlags.Fungus);
			}
			return false;
		}
	}

	public bool IsAnimalProduct
	{
		get
		{
			if (ingestible != null)
			{
				return ingestible.foodType.HasFlag(FoodTypeFlags.AnimalProduct);
			}
			return false;
		}
	}

	public bool IsProcessedFood
	{
		get
		{
			if (ingestible != null)
			{
				return ingestible.foodType.HasFlag(FoodTypeFlags.Processed);
			}
			return false;
		}
	}

	public bool CanAffectLinker
	{
		get
		{
			if (graphicData == null || !graphicData.Linked)
			{
				return IsDoor;
			}
			return true;
		}
	}

	public bool IsNonDeconstructibleAttackableBuilding
	{
		get
		{
			if (IsBuildingArtificial && !building.IsDeconstructible && destroyable && !mineable && building.isTargetable)
			{
				return building.draftAttackNonDeconstructable;
			}
			return false;
		}
	}

	public bool IsPlant => typeof(Plant).IsAssignableFrom(thingClass);

	public bool IsDeadPlant => typeof(DeadPlant).IsAssignableFrom(thingClass);

	public bool IsStudiable => HasAssignableCompFrom(typeof(CompStudiable));

	public List<StyleCategoryDef> RelevantStyleCategories
	{
		get
		{
			if (cachedRelevantStyleCategories == null)
			{
				cachedRelevantStyleCategories = new List<StyleCategoryDef>();
				foreach (StyleCategoryDef allDef in DefDatabase<StyleCategoryDef>.AllDefs)
				{
					if (allDef.thingDefStyles.NullOrEmpty())
					{
						continue;
					}
					foreach (ThingDefStyle thingDefStyle in allDef.thingDefStyles)
					{
						if (thingDefStyle.ThingDef == this)
						{
							cachedRelevantStyleCategories.Add(allDef);
							break;
						}
					}
				}
			}
			return cachedRelevantStyleCategories;
		}
	}

	public string LabelAsStuff
	{
		get
		{
			if (!stuffProps.stuffAdjective.NullOrEmpty())
			{
				return stuffProps.stuffAdjective;
			}
			return label;
		}
	}

	public bool BlocksPlanting(bool canWipePlants = false)
	{
		if (building != null && building.SupportsPlants)
		{
			return false;
		}
		if (building != null && building.isAttachment)
		{
			return false;
		}
		if (blockPlants)
		{
			return true;
		}
		if (!canWipePlants && category == ThingCategory.Plant)
		{
			return true;
		}
		if ((int)Fillage > 0)
		{
			return true;
		}
		if (this.IsEdifice())
		{
			return true;
		}
		return false;
	}

	public virtual bool CanSpawnAt(IntVec3 pos, Rot4 rot, Map map)
	{
		return true;
	}

	public bool EverStorable(bool willMinifyIfPossible)
	{
		if (typeof(MinifiedThing).IsAssignableFrom(thingClass))
		{
			return true;
		}
		if (!thingCategories.NullOrEmpty())
		{
			if (category == ThingCategory.Item)
			{
				return true;
			}
			if (willMinifyIfPossible && Minifiable)
			{
				return true;
			}
		}
		return false;
	}

	public Thing GetConcreteExample(ThingDef stuff = null)
	{
		if (concreteExamplesInt == null)
		{
			concreteExamplesInt = new Dictionary<ThingDef, Thing>();
		}
		if (stuff == null)
		{
			stuff = ThingDefOf.Steel;
		}
		if (!concreteExamplesInt.ContainsKey(stuff))
		{
			if (race == null)
			{
				concreteExamplesInt[stuff] = ThingMaker.MakeThing(this, base.MadeFromStuff ? stuff : null);
			}
			else
			{
				concreteExamplesInt[stuff] = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefsListForReading.FirstOrDefault((PawnKindDef pkd) => pkd.race == this));
			}
		}
		return concreteExamplesInt[stuff];
	}

	public CompProperties CompDefFor<T>() where T : ThingComp
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (comps[i].compClass == typeof(T))
			{
				return comps[i];
			}
		}
		return null;
	}

	public CompProperties CompDefForAssignableFrom<T>() where T : ThingComp
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (typeof(T).IsAssignableFrom(comps[i].compClass))
			{
				return comps[i];
			}
		}
		return null;
	}

	public bool HasComp(Type compType)
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (comps[i].compClass == compType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasComp<T>() where T : ThingComp
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (comps[i].compClass == typeof(T) || typeof(T).IsAssignableFrom(comps[i].compClass))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAssignableCompFrom(Type compType)
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (compType.IsAssignableFrom(comps[i].compClass))
			{
				return true;
			}
		}
		return false;
	}

	public T GetCompProperties<T>() where T : CompProperties
	{
		for (int i = 0; i < comps.Count; i++)
		{
			if (comps[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public override void PostLoad()
	{
		if (graphicData != null)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				GraphicData graphicData = this.graphicData;
				if (graphicData.shaderType == null)
				{
					graphicData.shaderType = ShaderTypeDefOf.Cutout;
				}
				ContentFinderRequester.requester = this;
				try
				{
					graphic = this.graphicData.Graphic;
					if (drawerType != DrawerType.RealtimeOnly)
					{
						TextureAtlasGroup textureAtlasGroup = category.ToAtlasGroup();
						graphic.TryInsertIntoAtlas(textureAtlasGroup);
						if (textureAtlasGroup == TextureAtlasGroup.Building && Minifiable)
						{
							graphic.TryInsertIntoAtlas(TextureAtlasGroup.Item);
						}
					}
				}
				finally
				{
					ContentFinderRequester.requester = null;
				}
			});
		}
		if (tools != null)
		{
			for (int num = 0; num < tools.Count; num++)
			{
				tools[num].id = num.ToString();
			}
		}
		if (verbs != null && verbs.Count == 1 && verbs[0].label.NullOrEmpty())
		{
			verbs[0].label = label;
		}
		base.PostLoad();
		if (category == ThingCategory.Building && building == null)
		{
			building = new BuildingProperties();
		}
		building?.PostLoadSpecial(this);
		apparel?.PostLoadSpecial(this);
		plant?.PostLoadSpecial(this);
		if (comps == null)
		{
			return;
		}
		foreach (CompProperties comp in comps)
		{
			comp.PostLoadSpecial(this);
		}
	}

	protected override void ResolveIcon()
	{
		base.ResolveIcon();
		if (category == ThingCategory.Pawn)
		{
			if (!uiIconPath.NullOrEmpty())
			{
				uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
			}
			else
			{
				if (race.Humanlike)
				{
					return;
				}
				PawnKindDef anyPawnKind = race.AnyPawnKind;
				if (anyPawnKind != null)
				{
					Material material = ((ModsConfig.BiotechActive && anyPawnKind.RaceProps.IsMechanoid) ? anyPawnKind.lifeStages.First() : anyPawnKind.lifeStages.Last()).bodyGraphicData.Graphic.MatAt(Rot4.East);
					uiIcon = (Texture2D)material.mainTexture;
					uiIconColor = material.color;
					if (ShaderDatabase.TryGetUIShader(material.shader, out var uiShader) && MaterialPool.TryGetRequestForMat(material, out var request))
					{
						request.shader = uiShader;
						uiIconMaterial = MaterialPool.MatFrom(request);
					}
				}
			}
		}
		else
		{
			ThingDef thingDef = GenStuff.DefaultStuffFor(this);
			if (colorGenerator != null && (thingDef == null || thingDef.stuffProps.allowColorGenerators))
			{
				uiIconColor = colorGenerator.ExemplaryColor;
			}
			else if (thingDef != null)
			{
				uiIconColor = GetColorForStuff(thingDef);
			}
			else if (graphicData != null)
			{
				uiIconColor = graphicData.color;
			}
			if (rotatable && graphic != null && graphic != BaseContent.BadGraphic && graphic.ShouldDrawRotated && defaultPlacingRot == Rot4.South)
			{
				uiIconAngle = 180f + graphic.DrawRotatedExtraAngleOffset;
			}
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (ingestible != null)
		{
			ingestible.parent = this;
		}
		if (stuffProps != null)
		{
			stuffProps.parent = this;
		}
		building?.ResolveReferencesSpecial();
		graphicData?.ResolveReferencesSpecial();
		race?.ResolveReferencesSpecial();
		stuffProps?.ResolveReferencesSpecial();
		apparel?.ResolveReferencesSpecial();
		if (soundImpactDefault == null)
		{
			soundImpactDefault = SoundDefOf.BulletImpact_Ground;
		}
		if (soundDrop == null)
		{
			soundDrop = SoundDefOf.Standard_Drop;
		}
		if (soundPickup == null)
		{
			soundPickup = SoundDefOf.Standard_Pickup;
		}
		if (soundInteract == null)
		{
			soundInteract = SoundDefOf.Standard_Pickup;
		}
		if (inspectorTabs != null && inspectorTabs.Any())
		{
			inspectorTabsResolved = new List<InspectTabBase>();
			for (int i = 0; i < inspectorTabs.Count; i++)
			{
				try
				{
					inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(inspectorTabs[i]));
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate inspector tab of type " + inspectorTabs[i]?.ToString() + ": " + ex);
				}
			}
		}
		if (comps != null)
		{
			for (int j = 0; j < comps.Count; j++)
			{
				comps[j].ResolveReferences(this);
			}
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (category != ThingCategory.Ethereal && label.NullOrEmpty())
		{
			yield return "no label";
		}
		if (category == ThingCategory.Building && !IsFrame && building.IsDeconstructible && thingClass != null && typeof(Building).IsSubclassOf(thingClass))
		{
			yield return "has building category and is marked as deconstructible, but thing class is not a subclass of building (" + thingClass.Name + ")";
		}
		if (graphicData != null)
		{
			foreach (string item2 in graphicData.ConfigErrors(this))
			{
				yield return item2;
			}
		}
		if (projectile != null)
		{
			foreach (string item3 in projectile.ConfigErrors(this))
			{
				yield return item3;
			}
		}
		if (statBases != null)
		{
			foreach (StatModifier statBase in statBases)
			{
				if (statBases.Count((StatModifier st) => st.stat == statBase.stat) > 1)
				{
					yield return "defines the stat base " + statBase.stat?.ToString() + " more than once.";
				}
			}
		}
		if (!BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.Beauty))
		{
			yield return "Beauty stat base is defined, but Things of category " + category.ToString() + " cannot have beauty.";
		}
		if (!BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.BeautyOutdoors))
		{
			yield return "BeautyOutdoors stat base is defined, but Things of category " + category.ToString() + " cannot have beauty.";
		}
		if (char.IsNumber(defName[defName.Length - 1]))
		{
			yield return "ends with a numerical digit, which is not allowed on ThingDefs.";
		}
		if (thingClass == null)
		{
			yield return "has null thingClass.";
		}
		if (comps.Count > 0 && !typeof(ThingWithComps).IsAssignableFrom(thingClass))
		{
			yield return "has components but it's thingClass is not a ThingWithComps";
		}
		if (ConnectToPower && drawerType == DrawerType.RealtimeOnly && IsFrame)
		{
			yield return "connects to power but does not add to map mesh. Will not create wire meshes.";
		}
		if (costList != null)
		{
			foreach (ThingDefCountClass cost in costList)
			{
				if (cost.count == 0)
				{
					yield return "cost in " + cost.thingDef?.ToString() + " is zero.";
				}
			}
		}
		ThingCategoryDef thingCategoryDef = thingCategories?.FirstOrDefault((ThingCategoryDef cat) => thingCategories.Count((ThingCategoryDef c) => c == cat) > 1);
		if (thingCategoryDef != null)
		{
			yield return "has duplicate thingCategory " + thingCategoryDef?.ToString() + ".";
		}
		if (Fillage == FillCategory.Full && category != ThingCategory.Building)
		{
			yield return "gives full cover but is not a building.";
		}
		if (equipmentType != EquipmentType.None)
		{
			if (techLevel == TechLevel.Undefined && !destroyOnDrop)
			{
				yield return "is equipment but has no tech level.";
			}
			if (!comps.Any((CompProperties c) => typeof(CompEquippable).IsAssignableFrom(c.compClass)))
			{
				yield return "is equipment but has no CompEquippable";
			}
		}
		if (thingClass == typeof(Bullet) && projectile.damageDef == null)
		{
			yield return " is a bullet but has no damageDef.";
		}
		if (destroyOnDrop && tradeability != Tradeability.None)
		{
			yield return "destroyOnDrop but tradeability is " + tradeability;
		}
		if (stackLimit > 1 && !drawGUIOverlay)
		{
			yield return "has stackLimit > 1 but also has drawGUIOverlay = false.";
		}
		if (damageMultipliers != null)
		{
			foreach (DamageMultiplier mult in damageMultipliers)
			{
				if (damageMultipliers.Count((DamageMultiplier m) => m.damageDef == mult.damageDef) > 1)
				{
					yield return "has multiple damage multipliers for damageDef " + mult.damageDef;
					break;
				}
			}
		}
		if (Fillage == FillCategory.Full && !this.IsEdifice())
		{
			yield return "fillPercent is 1.00 but is not edifice";
		}
		if (base.MadeFromStuff && constructEffect != null)
		{
			yield return "madeFromStuff but has a defined constructEffect (which will always be overridden by stuff's construct animation).";
		}
		if (base.MadeFromStuff && stuffCategories.NullOrEmpty())
		{
			yield return "madeFromStuff but has no stuffCategories.";
		}
		if (costList.NullOrEmpty() && costStuffCount <= 0 && recipeMaker != null)
		{
			yield return "has a recipeMaker but no costList or costStuffCount.";
		}
		if (costStuffCount > 0 && stuffCategories.NullOrEmpty())
		{
			yield return "has costStuffCount but no stuffCategories.";
		}
		if (this.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 1E-05f && !CanEverDeteriorate && !destroyOnDrop)
		{
			yield return "has >0 DeteriorationRate but can't deteriorate.";
		}
		if (smeltProducts != null && !smeltable)
		{
			yield return "has smeltProducts but has smeltable=false";
		}
		if (smeltable && smeltProducts.NullOrEmpty() && base.CostList.NullOrEmpty() && !IsStuff && !base.MadeFromStuff && !destroyOnDrop)
		{
			yield return "is smeltable but does not give anything for smelting.";
		}
		if (equipmentType != EquipmentType.None && verbs.NullOrEmpty() && tools.NullOrEmpty())
		{
			yield return "is equipment but has no verbs or tools";
		}
		if (Minifiable && thingCategories.NullOrEmpty())
		{
			yield return "is minifiable but not in any thing category";
		}
		if (category == ThingCategory.Building && !Minifiable && !thingCategories.NullOrEmpty())
		{
			yield return "is not minifiable yet has thing categories (could be confusing in thing filters because it can't be moved/stored anyway)";
		}
		if (!destroyOnDrop && !typeof(MinifiedThing).IsAssignableFrom(thingClass) && (EverHaulable || Minifiable) && (statBases.NullOrEmpty() || !statBases.Any((StatModifier s) => s.stat == StatDefOf.Mass)))
		{
			yield return "is haulable, but does not have an authored mass value";
		}
		if (ingestible == null && this.GetStatValueAbstract(StatDefOf.Nutrition) != 0f)
		{
			yield return "has nutrition but ingestible properties are null";
		}
		if (BaseFlammability != 0f && !useHitPoints && category != ThingCategory.Pawn && !destroyOnDrop)
		{
			yield return "flammable but has no hitpoints (will burn indefinitely)";
		}
		if (graphicData?.shadowData != null && staticSunShadowHeight > 0f)
		{
			yield return "graphicData defines a shadowInfo but staticSunShadowHeight > 0";
		}
		if (saveCompressible && Claimable)
		{
			yield return "claimable item is compressible; faction will be unset after load";
		}
		if (deepCommonality > 0f != deepLumpSizeRange.TrueMax > 0)
		{
			yield return "if deepCommonality or deepLumpSizeRange is set, the other also must be set";
		}
		if (deepCommonality > 0f && deepCountPerPortion <= 0)
		{
			yield return "deepCommonality > 0 but deepCountPerPortion is not set";
		}
		if (verbs != null)
		{
			for (int i = 0; i < verbs.Count; i++)
			{
				foreach (string item4 in verbs[i].ConfigErrors(this))
				{
					yield return $"verb {i}: {item4}";
				}
			}
		}
		if (building != null)
		{
			foreach (string item5 in building.ConfigErrors(this))
			{
				yield return item5;
			}
			if ((building.isAirtight || building.isStuffableAirtight) && Fillage != FillCategory.Full)
			{
				yield return "is airtight but Fillage is not Full";
			}
		}
		if (apparel != null)
		{
			foreach (string item6 in apparel.ConfigErrors(this))
			{
				yield return item6;
			}
		}
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (string item7 in comps[i].ConfigErrors(this))
				{
					yield return item7;
				}
			}
		}
		if (race != null)
		{
			foreach (string item8 in race.ConfigErrors(this))
			{
				yield return item8;
			}
			if (race.body != null && race != null && tools != null)
			{
				int i2;
				for (i2 = 0; i2 < tools.Count; i2++)
				{
					if (tools[i2].linkedBodyPartsGroup != null && !race.body.AllParts.Any((BodyPartRecord part) => part.groups.Contains(tools[i2].linkedBodyPartsGroup)))
					{
						yield return "has tool with linkedBodyPartsGroup " + tools[i2].linkedBodyPartsGroup?.ToString() + " but body " + race.body?.ToString() + " has no parts with that group.";
					}
				}
			}
			if (race.Animal && this.GetStatValueAbstract(StatDefOf.Wildness) < 0f)
			{
				yield return "is animal but wildness is not defined";
			}
		}
		if (ingestible != null)
		{
			foreach (string item9 in ingestible.ConfigErrors())
			{
				yield return item9;
			}
		}
		if (plant != null)
		{
			foreach (string item10 in plant.ConfigErrors())
			{
				yield return item10;
			}
		}
		if (tools != null)
		{
			Tool tool = tools.SelectMany((Tool lhs) => tools.Where((Tool rhs) => lhs != rhs && lhs.id == rhs.id)).FirstOrDefault();
			if (tool != null)
			{
				yield return "duplicate thingdef tool id " + tool.id;
			}
			foreach (Tool tool2 in tools)
			{
				foreach (string item11 in tool2.ConfigErrors())
				{
					yield return item11;
				}
			}
		}
		if (!randomStyle.NullOrEmpty())
		{
			foreach (ThingStyleChance item12 in randomStyle)
			{
				if (item12.Chance <= 0f)
				{
					yield return "style chance <= 0.";
				}
			}
			if (!comps.Any((CompProperties c) => c.compClass == typeof(CompStyleable)))
			{
				yield return "random style assigned, but missing CompStyleable!";
			}
		}
		if (relicChance > 0f && category != ThingCategory.Item)
		{
			yield return "relic chance > 0 but category != item";
		}
		if (hasInteractionCell && !multipleInteractionCellOffsets.NullOrEmpty())
		{
			yield return "both single and multiple interaction cells are defined, it should be one or the other";
		}
		if (Fillage != FillCategory.Full && passability == Traversability.Impassable && !IsDoor && base.BuildableByPlayer && !disableImpassableShotOverConfigError)
		{
			yield return "impassable, player-buildable building that can be shot/seen over.";
		}
	}

	public static ThingDef Named(string defName)
	{
		return DefDatabase<ThingDef>.GetNamed(defName);
	}

	public bool IsWithinCategory(ThingCategoryDef category)
	{
		if (thingCategories == null)
		{
			return false;
		}
		for (int i = 0; i < thingCategories.Count; i++)
		{
			for (ThingCategoryDef thingCategoryDef = thingCategories[i]; thingCategoryDef != null; thingCategoryDef = thingCategoryDef.parent)
			{
				if (thingCategoryDef == category)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Notify_UnlockedByResearch()
	{
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].Notify_PostUnlockedByResearch(this);
			}
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (apparel != null)
		{
			string coveredOuterPartsString = apparel.GetCoveredOuterPartsString(BodyDefOf.Human);
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Covers".Translate(), coveredOuterPartsString, "Stat_Thing_Apparel_Covers_Desc".Translate(), 2750);
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Layer".Translate(), apparel.GetLayersString(), "Stat_Thing_Apparel_Layer_Desc".Translate(), 2751);
			yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_CountsAsClothingNudity_Name".Translate(), apparel.countsAsClothingForNudity ? "Yes".Translate() : "No".Translate(), "Stat_Thing_Apparel_CountsAsClothingNudity_Desc".Translate(), 2753);
			if (ModsConfig.BiotechActive)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_ValidLifestage".Translate(), apparel.developmentalStageFilter.ToCommaList().CapitalizeFirst(), "Stat_Thing_Apparel_ValidLifestage_Desc".Translate(), 2748);
			}
			if (apparel.gender != Gender.None)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_Gender".Translate(), apparel.gender.GetLabel().CapitalizeFirst(), "Stat_Thing_Apparel_Gender_Desc".Translate(), 2749);
			}
		}
		if (IsMedicine && MedicineTendXpGainFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MedicineXpGainFactor".Translate(), MedicineTendXpGainFactor.ToStringPercent(), "Stat_Thing_Drug_MedicineXpGainFactor_Desc".Translate(), 1000);
		}
		if (fillPercent > 0f && (category == ThingCategory.Item || category == ThingCategory.Building || category == ThingCategory.Plant))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CoverEffectiveness".Translate(), this.BaseBlockChance().ToStringPercent(), "CoverEffectivenessExplanation".Translate(), 2000);
		}
		if (constructionSkillPrerequisite > 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequiredToBuild".Translate(SkillDefOf.Construction.LabelCap), constructionSkillPrerequisite.ToString(), "SkillRequiredToBuildExplanation".Translate(SkillDefOf.Construction.LabelCap), 1100);
		}
		if (artisticSkillPrerequisite > 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequiredToBuild".Translate(SkillDefOf.Artistic.LabelCap), artisticSkillPrerequisite.ToString(), "SkillRequiredToBuildExplanation".Translate(SkillDefOf.Artistic.LabelCap), 1100);
		}
		IEnumerable<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.products.Count == 1 && r.products.Any((ThingDefCountClass thingDefCountClass) => thingDefCountClass.thingDef == this) && !r.IsSurgery);
		if (recipes.Any())
		{
			IEnumerable<string> enumerable = (from u in recipes.Where((RecipeDef x) => x.recipeUsers != null).SelectMany((RecipeDef r) => r.recipeUsers)
				select u.label).Concat(from x in DefDatabase<ThingDef>.AllDefsListForReading
				where x.recipes != null && x.recipes.Any((RecipeDef y) => y.products.Any((ThingDefCountClass z) => z.thingDef == this))
				select x.label).Distinct();
			if (enumerable.Any())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CreatedAt".Translate(), enumerable.ToCommaList().CapitalizeFirst(), "Stat_Thing_CreatedAt_Desc".Translate(), 1103);
			}
			RecipeDef recipeDef = recipes.FirstOrDefault();
			if (recipeDef != null && !recipeDef.ingredients.NullOrEmpty())
			{
				BuildableDef.tmpCostList.Clear();
				BuildableDef.tmpHyperlinks.Clear();
				for (int num = 0; num < recipeDef.ingredients.Count; num++)
				{
					IngredientCount ingredientCount = recipeDef.ingredients[num];
					if (ingredientCount.filter.Summary.NullOrEmpty())
					{
						continue;
					}
					IEnumerable<ThingDef> allowedThingDefs = ingredientCount.filter.AllowedThingDefs;
					if (allowedThingDefs.Any())
					{
						foreach (ThingDef p in allowedThingDefs)
						{
							if (!BuildableDef.tmpHyperlinks.Any((Dialog_InfoCard.Hyperlink x) => x.def == p))
							{
								BuildableDef.tmpHyperlinks.Add(new Dialog_InfoCard.Hyperlink(p));
							}
						}
					}
					BuildableDef.tmpCostList.Add(recipeDef.IngredientValueGetter.BillRequirementsDescription(recipeDef, ingredientCount));
				}
			}
			if (BuildableDef.tmpCostList.Any())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Ingredients".Translate(), BuildableDef.tmpCostList.ToCommaList(), "Stat_Thing_Ingredients".Translate(), 1102, null, BuildableDef.tmpHyperlinks);
			}
		}
		if (thingClass != null && typeof(Building_Bed).IsAssignableFrom(thingClass) && !statBases.StatListContains(StatDefOf.BedRestEffectiveness))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, StatDefOf.BedRestEffectiveness, StatDefOf.BedRestEffectiveness.valueIfMissing, StatRequest.ForEmpty());
		}
		if (!verbs.NullOrEmpty())
		{
			VerbProperties verb = verbs.First((VerbProperties x) => x.isPrimary);
			StatCategoryDef verbStatCategory = ((category == ThingCategory.Pawn) ? StatCategoryDefOf.PawnCombat : null);
			float num2 = verb.warmupTime;
			StringBuilder stringBuilder = new StringBuilder("Stat_Thing_Weapon_RangedWarmupTime_Desc".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + num2.ToString("0.##") + " " + "LetterSecond".Translate());
			if (num2 > 0f)
			{
				if (req.HasThing)
				{
					float statValue = req.Thing.GetStatValue(StatDefOf.RangedWeapon_WarmupMultiplier);
					num2 *= statValue;
					if (!Mathf.Approximately(statValue, 1f))
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("Stat_Thing_Weapon_WarmupTime_Multiplier".Translate() + ": x" + statValue.ToStringPercent());
						stringBuilder.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_WarmupMultiplier, req.Thing));
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_FinalValue".Translate() + ": " + num2.ToString("0.##") + " " + "LetterSecond".Translate());
				yield return new StatDrawEntry(verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged, "RangedWarmupTime".Translate(), num2.ToString("0.##") + " " + "LetterSecond".Translate(), stringBuilder.ToString(), 3555);
			}
			if (verb.defaultProjectile?.projectile.damageDef != null && verb.defaultProjectile.projectile.damageDef.harmsHealth)
			{
				StatCategoryDef statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("Stat_Thing_Damage_Desc".Translate());
				stringBuilder2.AppendLine();
				float num3 = verb.defaultProjectile.projectile.GetDamageAmount(req.Thing, stringBuilder2);
				yield return new StatDrawEntry(statCat, "Damage".Translate(), num3.ToString(), stringBuilder2.ToString(), 5500);
				if (verb.defaultProjectile.projectile.damageDef.armorCategory != null)
				{
					StringBuilder stringBuilder3 = new StringBuilder();
					float armorPenetration = verb.defaultProjectile.projectile.GetArmorPenetration(req.Thing, stringBuilder3);
					TaggedString taggedString = "ArmorPenetrationExplanation".Translate();
					if (stringBuilder3.Length != 0)
					{
						taggedString += "\n\n" + stringBuilder3;
					}
					yield return new StatDrawEntry(statCat, "ArmorPenetration".Translate(), armorPenetration.ToStringPercent(), taggedString, 5400);
				}
				float buildingDamageFactor = verb.defaultProjectile.projectile.damageDef.buildingDamageFactor;
				float dmgBuildingsImpassable = verb.defaultProjectile.projectile.damageDef.buildingDamageFactorImpassable;
				float dmgBuildingsPassable = verb.defaultProjectile.projectile.damageDef.buildingDamageFactorPassable;
				if (buildingDamageFactor != 1f)
				{
					yield return new StatDrawEntry(statCat, "BuildingDamageFactor".Translate(), buildingDamageFactor.ToStringPercent(), "BuildingDamageFactorExplanation".Translate(), 5410);
				}
				if (dmgBuildingsImpassable != 1f)
				{
					yield return new StatDrawEntry(statCat, "BuildingDamageFactorImpassable".Translate(), dmgBuildingsImpassable.ToStringPercent(), "BuildingDamageFactorImpassableExplanation".Translate(), 5420);
				}
				if (dmgBuildingsPassable != 1f)
				{
					yield return new StatDrawEntry(statCat, "BuildingDamageFactorPassable".Translate(), dmgBuildingsPassable.ToStringPercent(), "BuildingDamageFactorPassableExplanation".Translate(), 5430);
				}
			}
			if (verb.defaultProjectile == null && verb.beamDamageDef != null)
			{
				yield return new StatDrawEntry(verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged, "ArmorPenetration".Translate(), verb.beamDamageDef.defaultArmorPenetration.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
			}
			if (verb.Ranged)
			{
				float num4 = verb.burstShotCount;
				float num5 = verb.ticksBetweenBurstShots;
				float dmgBuildingsPassable = (verb?.defaultProjectile?.projectile?.stoppingPower).GetValueOrDefault();
				StringBuilder stringBuilder4 = new StringBuilder("Stat_Thing_Weapon_BurstShotCount_Desc".Translate());
				stringBuilder4.AppendLine();
				stringBuilder4.AppendLine();
				stringBuilder4.AppendLine("StatsReport_BaseValue".Translate() + ": " + verb.burstShotCount.ToString());
				stringBuilder4.AppendLine();
				StringBuilder ticksBetweenBurstShotsExplanation = new StringBuilder("Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate());
				ticksBetweenBurstShotsExplanation.AppendLine();
				ticksBetweenBurstShotsExplanation.AppendLine();
				ticksBetweenBurstShotsExplanation.AppendLine("StatsReport_BaseValue".Translate() + ": " + (60f / verb.ticksBetweenBurstShots.TicksToSeconds()).ToString("0.##") + " rpm");
				ticksBetweenBurstShotsExplanation.AppendLine();
				StringBuilder stoppingPowerExplanation = new StringBuilder("StoppingPowerExplanation".Translate());
				stoppingPowerExplanation.AppendLine();
				stoppingPowerExplanation.AppendLine();
				stoppingPowerExplanation.AppendLine("StatsReport_BaseValue".Translate() + ": " + dmgBuildingsPassable.ToString("F1"));
				stoppingPowerExplanation.AppendLine();
				if (req.HasThing && req.Thing.TryGetComp(out CompUniqueWeapon comp))
				{
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					foreach (WeaponTraitDef item2 in comp.TraitsListForReading)
					{
						if (!Mathf.Approximately(item2.burstShotCountMultiplier, 1f))
						{
							if (!flag)
							{
								stringBuilder4.AppendLine("StatsReport_WeaponTraits".Translate() + ":");
								flag = true;
							}
							num4 *= item2.burstShotCountMultiplier;
							stringBuilder4.AppendLine("    " + item2.LabelCap + ": " + item2.burstShotCountMultiplier.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
						}
						if (!Mathf.Approximately(item2.burstShotSpeedMultiplier, 1f))
						{
							if (!flag2)
							{
								ticksBetweenBurstShotsExplanation.AppendLine("StatsReport_WeaponTraits".Translate() + ":");
								flag2 = true;
							}
							num5 /= item2.burstShotSpeedMultiplier;
							ticksBetweenBurstShotsExplanation.AppendLine("    " + item2.LabelCap + ": " + item2.burstShotSpeedMultiplier.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor));
						}
						if (!Mathf.Approximately(item2.additionalStoppingPower, 0f))
						{
							if (!flag3)
							{
								stoppingPowerExplanation.AppendLine("StatsReport_WeaponTraits".Translate() + ":");
								flag3 = true;
							}
							dmgBuildingsPassable += item2.additionalStoppingPower;
							stoppingPowerExplanation.AppendLine("    " + item2.LabelCap + ": " + item2.additionalStoppingPower.ToStringByStyle(ToStringStyle.FloatOne, ToStringNumberSense.Offset));
						}
					}
				}
				stringBuilder4.AppendLine();
				stringBuilder4.AppendLine("StatsReport_FinalValue".Translate() + ": " + Mathf.CeilToInt(num4).ToString());
				float dmgBuildingsImpassable = 60f / ((int)num5).TicksToSeconds();
				ticksBetweenBurstShotsExplanation.AppendLine();
				ticksBetweenBurstShotsExplanation.AppendLine("StatsReport_FinalValue".Translate() + ": " + dmgBuildingsImpassable.ToString("0.##") + " rpm");
				stoppingPowerExplanation.AppendLine();
				stoppingPowerExplanation.AppendLine("StatsReport_FinalValue".Translate() + ": " + dmgBuildingsPassable.ToString("F1"));
				StatCategoryDef statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;
				if (verb.showBurstShotStats && verb.burstShotCount > 1)
				{
					yield return new StatDrawEntry(statCat, "BurstShotCount".Translate(), Mathf.CeilToInt(num4).ToString(), stringBuilder4.ToString(), 5391);
					yield return new StatDrawEntry(statCat, "BurstShotFireRate".Translate(), dmgBuildingsImpassable.ToString("0.##") + " rpm", ticksBetweenBurstShotsExplanation.ToString(), 5395);
				}
				if (dmgBuildingsPassable > 0f)
				{
					yield return new StatDrawEntry(statCat, "StoppingPower".Translate(), dmgBuildingsPassable.ToString("F1"), stoppingPowerExplanation.ToString(), 5402);
				}
				float num6 = verb.range;
				StringBuilder stringBuilder5 = new StringBuilder("Stat_Thing_Weapon_Range_Desc".Translate());
				stringBuilder5.AppendLine();
				stringBuilder5.AppendLine();
				stringBuilder5.AppendLine("StatsReport_BaseValue".Translate() + ": " + num6.ToString("F0"));
				if (req.HasThing)
				{
					float statValue2 = req.Thing.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier);
					num6 *= statValue2;
					if (!Mathf.Approximately(statValue2, 1f))
					{
						stringBuilder5.AppendLine();
						stringBuilder5.AppendLine("Stat_Thing_Weapon_Range_Multiplier".Translate() + ": x" + statValue2.ToStringPercent());
						stringBuilder5.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_RangeMultiplier, req.Thing));
					}
					Map obj = req.Thing.Map ?? req.Thing.MapHeld;
					if (obj != null && obj.weatherManager.CurWeatherMaxRangeCap >= 0f)
					{
						WeatherManager weatherManager = (req.Thing.Map ?? req.Thing.MapHeld).weatherManager;
						bool num7 = num6 > weatherManager.CurWeatherMaxRangeCap;
						float num8 = num6;
						num6 = Mathf.Min(num6, weatherManager.CurWeatherMaxRangeCap);
						if (num7)
						{
							stringBuilder5.AppendLine();
							stringBuilder5.AppendLine("    " + "Stat_Thing_Weapon_Range_Clamped".Translate(num6.ToString("F0").Named("CAP"), num8.ToString("F0").Named("ORIGINAL")));
						}
					}
				}
				stringBuilder5.AppendLine();
				stringBuilder5.AppendLine("StatsReport_FinalValue".Translate() + ": " + num6.ToString("F0"));
				yield return new StatDrawEntry(statCat, "Range".Translate(), num6.ToString("F0"), stringBuilder5.ToString(), 5390);
			}
			if (verb.ForcedMissRadius > 0f)
			{
				StatCategoryDef statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;
				yield return new StatDrawEntry(statCat, "MissRadius".Translate(), verb.ForcedMissRadius.ToString("0.#"), "Stat_Thing_Weapon_MissRadius_Desc".Translate(), 3557);
				yield return new StatDrawEntry(statCat, "DirectHitChance".Translate(), (1f / (float)GenRadial.NumCellsInRadius(verb.ForcedMissRadius)).ToStringPercent(), "Stat_Thing_Weapon_DirectHitChance_Desc".Translate(), 3560);
			}
		}
		if (plant != null)
		{
			foreach (StatDrawEntry item3 in plant.SpecialDisplayStats())
			{
				yield return item3;
			}
		}
		if (ingestible != null)
		{
			foreach (StatDrawEntry item4 in ingestible.SpecialDisplayStats())
			{
				yield return item4;
			}
		}
		if (race != null)
		{
			foreach (StatDrawEntry item5 in race.SpecialDisplayStats(this, req))
			{
				yield return item5;
			}
		}
		if (building != null)
		{
			foreach (StatDrawEntry item6 in building.SpecialDisplayStats(this, req))
			{
				yield return item6;
			}
		}
		if (isTechHediff)
		{
			IEnumerable<RecipeDef> enumerable2 = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.addsHediff != null && x.IsIngredient(this));
			foreach (StatDrawEntry medicalStatsFromRecipeDef in MedicalRecipesUtility.GetMedicalStatsFromRecipeDefs(enumerable2))
			{
				yield return medicalStatsFromRecipeDef;
			}
		}
		for (int i = 0; i < comps.Count; i++)
		{
			foreach (StatDrawEntry item7 in comps[i].SpecialDisplayStats(req))
			{
				yield return item7;
			}
		}
		if (building != null)
		{
			if (building.mineableThing != null)
			{
				Dialog_InfoCard.Hyperlink[] hyperlinks = new Dialog_InfoCard.Hyperlink[1]
				{
					new Dialog_InfoCard.Hyperlink(building.mineableThing)
				};
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_MineableThing_Name".Translate(), building.mineableThing.LabelCap, "Stat_MineableThing_Desc".Translate(), 2200, null, hyperlinks);
				StringBuilder stringBuilder6 = new StringBuilder();
				stringBuilder6.AppendLine("Stat_MiningYield_Desc".Translate());
				stringBuilder6.AppendLine();
				stringBuilder6.AppendLine("StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficultyDef.label) + ": " + Find.Storyteller.difficulty.mineYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_MiningYield_Name".Translate(), Mathf.CeilToInt(building.EffectiveMineableYield).ToString("F0"), stringBuilder6.ToString(), 2200, null, hyperlinks);
			}
			if (building.IsTurret)
			{
				ThingDef turret = building.turretGunDef;
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Weapon_Name".Translate(), turret.LabelCap, "Stat_Weapon_Desc".Translate(), 5389, null, new Dialog_InfoCard.Hyperlink[1]
				{
					new Dialog_InfoCard.Hyperlink(turret)
				});
				StatRequest request = StatRequest.For(turret, null);
				foreach (StatDrawEntry item8 in turret.SpecialDisplayStats(request))
				{
					if (item8.category == StatCategoryDefOf.Weapon_Ranged)
					{
						yield return item8;
					}
				}
				for (int i = 0; i < turret.statBases.Count; i++)
				{
					StatModifier statModifier = turret.statBases[i];
					if (statModifier.stat.category == StatCategoryDefOf.Weapon_Ranged)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, statModifier.stat, statModifier.value, request);
					}
				}
			}
			if (ModsConfig.OdysseyActive && Fillage == FillCategory.Full)
			{
				bool b = building.isAirtight || (building.isStuffableAirtight && req.StuffDef.stuffProps.isAirtight);
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "Stat_Airtight".Translate(), b.ToStringYesNo(), "Stat_Airtight_Desc".Translate(), 6100);
			}
		}
		if (IsMeat)
		{
			List<ThingDef> list = new List<ThingDef>();
			bool flag4 = false;
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.race != null && allDef.race.meatDef == this && !allDef.IsCorpse)
				{
					if (!Find.HiddenItemsManager.Hidden(allDef))
					{
						flag4 = true;
					}
					list.Add(allDef);
				}
			}
			yield return new StatDrawEntry(valueString: (!flag4) ? string.Format("({0})", "NotYetDiscovered".Translate()) : string.Join(", ", (from thingDef in list
				where !Find.HiddenItemsManager.Hidden(thingDef)
				select thingDef.label).ToArray()).CapitalizeFirst(), category: StatCategoryDefOf.BasicsPawn, label: "Stat_SourceSpecies_Name".Translate(), reportText: "Stat_SourceSpecies_Desc".Translate(), displayPriorityWithinCategory: 1200, overrideReportTitle: null, hyperlinks: Dialog_InfoCard.DefsToHyperlinks(list));
		}
		if (IsLeather)
		{
			List<ThingDef> list2 = new List<ThingDef>();
			bool flag5 = false;
			foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef2.race != null && allDef2.race.leatherDef == this && !allDef2.IsCorpse)
				{
					if (!Find.HiddenItemsManager.Hidden(allDef2))
					{
						flag5 = true;
					}
					list2.Add(allDef2);
				}
			}
			yield return new StatDrawEntry(valueString: (!flag5) ? string.Format("({0})", "NotYetDiscovered".Translate()) : string.Join(", ", (from thingDef in list2
				where !Find.HiddenItemsManager.Hidden(thingDef)
				select thingDef.label).ToArray()).CapitalizeFirst(), category: StatCategoryDefOf.BasicsPawn, label: "Stat_SourceSpecies_Name".Translate(), reportText: "Stat_SourceSpecies_Desc".Translate(), displayPriorityWithinCategory: 1200, overrideReportTitle: null, hyperlinks: Dialog_InfoCard.DefsToHyperlinks(list2));
		}
		if (!equippedStatOffsets.NullOrEmpty())
		{
			for (int i = 0; i < equippedStatOffsets.Count; i++)
			{
				StatDef stat = equippedStatOffsets[i].stat;
				float num9 = equippedStatOffsets[i].value;
				StringBuilder stringBuilder7 = new StringBuilder(stat.description);
				if (req.HasThing && stat.Worker != null)
				{
					stringBuilder7.AppendLine();
					stringBuilder7.AppendLine();
					stringBuilder7.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(num9, ToStringNumberSense.Offset, stat.finalizeEquippedStatOffset));
					num9 = StatWorker.StatOffsetFromGear(req.Thing, stat);
					if (!stat.parts.NullOrEmpty())
					{
						stringBuilder7.AppendLine();
						for (int num10 = 0; num10 < stat.parts.Count; num10++)
						{
							string text = stat.parts[num10].ExplanationPart(req);
							if (!text.NullOrEmpty())
							{
								stringBuilder7.AppendLine(text);
							}
						}
					}
					stringBuilder7.AppendLine();
					stringBuilder7.AppendLine("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(num9, ToStringNumberSense.Offset, !stat.formatString.NullOrEmpty()));
				}
				yield return new StatDrawEntry(StatCategoryDefOf.EquippedStatOffsets, equippedStatOffsets[i].stat, num9, StatRequest.ForEmpty(), ToStringNumberSense.Offset, null, forceUnfinalizedMode: true).SetReportText(stringBuilder7.ToString());
			}
		}
		if (!IsDrug)
		{
			yield break;
		}
		foreach (StatDrawEntry item9 in DrugStatsUtility.SpecialDisplayStats(this))
		{
			yield return item9;
		}
	}
}
