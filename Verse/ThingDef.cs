using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class ThingDef : BuildableDef
	{
		public Type thingClass;

		public ThingCategory category;

		public TickerType tickerType;

		public int stackLimit = 1;

		public IntVec2 size = new IntVec2(1, 1);

		public bool destroyable = true;

		public bool rotatable = true;

		public bool smallVolume;

		public bool useHitPoints = true;

		public bool receivesSignals;

		public List<CompProperties> comps = new List<CompProperties>();

		public List<ThingDefCountClass> killedLeavings;

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

		public IntRange deepLumpSizeRange = IntRange.zero;

		public float generateCommonality = 1f;

		public float generateAllowChance = 1f;

		private bool canOverlapZones = true;

		public FloatRange startingHpRange = FloatRange.One;

		[NoTranslate]
		public List<string> thingSetMakerTags;

		public bool alwaysFlee;

		public List<RecipeDef> recipes;

		public bool messageOnDeteriorateInStorage = true;

		public bool canLoadIntoCaravan = true;

		public bool isMechClusterThreat;

		public FloatRange displayNumbersBetweenSameDefDistRange = FloatRange.Zero;

		public int minRewardCount = 1;

		public GraphicData graphicData;

		public DrawerType drawerType = DrawerType.RealtimeOnly;

		public bool drawOffscreen;

		public ColorGenerator colorGenerator;

		public float hideAtSnowDepth = 99999f;

		public bool drawDamagedOverlay = true;

		public bool castEdgeShadows;

		public float staticSunShadowHeight;

		public bool useSameGraphicForGhost;

		public bool selectable;

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

		public bool alwaysHaulable;

		public bool designateHaulable;

		public List<ThingCategoryDef> thingCategories;

		public bool mineable;

		public bool socialPropernessMatters;

		public bool stealable = true;

		public SoundDef soundDrop;

		public SoundDef soundPickup;

		public SoundDef soundInteract;

		public SoundDef soundImpactDefault;

		public SoundDef soundPlayInstrument;

		public bool saveCompressible;

		public bool isSaveable = true;

		public bool holdsRoof;

		public float fillPercent;

		public bool coversFloor;

		public bool neverOverlapFloors;

		public SurfaceType surfaceType;

		public bool blockPlants;

		public bool blockLight;

		public bool blockWind;

		public Tradeability tradeability = Tradeability.All;

		[NoTranslate]
		public List<string> tradeTags;

		public bool tradeNeverStack;

		public bool healthAffectsPrice = true;

		public ColorGenerator colorGeneratorInTraderStock;

		private List<VerbProperties> verbs;

		public List<Tool> tools;

		public float equippedAngleOffset;

		public EquipmentType equipmentType;

		public TechLevel techLevel;

		[NoTranslate]
		public List<string> weaponTags;

		[NoTranslate]
		public List<string> techHediffsTags;

		public bool destroyOnDrop;

		public List<StatModifier> equippedStatOffsets;

		public SoundDef meleeHitSound;

		public BuildableDef entityDefToBuild;

		public ThingDef projectileWhenLoaded;

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

		public bool canBeUsedUnderRoof = true;

		[Unsaved(false)]
		private string descriptionDetailedCached;

		[Unsaved(false)]
		public Graphic interactionCellGraphic;

		public const int SmallUnitPerVolume = 10;

		public const float SmallVolumePerUnit = 0.1f;

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

		public bool PlayerAcquirable => !destroyOnDrop;

		public bool EverTransmitsPower
		{
			get
			{
				for (int i = 0; i < comps.Count; i++)
				{
					CompProperties_Power compProperties_Power = comps[i] as CompProperties_Power;
					if (compProperties_Power != null && compProperties_Power.transmitsPower)
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
				if (passability == Traversability.Impassable && category != ThingCategory.Plant)
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
				if (IsBlueprint || IsFrame)
				{
					ThingDef thingDef = entityDefToBuild as ThingDef;
					if (thingDef != null)
					{
						return thingDef.CanOverlapZones;
					}
				}
				return true;
			}
		}

		public bool CountAsResource => resourceReadoutPriority != ResourceCountPriority.Uncounted;

		[Obsolete("Will be removed in a future version.")]
		public bool BlockPlanting => BlocksPlanting();

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
				switch (category)
				{
				case ThingCategory.Pawn:
					return true;
				case ThingCategory.Building:
					return true;
				default:
					return false;
				}
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
					return this == ThingDefOf.BurnedTree;
				}
				return true;
			}
		}

		public bool CanInteractThroughCorners
		{
			get
			{
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
				if (passability != Traversability.Impassable)
				{
					return IsDoor;
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

		public bool IsApparel => apparel != null;

		public bool IsBed => typeof(Building_Bed).IsAssignableFrom(thingClass);

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

		public bool IsWeapon
		{
			get
			{
				if (category == ThingCategory.Item)
				{
					if (verbs.NullOrEmpty())
					{
						return !tools.NullOrEmpty();
					}
					return true;
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

		public bool IsSmoothable
		{
			get
			{
				if (building != null)
				{
					return building.smoothedThing != null;
				}
				return false;
			}
		}

		public bool IsSmoothed
		{
			get
			{
				if (building != null)
				{
					return building.unsmoothedThing != null;
				}
				return false;
			}
		}

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
				if (category == ThingCategory.Building && building.isNaturalRock && !building.isResourceRock)
				{
					return !IsSmoothed;
				}
				return false;
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
					concreteExamplesInt[stuff] = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pkd) => pkd.race == this).FirstOrDefault());
				}
			}
			return concreteExamplesInt[stuff];
		}

		public CompProperties CompDefFor<T>() where T : ThingComp
		{
			return comps.FirstOrDefault((CompProperties c) => c.compClass == typeof(T));
		}

		public CompProperties CompDefForAssignableFrom<T>() where T : ThingComp
		{
			return comps.FirstOrDefault((CompProperties c) => typeof(T).IsAssignableFrom(c.compClass));
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
				T val = comps[i] as T;
				if (val != null)
				{
					return val;
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
					if (graphicData.shaderType == null)
					{
						graphicData.shaderType = ShaderTypeDefOf.Cutout;
					}
					graphic = graphicData.Graphic;
				});
			}
			if (tools != null)
			{
				for (int i = 0; i < tools.Count; i++)
				{
					tools[i].id = i.ToString();
				}
			}
			if (verbs != null && verbs.Count == 1)
			{
				verbs[0].label = label;
			}
			base.PostLoad();
			if (category == ThingCategory.Building && building == null)
			{
				building = new BuildingProperties();
			}
			if (building != null)
			{
				building.PostLoadSpecial(this);
			}
			if (plant != null)
			{
				plant.PostLoadSpecial(this);
			}
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
				if (!race.Humanlike)
				{
					PawnKindDef anyPawnKind = race.AnyPawnKind;
					if (anyPawnKind != null)
					{
						Material material = anyPawnKind.lifeStages.Last().bodyGraphicData.Graphic.MatAt(Rot4.East);
						uiIcon = (Texture2D)material.mainTexture;
						uiIconColor = material.color;
					}
				}
				return;
			}
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
			if (building != null)
			{
				building.ResolveReferencesSpecial();
			}
			if (graphicData != null)
			{
				graphicData.ResolveReferencesSpecial();
			}
			if (race != null)
			{
				race.ResolveReferencesSpecial();
			}
			if (stuffProps != null)
			{
				stuffProps.ResolveReferencesSpecial();
			}
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
						Log.Error(string.Concat("Could not instantiate inspector tab of type ", inspectorTabs[i], ": ", ex));
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
			if (label.NullOrEmpty())
			{
				yield return "no label";
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
					if (statBases.Where((StatModifier st) => st.stat == statBase.stat).Count() > 1)
					{
						yield return string.Concat("defines the stat base ", statBase.stat, " more than once.");
					}
				}
			}
			if (!BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.Beauty))
			{
				yield return string.Concat("Beauty stat base is defined, but Things of category ", category, " cannot have beauty.");
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
						yield return string.Concat("cost in ", cost.thingDef, " is zero.");
					}
				}
			}
			if (thingCategories != null)
			{
				ThingCategoryDef thingCategoryDef = thingCategories.FirstOrDefault((ThingCategoryDef cat) => thingCategories.Count((ThingCategoryDef c) => c == cat) > 1);
				if (thingCategoryDef != null)
				{
					yield return string.Concat("has duplicate thingCategory ", thingCategoryDef, ".");
				}
			}
			if (Fillage == FillCategory.Full && category != ThingCategory.Building)
			{
				yield return "gives full cover but is not a building.";
			}
			if (comps.Any((CompProperties c) => c.compClass == typeof(CompPowerTrader)) && drawerType == DrawerType.MapMeshOnly)
			{
				yield return "has PowerTrader comp but does not draw real time. It won't draw a needs-power overlay.";
			}
			if (equipmentType != 0)
			{
				if (techLevel == TechLevel.Undefined && !destroyOnDrop)
				{
					yield return "is equipment but has no tech level.";
				}
				if (!comps.Any((CompProperties c) => c.compClass == typeof(CompEquippable)))
				{
					yield return "is equipment but has no CompEquippable";
				}
			}
			if (thingClass == typeof(Bullet) && projectile.damageDef == null)
			{
				yield return " is a bullet but has no damageDef.";
			}
			if (destroyOnDrop)
			{
				if (!menuHidden)
				{
					yield return "destroyOnDrop but not menuHidden.";
				}
				if (tradeability != 0)
				{
					yield return "destroyOnDrop but tradeability is " + tradeability;
				}
			}
			if (stackLimit > 1 && !drawGUIOverlay)
			{
				yield return "has stackLimit > 1 but also has drawGUIOverlay = false.";
			}
			if (damageMultipliers != null)
			{
				foreach (DamageMultiplier mult in damageMultipliers)
				{
					if (damageMultipliers.Where((DamageMultiplier m) => m.damageDef == mult.damageDef).Count() > 1)
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
			if (this.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 1E-05f && !CanEverDeteriorate && !destroyOnDrop)
			{
				yield return "has >0 DeteriorationRate but can't deteriorate.";
			}
			if (drawerType == DrawerType.MapMeshOnly && comps.Any((CompProperties c) => c.compClass == typeof(CompForbiddable)))
			{
				yield return "drawerType=MapMeshOnly but has a CompForbiddable, which must draw in real time.";
			}
			if (smeltProducts != null && smeltable)
			{
				yield return "has smeltProducts but has smeltable=false";
			}
			if (equipmentType != 0 && verbs.NullOrEmpty() && tools.NullOrEmpty())
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
			if (!destroyOnDrop && this != ThingDefOf.MinifiedThing && (EverHaulable || Minifiable) && (statBases.NullOrEmpty() || !statBases.Any((StatModifier s) => s.stat == StatDefOf.Mass)))
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
			if (graphicData != null && graphicData.shadowData != null && staticSunShadowHeight > 0f)
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
				for (int k = 0; k < verbs.Count; k++)
				{
					foreach (string item4 in verbs[k].ConfigErrors(this))
					{
						yield return $"verb {k}: {item4}";
					}
				}
			}
			if (building != null)
			{
				foreach (string item5 in building.ConfigErrors(this))
				{
					yield return item5;
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
				for (int k = 0; k < comps.Count; k++)
				{
					foreach (string item7 in comps[k].ConfigErrors(this))
					{
						yield return item7;
					}
				}
			}
			if (race != null)
			{
				foreach (string item8 in race.ConfigErrors())
				{
					yield return item8;
				}
			}
			if (race != null && tools != null)
			{
				ThingDef thingDef = this;
				int i;
				for (i = 0; i < tools.Count; i++)
				{
					if (tools[i].linkedBodyPartsGroup != null && !race.body.AllParts.Any((BodyPartRecord part) => part.groups.Contains(thingDef.tools[i].linkedBodyPartsGroup)))
					{
						yield return string.Concat("has tool with linkedBodyPartsGroup ", tools[i].linkedBodyPartsGroup, " but body ", race.body, " has no parts with that group.");
					}
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
			if (tools == null)
			{
				yield break;
			}
			Tool tool = tools.SelectMany((Tool lhs) => tools.Where((Tool rhs) => lhs != rhs && lhs.id == rhs.id)).FirstOrDefault();
			if (tool != null)
			{
				yield return $"duplicate thingdef tool id {tool.id}";
			}
			foreach (Tool tool2 in tools)
			{
				foreach (string item11 in tool2.ConfigErrors())
				{
					yield return item11;
				}
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
			string[] array = (from u in DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.recipeUsers != null && r.products.Count == 1 && r.products.Any((ThingDefCountClass p) => p.thingDef == this) && !r.IsSurgery).SelectMany((RecipeDef r) => r.recipeUsers)
				select u.label).ToArray();
			if (array.Any())
			{
				string valueString = array.ToCommaList().CapitalizeFirst();
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CreatedAt".Translate(), valueString, "Stat_Thing_CreatedAt_Desc".Translate(), 1103);
			}
			if (thingClass != null && typeof(Building_Bed).IsAssignableFrom(thingClass) && !statBases.StatListContains(StatDefOf.BedRestEffectiveness))
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Building, StatDefOf.BedRestEffectiveness, StatDefOf.BedRestEffectiveness.valueIfMissing, StatRequest.ForEmpty());
			}
			if (!verbs.NullOrEmpty())
			{
				VerbProperties verb = verbs.First((VerbProperties x) => x.isPrimary);
				StatCategoryDef statCategoryDef;
				if (category != ThingCategory.Pawn)
				{
					StatCategoryDef weapon = StatCategoryDefOf.Weapon;
					statCategoryDef = weapon;
				}
				else
				{
					StatCategoryDef weapon = StatCategoryDefOf.PawnCombat;
					statCategoryDef = weapon;
				}
				StatCategoryDef verbStatCategory = statCategoryDef;
				float warmupTime = verb.warmupTime;
				if (warmupTime > 0f)
				{
					TaggedString taggedString = (category == ThingCategory.Pawn) ? "MeleeWarmupTime".Translate() : "WarmupTime".Translate();
					yield return new StatDrawEntry(verbStatCategory, taggedString, warmupTime.ToString("0.##") + " " + "LetterSecond".Translate(), "Stat_Thing_Weapon_MeleeWarmupTime_Desc".Translate(), 3555);
				}
				if (verb.defaultProjectile != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Stat_Thing_Damage_Desc".Translate());
					stringBuilder.AppendLine();
					yield return new StatDrawEntry(valueString: ((float)verb.defaultProjectile.projectile.GetDamageAmount(req.Thing, stringBuilder)).ToString(), category: verbStatCategory, label: "Damage".Translate(), reportText: stringBuilder.ToString(), displayPriorityWithinCategory: 5500);
					if (verb.defaultProjectile.projectile.damageDef.armorCategory != null)
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						float armorPenetration = verb.defaultProjectile.projectile.GetArmorPenetration(req.Thing, stringBuilder2);
						TaggedString taggedString2 = "ArmorPenetrationExplanation".Translate();
						if (stringBuilder2.Length != 0)
						{
							taggedString2 += "\n\n" + stringBuilder2;
						}
						yield return new StatDrawEntry(verbStatCategory, "ArmorPenetration".Translate(), armorPenetration.ToStringPercent(), taggedString2, 5400);
					}
				}
				if (verb.LaunchesProjectile)
				{
					int burstShotCount = verb.burstShotCount;
					float burstShotFireRate = 60f / verb.ticksBetweenBurstShots.TicksToSeconds();
					float range = verb.range;
					if (burstShotCount > 1)
					{
						yield return new StatDrawEntry(verbStatCategory, "BurstShotCount".Translate(), burstShotCount.ToString(), "Stat_Thing_Weapon_BurstShotCount_Desc".Translate(), 5391);
						yield return new StatDrawEntry(verbStatCategory, "BurstShotFireRate".Translate(), burstShotFireRate.ToString("0.##") + " rpm", "Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate(), 5392);
					}
					yield return new StatDrawEntry(verbStatCategory, "Range".Translate(), range.ToString("F0"), "Stat_Thing_Weapon_Range_Desc".Translate(), 5390);
					if (verb.defaultProjectile != null && verb.defaultProjectile.projectile != null && verb.defaultProjectile.projectile.stoppingPower != 0f)
					{
						yield return new StatDrawEntry(verbStatCategory, "StoppingPower".Translate(), verb.defaultProjectile.projectile.stoppingPower.ToString("F1"), "StoppingPowerExplanation".Translate(), 5402);
					}
				}
				if (verb.forcedMissRadius > 0f)
				{
					yield return new StatDrawEntry(verbStatCategory, "MissRadius".Translate(), verb.forcedMissRadius.ToString("0.#"), "Stat_Thing_Weapon_MissRadius_Desc".Translate(), 3557);
					yield return new StatDrawEntry(verbStatCategory, "DirectHitChance".Translate(), (1f / (float)GenRadial.NumCellsInRadius(verb.forcedMissRadius)).ToStringPercent(), "Stat_Thing_Weapon_DirectHitChance_Desc".Translate(), 3560);
				}
			}
			if (plant != null)
			{
				foreach (StatDrawEntry item2 in plant.SpecialDisplayStats())
				{
					yield return item2;
				}
			}
			if (ingestible != null)
			{
				foreach (StatDrawEntry item3 in ingestible.SpecialDisplayStats())
				{
					yield return item3;
				}
			}
			if (race != null)
			{
				foreach (StatDrawEntry item4 in race.SpecialDisplayStats(this, req))
				{
					yield return item4;
				}
			}
			if (building != null)
			{
				foreach (StatDrawEntry item5 in building.SpecialDisplayStats(this, req))
				{
					yield return item5;
				}
			}
			if (isTechHediff)
			{
				IEnumerable<RecipeDef> enumerable = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.addsHediff != null && x.IsIngredient(this));
				bool multiple = enumerable.Count() >= 2;
				foreach (RecipeDef item6 in enumerable)
				{
					string extraLabelPart = multiple ? (" (" + item6.addsHediff.label + ")") : "";
					HediffDef diff = item6.addsHediff;
					if (diff.addedPartProps != null)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BodyPartEfficiency".Translate() + extraLabelPart, diff.addedPartProps.partEfficiency.ToStringByStyle(ToStringStyle.PercentZero), "Stat_Thing_BodyPartEfficiency_Desc".Translate(), 4000);
					}
					foreach (StatDrawEntry item7 in diff.SpecialDisplayStats(StatRequest.ForEmpty()))
					{
						item7.category = StatCategoryDefOf.Implant;
						yield return item7;
					}
					HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = diff.CompProps<HediffCompProperties_VerbGiver>();
					if (hediffCompProperties_VerbGiver != null)
					{
						if (!hediffCompProperties_VerbGiver.verbs.NullOrEmpty())
						{
							VerbProperties verb = hediffCompProperties_VerbGiver.verbs[0];
							if (!verb.IsMeleeAttack)
							{
								if (verb.defaultProjectile != null)
								{
									StringBuilder stringBuilder3 = new StringBuilder();
									stringBuilder3.AppendLine("Stat_Thing_Damage_Desc".Translate());
									stringBuilder3.AppendLine();
									yield return new StatDrawEntry(valueString: verb.defaultProjectile.projectile.GetDamageAmount(null, stringBuilder3).ToString(), category: StatCategoryDefOf.Basics, label: "Damage".Translate() + extraLabelPart, reportText: stringBuilder3.ToString(), displayPriorityWithinCategory: 5500);
									if (verb.defaultProjectile.projectile.damageDef.armorCategory != null)
									{
										float armorPenetration2 = verb.defaultProjectile.projectile.GetArmorPenetration(null);
										yield return new StatDrawEntry(StatCategoryDefOf.Basics, "ArmorPenetration".Translate() + extraLabelPart, armorPenetration2.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
									}
								}
							}
							else
							{
								int meleeDamageBaseAmount = verb.meleeDamageBaseAmount;
								if (verb.meleeDamageDef.armorCategory != null)
								{
									float num = verb.meleeArmorPenetrationBase;
									if (num < 0f)
									{
										num = (float)meleeDamageBaseAmount * 0.015f;
									}
									yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "ArmorPenetration".Translate() + extraLabelPart, num.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
								}
							}
						}
						else if (!hediffCompProperties_VerbGiver.tools.NullOrEmpty())
						{
							Tool tool = hediffCompProperties_VerbGiver.tools[0];
							if (ThingUtility.PrimaryMeleeWeaponDamageType(hediffCompProperties_VerbGiver.tools).armorCategory != null)
							{
								float num2 = tool.armorPenetration;
								if (num2 < 0f)
								{
									num2 = tool.power * 0.015f;
								}
								yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "ArmorPenetration".Translate() + extraLabelPart, num2.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 5400);
							}
						}
					}
					ThoughtDef thoughtDef = DefDatabase<ThoughtDef>.AllDefs.FirstOrDefault((ThoughtDef x) => x.hediff == diff);
					if (thoughtDef != null && thoughtDef.stages != null && thoughtDef.stages.Any())
					{
						yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MoodChange".Translate() + extraLabelPart, thoughtDef.stages.First().baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset), "Stat_Thing_MoodChange_Desc".Translate(), 3500);
					}
				}
			}
			for (int k = 0; k < comps.Count; k++)
			{
				foreach (StatDrawEntry item8 in comps[k].SpecialDisplayStats(req))
				{
					yield return item8;
				}
			}
			if (building != null)
			{
				if (building.mineableThing != null)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_MineableThing_Name".Translate(), building.mineableThing.LabelCap, "Stat_MineableThing_Desc".Translate(), 2200, null, new Dialog_InfoCard.Hyperlink[1]
					{
						new Dialog_InfoCard.Hyperlink(building.mineableThing)
					});
				}
				if (building.IsTurret)
				{
					ThingDef turret = building.turretGunDef;
					yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Weapon_Name".Translate(), turret.LabelCap, "Stat_Weapon_Desc".Translate(), 5389, null, new Dialog_InfoCard.Hyperlink[1]
					{
						new Dialog_InfoCard.Hyperlink(turret)
					});
					StatRequest request = StatRequest.For(turret, null);
					foreach (StatDrawEntry item9 in turret.SpecialDisplayStats(request))
					{
						if (item9.category == StatCategoryDefOf.Weapon)
						{
							yield return item9;
						}
					}
					for (int k = 0; k < turret.statBases.Count; k++)
					{
						StatModifier statModifier = turret.statBases[k];
						if (statModifier.stat.category == StatCategoryDefOf.Weapon)
						{
							yield return new StatDrawEntry(StatCategoryDefOf.Weapon, statModifier.stat, statModifier.value, request);
						}
					}
				}
			}
			if (IsMeat)
			{
				List<ThingDef> list = new List<ThingDef>();
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (allDef.race != null && allDef.race.meatDef == this)
					{
						list.Add(allDef);
					}
				}
				string valueString4 = string.Join(", ", list.Select((ThingDef p) => p.label).ToArray()).CapitalizeFirst();
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_SourceSpecies_Name".Translate(), valueString4, "Stat_SourceSpecies_Desc".Translate(), 1200, null, Dialog_InfoCard.DefsToHyperlinks(list));
			}
			if (IsLeather)
			{
				List<ThingDef> list2 = new List<ThingDef>();
				foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
				{
					if (allDef2.race != null && allDef2.race.leatherDef == this)
					{
						list2.Add(allDef2);
					}
				}
				string valueString5 = string.Join(", ", list2.Select((ThingDef p) => p.label).ToArray()).CapitalizeFirst();
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_SourceSpecies_Name".Translate(), valueString5, "Stat_SourceSpecies_Desc".Translate(), 1200, null, Dialog_InfoCard.DefsToHyperlinks(list2));
			}
			if (!equippedStatOffsets.NullOrEmpty())
			{
				for (int k = 0; k < equippedStatOffsets.Count; k++)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.EquippedStatOffsets, equippedStatOffsets[k].stat, equippedStatOffsets[k].value, StatRequest.ForEmpty(), ToStringNumberSense.Offset, null, forceUnfinalizedMode: true);
				}
			}
			if (!IsDrug)
			{
				yield break;
			}
			foreach (StatDrawEntry item10 in DrugStatsUtility.SpecialDisplayStats(this))
			{
				yield return item10;
			}
		}
	}
}
