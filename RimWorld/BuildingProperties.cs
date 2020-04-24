using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class BuildingProperties
	{
		public bool isEdifice = true;

		[NoTranslate]
		public List<string> buildingTags = new List<string>();

		public bool isInert;

		private bool deconstructible = true;

		public bool alwaysDeconstructible;

		public bool claimable = true;

		public bool isSittable;

		public SoundDef soundAmbient;

		public ConceptDef spawnedConceptLearnOpportunity;

		public ConceptDef boughtConceptLearnOpportunity;

		public bool expandHomeArea = true;

		public Type blueprintClass = typeof(Blueprint_Build);

		public GraphicData blueprintGraphicData;

		public float uninstallWork = 200f;

		public bool forceShowRoomStats;

		public bool wantsHopperAdjacent;

		public bool allowWireConnection = true;

		public bool shipPart;

		public bool canPlaceOverImpassablePlant = true;

		public float heatPerTickWhileWorking;

		public bool canBuildNonEdificesUnder = true;

		public bool canPlaceOverWall;

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

		public bool isPlayerEjectable;

		public GraphicData fullGraveGraphicData;

		public float bed_healPerDay;

		public bool bed_defaultMedical;

		public bool bed_showSleeperBody;

		public bool bed_humanlike = true;

		public float bed_maxBodySize = 9999f;

		public bool bed_caravansCanUse;

		public float nutritionCostPerDispense;

		public SoundDef soundDispense;

		public ThingDef turretGunDef;

		public float turretBurstWarmupTime;

		public float turretBurstCooldownTime = -1f;

		public float turretInitialCooldownTime;

		[Unsaved(false)]
		public Material turretTopMat;

		public float turretTopDrawSize = 2f;

		public Vector2 turretTopOffset;

		public bool ai_combatDangerous;

		public bool ai_chillDestination = true;

		public SoundDef soundDoorOpenPowered;

		public SoundDef soundDoorClosePowered;

		public SoundDef soundDoorOpenManual;

		public SoundDef soundDoorCloseManual;

		[NoTranslate]
		public string sowTag;

		public ThingDef defaultPlantToGrow;

		public ThingDef mineableThing;

		public int mineableYield = 1;

		public float mineableNonMinedEfficiency = 0.7f;

		public float mineableDropChance = 1f;

		public bool mineableYieldWasteable = true;

		public float mineableScatterCommonality;

		public IntRange mineableScatterLumpSizeRange = new IntRange(20, 40);

		public StorageSettings fixedStorageSettings;

		public StorageSettings defaultStorageSettings;

		public bool ignoreStoredThingsBeauty;

		public bool isTrap;

		public bool trapDestroyOnSpring;

		public float trapPeacefulWildAnimalsSpringChanceFactor = 1f;

		public DamageArmorCategoryDef trapDamageCategory;

		public GraphicData trapUnarmedGraphicData;

		[Unsaved(false)]
		public Graphic trapUnarmedGraphic;

		public float unpoweredWorkTableWorkSpeedFactor;

		public IntRange watchBuildingStandDistanceRange = IntRange.one;

		public int watchBuildingStandRectWidth = 3;

		public bool watchBuildingInSameRoom;

		public JoyKindDef joyKind;

		public int haulToContainerDuration;

		public float instrumentRange;

		public int minDistanceToSameTypeOfBuilding;

		public bool SupportsPlants => sowTag != null;

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
				if (thingDef.building != null && thingDef.building.smoothedThing != null)
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
		}
	}
}
