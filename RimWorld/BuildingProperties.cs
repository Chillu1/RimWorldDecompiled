using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class BuildingProperties
{
	public bool isEdifice = true;

	[NoTranslate]
	public List<string> buildingTags = new List<string>();

	public bool isInert;

	public bool isTargetable = true;

	private bool deconstructible = true;

	public bool alwaysDeconstructible;

	public bool alwaysUninstallable;

	public List<ThingDef> leavingsBlacklist;

	public List<ThingDef> forcedCostLeavings;

	public bool claimable = true;

	public bool isSittable;

	public bool multiSittable;

	public bool sitIgnoreOrientation;

	public ConceptDef spawnedConceptLearnOpportunity;

	public ConceptDef boughtConceptLearnOpportunity;

	public bool expandHomeArea = true;

	public Type blueprintClass = typeof(Blueprint_Build);

	public GraphicData blueprintGraphicData;

	public float uninstallWork = 200f;

	public bool forceShowRoomStats;

	public bool neverBuildable;

	public bool isWall;

	public bool alwaysExchangeVacuum;

	public bool canExchangeVacuum;

	public bool draftAttackNonDeconstructable = true;

	public bool wantsHopperAdjacent;

	public bool allowWireConnection = true;

	public bool shipPart;

	public bool canPlaceOverImpassablePlant = true;

	public float heatPerTickWhileWorking;

	public bool canBuildNonEdificesUnder = true;

	public bool canPlaceOverWall;

	public bool isPlaceOverableWall;

	public bool allowAutoroof = true;

	public bool preventDeteriorationOnTop;

	public bool preventDeteriorationInside;

	public bool isMealSource;

	public bool isNaturalRock;

	public bool isResourceRock;

	public bool repairable = true;

	public float roofCollapseDamageMultiplier = 1f;

	public bool hasFuelingPort;

	public ThingDef smoothedThing;

	[Unsaved(false)]
	public ThingDef unsmoothedThing;

	public TerrainDef naturalTerrain;

	public TerrainDef leaveTerrain;

	public float combatPower;

	public int minMechClusterPoints;

	public float destroyShakeAmount = -1f;

	public SoundDef destroySound;

	public SoundDef soundMeleeHitOverride;

	public EffecterDef destroyEffecter;

	public BuildingSizeCategory buildingSizeCategory;

	public bool isFence;

	public List<ThingDef> relatedBuildCommands;

	public List<TerrainDef> relatedTerrain;

	public bool alwaysShowRelatedBuildCommands;

	public bool useIdeoColor;

	public bool wakeDormantPawnsOnConstruction = true;

	public int maxItemsInCell = 1;

	public bool paintable;

	public bool canBeDamagedByAttacks = true;

	public bool isHopper;

	public bool quickTargetable;

	public bool displayAttackToDestroyOnInspectPane;

	public bool isEscapableContainer;

	public bool isPowerConduit;

	public bool biomeSpecific;

	public bool crater;

	public bool canLandGravshipOn;

	public bool isAirtight;

	public bool isStuffableAirtight;

	public bool canConstructInCorner;

	public bool isAttachment;

	public bool supportsWallAttachments;

	public bool isPlayerEjectable;

	public SoundDef openingStartedSound;

	public GraphicData fullGraveGraphicData;

	public float bed_healPerDay;

	public bool bed_defaultMedical;

	public bool bed_showSleeperBody;

	public bool bed_humanlike = true;

	public float bed_maxBodySize = 9999f;

	public bool bed_caravansCanUse;

	public bool bed_slabBed;

	public bool bed_crib;

	public float bed_pawnDrawOffset;

	public bool bed_canBeMedical = true;

	public bool bed_DisplayOwnerType = true;

	public bool bed_DisplayOwnersInInspectString = true;

	public bool bed_countsForBedroomOrBarracks = true;

	public bool bed_emptyCountsForBarracks = true;

	public bool bed_UseSheetColor = true;

	public float nutritionCostPerDispense;

	public SoundDef soundDispense;

	public ThingDef turretGunDef;

	public FloatRange turretBurstWarmupTime = new FloatRange(0.25f, 0.75f);

	public float turretBurstCooldownTime = -1f;

	public float turretInitialCooldownTime;

	[Unsaved(false)]
	public Material turretTopMat;

	[Unsaved(false)]
	public Material turretTopLoadedMat;

	public float turretTopDrawSize = 2f;

	public Vector2 turretTopOffset;

	public bool playTargetAcquiredSound = true;

	public GraphicData turretTopLoadedGraphic;

	public List<MechWeightClassDef> requiredMechWeightClasses;

	public bool ai_combatDangerous;

	public bool ai_chillDestination = true;

	public bool ai_neverTrashThis;

	public bool preferConnectingToFences;

	public bool roamerCanOpen;

	public SoundDef soundDoorOpenPowered;

	public SoundDef soundDoorClosePowered;

	public SoundDef soundDoorOpenManual;

	public SoundDef soundDoorCloseManual;

	public SoundDef soundDoorCloseEnd;

	public float poweredDoorOpenSpeedFactor = 1f;

	public float poweredDoorCloseSpeedFactor = 1f;

	public float unpoweredDoorOpenSpeedFactor = 1f;

	public float unpoweredDoorCloseSpeedFactor = 1f;

	public int doorTempEqualizeIntervalClosed = 375;

	public int doorTempEqualizeIntervalOpen = 34;

	public float doorTempEqualizeRate = 1f;

	[NoTranslate]
	public string sowTag;

	public ThingDef defaultPlantToGrow;

	public ThingDef mineableThing;

	public int mineableYield = 1;

	public bool veinMineable;

	public float mineableNonMinedEfficiency = 0.7f;

	public float mineableDropChance = 1f;

	public bool mineableYieldWasteable = true;

	public float mineableScatterCommonality;

	public IntRange mineableScatterLumpSizeRange = new IntRange(20, 40);

	public bool mineablePreventMeteorite;

	public bool mineablePreventNaturalRockOnSurface;

	public StorageSettings fixedStorageSettings;

	public StorageSettings defaultStorageSettings;

	public bool ignoreStoredThingsBeauty;

	public string storageGroupTag;

	public string groupingLabel;

	public int groupingOrder;

	public bool isTrap;

	public bool trapDestroyOnSpring;

	public float trapPeacefulWildAnimalsSpringChanceFactor = 1f;

	public DamageArmorCategoryDef trapDamageCategory;

	public GraphicData trapUnarmedGraphicData;

	[Unsaved(false)]
	public Graphic trapUnarmedGraphic;

	public RoomRoleDef workTableRoomRole;

	public float workTableNotInRoomRoleFactor = 1f;

	public float unpoweredWorkTableWorkSpeedFactor;

	public SoundDef workTableCompleteSoundDef;

	public IntRange watchBuildingStandDistanceRange = IntRange.One;

	public int watchBuildingStandRectWidth = 3;

	public bool watchBuildingInSameRoom;

	public JoyKindDef joyKind;

	public int haulToContainerDuration;

	public float instrumentRange;

	public int minDistanceToSameTypeOfBuilding;

	public bool artificialForMeditationPurposes = true;

	public EffecterDef effectWatching;

	public GraphicData gibbetCageTopGraphicData;

	public Vector3 gibbetCorposeDrawOffset;

	public EffecterDef gibbetCagePlaceCorpseEffecter;

	public EffecterDef openingEffect;

	public FillableBarRequestWithRotation barDrawData;

	public MoteForRotationData gestatorFormingMote;

	public MoteForRotationData gestatorCycleCompleteMote;

	public MoteForRotationData gestatorFormedMote;

	public GraphicData mechGestatorCylinderGraphic;

	public GraphicData mechGestatorTopGraphic;

	public GraphicData formingGraphicData;

	public float formingMechBobSpeed = 0.001f;

	public float formingMechYBobDistance = 0.1f;

	public List<Vector3> formingMechPerRotationOffset = new List<Vector3>();

	public Vector2 maxFormedMechDrawSize = new Vector2(1.5f, 1.5f);

	public List<IngredientCount> subcoreScannerFixedIngredients;

	public int subcoreScannerTicks = 7500;

	public HediffDef subcoreScannerHediff;

	public bool destroyBrain;

	public ThingDef subcoreScannerOutputDef;

	public EffecterDef subcoreScannerStartEffect;

	public SoundDef subcoreScannerWorking;

	public SoundDef subcoreScannerComplete;

	public bool isInsectCocoon;

	public GraphicData wastepackAtomizerBottomGraphic;

	public GraphicData wastepackAtomizerWindowGraphic;

	public EffecterDef wastepackAtomizerOperationEffecter;

	public GraphicData bookendGraphicEast;

	public GraphicData bookendGraphicNorth;

	public bool isSupportDoor;

	public GraphicData doorTopGraphic;

	public GraphicData doorSupportGraphic;

	public GraphicData upperMoverGraphic;

	public Vector3 doorTopGraphicOffset;

	public Vector3 doorSupportGraphicOffset;

	public Vector3 doorTopHorizontalOffset = new Vector3(0f, 0f, 0.1f);

	public Vector3 doorTopVerticalOffset = new Vector3(0f, 0f, 0f);

	public ThingDef groundSpawnerThingToSpawn;

	public IntRange groundSpawnerSpawnDelay;

	public bool groundSpawnerDestroyAdjacent;

	public SoundDef groundSpawnerSustainerSound;

	public EffecterDef groundSpawnerSustainedEffecter;

	public EffecterDef groundSpawnerCompleteEffecter;

	public string groundSpawnerLetterLabel;

	public string groundSpawnerLetterText;

	private static List<string> tmpFenceBlockedAnimals = new List<string>();

	public bool SupportsPlants => sowTag != null;

	public int EffectiveMineableYield => Mathf.RoundToInt((float)mineableYield * Find.Storyteller.difficulty.mineYieldFactor);

	public bool IsTurret => turretGunDef != null;

	public bool IsDeconstructible
	{
		get
		{
			if (!alwaysDeconstructible)
			{
				if (!isNaturalRock)
				{
					return deconstructible;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsMortar
	{
		get
		{
			if (!IsTurret)
			{
				return false;
			}
			List<VerbProperties> verbs = turretGunDef.Verbs;
			for (int i = 0; i < verbs.Count; i++)
			{
				if (verbs[i].isPrimary && verbs[i].defaultProjectile != null && verbs[i].defaultProjectile.projectile.flyOverhead)
				{
					return true;
				}
			}
			if (turretGunDef.HasComp(typeof(CompChangeableProjectile)))
			{
				if (turretGunDef.building.fixedStorageSettings.filter.Allows(ThingDefOf.Shell_HighExplosive))
				{
					return true;
				}
				foreach (ThingDef allowedThingDef in turretGunDef.building.fixedStorageSettings.filter.AllowedThingDefs)
				{
					if (allowedThingDef.projectileWhenLoaded != null && allowedThingDef.projectileWhenLoaded.projectile.flyOverhead)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public IEnumerable<string> ConfigErrors(ThingDef parent)
	{
		if (isTrap && !isEdifice)
		{
			yield return "isTrap but is not edifice. Code will break.";
		}
		if (alwaysDeconstructible && !deconstructible)
		{
			yield return "alwaysDeconstructible=true but deconstructible=false";
		}
		if (parent.holdsRoof && !isEdifice)
		{
			yield return "holds roof but is not an edifice.";
		}
		if (buildingTags.Contains("MechClusterCombatThreat") && combatPower <= 0f)
		{
			yield return "has MechClusterCombatThreat tag but 0 combatPower and thus no points cost; this will make an infinite loop during mech cluster building selection";
		}
		if (parent.IsGibbetCage && gibbetCageTopGraphicData == null)
		{
			yield return "Gibbet cage has no graphic data for gibbet cage top set.";
		}
		if (parent.IsMechGestator)
		{
			if (mechGestatorCylinderGraphic == null)
			{
				yield return "Mech gestator has no cylinder graphic set.";
			}
			if (mechGestatorTopGraphic == null)
			{
				yield return "Mech gestator has no top graphic set.";
			}
		}
		if (isInsectCocoon && combatPower <= 0f)
		{
			yield return "Insect cocoon requires combat power value > 0.";
		}
		if (typeof(BuildingGroundSpawner).IsAssignableFrom(parent.thingClass) && groundSpawnerThingToSpawn == null)
		{
			yield return "Building ground spawner must have thing to spawn.";
		}
	}

	public void PostLoadSpecial(ThingDef parent)
	{
	}

	public void ResolveReferencesSpecial()
	{
		if (soundDoorOpenPowered == null)
		{
			soundDoorOpenPowered = SoundDefOf.Door_OpenPowered;
		}
		if (soundDoorClosePowered == null)
		{
			soundDoorClosePowered = SoundDefOf.Door_ClosePowered;
		}
		if (soundDoorOpenManual == null)
		{
			soundDoorOpenManual = SoundDefOf.Door_OpenManual;
		}
		if (soundDoorCloseManual == null)
		{
			soundDoorCloseManual = SoundDefOf.Door_CloseManual;
		}
		if (turretGunDef != null)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				turretTopMat = MaterialPool.MatFrom(turretGunDef.graphicData.texPath);
			});
		}
		if (turretTopLoadedGraphic != null)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				turretTopLoadedMat = MaterialPool.MatFrom(turretTopLoadedGraphic.texPath);
			});
		}
		if (fixedStorageSettings != null)
		{
			fixedStorageSettings.filter.ResolveReferences();
		}
		if (defaultStorageSettings == null && fixedStorageSettings != null)
		{
			defaultStorageSettings = new StorageSettings();
			defaultStorageSettings.CopyFrom(fixedStorageSettings);
		}
		if (defaultStorageSettings != null)
		{
			defaultStorageSettings.filter.ResolveReferences();
		}
	}

	public static void FinalizeInit()
	{
		List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			ThingDef thingDef = allDefsListForReading[i];
			if (thingDef.building?.smoothedThing != null)
			{
				ThingDef thingDef2 = thingDef.building.smoothedThing;
				if (thingDef2.building == null)
				{
					Log.Error($"{thingDef} is smoothable to non-building {thingDef2}");
				}
				else if (thingDef2.building.unsmoothedThing == null || thingDef2.building.unsmoothedThing == thingDef)
				{
					thingDef2.building.unsmoothedThing = thingDef;
				}
				else
				{
					Log.Error($"{thingDef} and {thingDef2.building.unsmoothedThing} both smooth to {thingDef2}");
				}
			}
		}
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef, StatRequest req)
	{
		if (joyKind != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Stat_RecreationType_Desc".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Stat_JoyKind_AllTypes".Translate() + ":");
			foreach (JoyKindDef allDef in DefDatabase<JoyKindDef>.AllDefs)
			{
				stringBuilder.AppendLine("  - " + allDef.LabelCap);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_JoyKind".Translate(), joyKind.LabelCap, stringBuilder.ToString(), 4750, joyKind.LabelCap);
		}
		if (parentDef.Minifiable)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_WorkToUninstall".Translate(), uninstallWork.ToStringWorkAmount(), "Stat_Thing_WorkToUninstall_Desc".Translate(), 1102);
		}
		if (typeof(Building_TrapDamager).IsAssignableFrom(parentDef.thingClass))
		{
			float f = StatDefOf.TrapMeleeDamage.Worker.GetValue(req) * 0.015f;
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "TrapArmorPenetration".Translate(), f.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 3000);
		}
		if (isFence)
		{
			TaggedString taggedString = "Stat_Thing_Fence_Desc".Translate();
			tmpFenceBlockedAnimals.Clear();
			tmpFenceBlockedAnimals.AddRange(from g in DefDatabase<PawnKindDef>.AllDefs
				where g.RaceProps.Animal && g.RaceProps.FenceBlocked
				select g.LabelCap.Resolve() into s
				orderby s
				select s);
			taggedString += ":\n\n";
			taggedString += tmpFenceBlockedAnimals.ToLineList("- ");
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_Fence".Translate(), "Yes".Translate(), taggedString, 4800);
			tmpFenceBlockedAnimals.Clear();
		}
		if (workTableRoomRole != null && !Mathf.Approximately(workTableNotInRoomRoleFactor, 1f))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_OptimalRoom".Translate(), workTableRoomRole.LabelCap, "Stat_Building_OptimalRoom_Desc".Translate(), 4850);
		}
		if (ModsConfig.IdeologyActive)
		{
			foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
			{
				for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
				{
					if (!(ideo.PreceptsListForReading[i] is Precept_Building precept_Building) || precept_Building.ThingDef != parentDef || precept_Building.presenceDemand == null)
					{
						continue;
					}
					IdeoBuildingPresenceDemand presenceDemand = precept_Building.presenceDemand;
					if (!presenceDemand.roomRequirements.NullOrEmpty())
					{
						string valueString = presenceDemand.roomRequirements.Select((RoomRequirement r) => r.Label()).ToCommaList().CapitalizeFirst();
						string reportText = presenceDemand.roomRequirements.Select((RoomRequirement r) => r.LabelCap()).ToLineList("  - ");
						yield return new StatDrawEntry(StatCategoryDefOf.Building, "RoomRequirements".Translate(), valueString, reportText, 2101);
					}
				}
			}
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Building, "Stat_Building_Paintable".Translate(), paintable.ToStringYesNo(), "Stat_Building_PaintableDesc".Translate(), 6000);
	}

	public GenDraw.FillableBarRequest BarDrawDataFor(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => barDrawData.north, 
			1 => barDrawData.east, 
			2 => barDrawData.south, 
			3 => barDrawData.west, 
			_ => barDrawData.north, 
		};
	}
}
