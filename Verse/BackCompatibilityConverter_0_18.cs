using System;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class BackCompatibilityConverter_0_18 : BackCompatibilityConverter
{
	private static readonly Regex MeatSuffixExtract = new Regex("^(.+)_Meat$");

	private static readonly Regex CorpseSuffixExtract = new Regex("^(.+)_Corpse$");

	private static readonly Regex BlueprintSuffixExtract = new Regex("^(.+)_Blueprint$");

	private static readonly Regex BlueprintInstallSuffixExtract = new Regex("^(.+)_Blueprint_Install$");

	private static readonly Regex FrameSuffixExtract = new Regex("^(.+)_Frame$");

	public override bool AppliesToVersion(int majorVer, int minorVer)
	{
		if (majorVer == 0)
		{
			return minorVer <= 18;
		}
		return false;
	}

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (defType == typeof(ThingDef))
		{
			switch (defName)
			{
			case "Apparel_VestPlate":
				return "Apparel_FlakVest";
			case "Refinery":
				return "BiofuelRefinery";
			case "TrapDeadfall":
				return "TrapSpike";
			case "PsychoidPekoe":
				return "PsychiteTea";
			case "ExplosiveDropPodIncoming":
				return "DropPodIncoming";
			case "MeleeWeapon_Shiv":
				return "MeleeWeapon_Knife";
			case "ScytherBlade":
				return "PowerClaw";
			case "GrizzlyBear":
				return "Bear_Grizzly";
			case "PolarBear":
				return "Bear_Polar";
			case "WolfTimber":
				return "Wolf_Timber";
			case "WolfArctic":
				return "Wolf_Arctic";
			case "FoxFennec":
				return "Fox_Fennec";
			case "FoxRed":
				return "Fox_Red";
			case "FoxArctic":
				return "Fox_Arctic";
			case "GrizzlyBear_Meat":
				return "Meat_Bear_Grizzly";
			case "WolfTimber_Meat":
				return "Meat_Wolf_Timber";
			case "FoxFennec_Meat":
				return "Meat_Fox_Fennec";
			case "GrizzlyBear_Corpse":
				return "Corpse_Bear_Grizzly";
			case "PolarBear_Corpse":
				return "Corpse_Bear_Polar";
			case "WolfTimber_Corpse":
				return "Corpse_Wolf_Timber";
			case "WolfArctic_Corpse":
				return "Corpse_Wolf_Arctic";
			case "FoxFennec_Corpse":
				return "Corpse_Fox_Fennec";
			case "FoxRed_Corpse":
				return "Corpse_Fox_Red";
			case "FoxArctic_Corpse":
				return "Corpse_Fox_Arctic";
			case "TurretGun":
				return "Turret_MiniTurret";
			case "Gun_TurretImprovised":
				return "Gun_MiniTurret";
			case "Bullet_TurretImprovised":
				return "Bullet_MiniTurret";
			case "MinifiedFurniture":
				return "MinifiedThing";
			case "MinifiedSculpture":
				return "MinifiedThing";
			case "HerbalMedicine":
				return "MedicineHerbal";
			case "Medicine":
				return "MedicineIndustrial";
			case "GlitterworldMedicine":
				return "MedicineUltratech";
			case "Component":
				return "ComponentIndustrial";
			case "AdvancedComponent":
				return "ComponentSpacer";
			case "MineableComponents":
				return "MineableComponentsIndustrial";
			case "Mechanoid_Scyther":
				return "Mech_Scyther";
			case "Mechanoid_Centipede":
				return "Mech_Centipede";
			case "Mechanoid_Scyther_Corpse":
				return "Corpse_Mech_Scyther";
			case "Mechanoid_Centipede_Corpse":
				return "Corpse_Mech_Centipede";
			case "ArcticBear":
				return "Bear_Arctic";
			case "ArcticBear_Corpse":
				return "Corpse_Bear_Arctic";
			case "GrizzlyBear_Leather":
				return "Leather_Bear";
			case "PolarBear_Leather":
				return "Leather_Bear";
			case "Cassowary_Leather":
				return "Leather_Bird";
			case "Emu_Leather":
				return "Leather_Bird";
			case "Ostrich_Leather":
				return "Leather_Bird";
			case "Turkey_Leather":
				return "Leather_Bird";
			case "Muffalo_Leather":
				return "Leather_Bluefur";
			case "Dromedary_Leather":
				return "Leather_Camel";
			case "Alpaca_Leather":
				return "Leather_Camel";
			case "Chinchilla_Leather":
				return "Leather_Chinchilla";
			case "Boomalope_Leather":
				return "Leather_Plain";
			case "Cow_Leather":
				return "Leather_Plain";
			case "Gazelle_Leather":
				return "Leather_Plain";
			case "Ibex_Leather":
				return "Leather_Plain";
			case "Deer_Leather":
				return "Leather_Plain";
			case "Elk_Leather":
				return "Leather_Plain";
			case "Caribou_Leather":
				return "Leather_Plain";
			case "YorkshireTerrier_Leather":
				return "Leather_Dog";
			case "Husky_Leather":
				return "Leather_Dog";
			case "LabradorRetriever_Leather":
				return "Leather_Dog";
			case "Elephant_Leather":
				return "Leather_Elephant";
			case "FoxFennec_Leather":
				return "Leather_Fox";
			case "FoxRed_Leather":
				return "Leather_Fox";
			case "FoxArctic_Leather":
				return "Leather_Fox";
			case "Megasloth_Leather":
				return "Leather_Heavy";
			case "Human_Leather":
				return "Leather_Human";
			case "Boomrat_Leather":
				return "Leather_Light";
			case "Cat_Leather":
				return "Leather_Light";
			case "Hare_Leather":
				return "Leather_Light";
			case "Snowhare_Leather":
				return "Leather_Light";
			case "Squirrel_Leather":
				return "Leather_Light";
			case "Alphabeaver_Leather":
				return "Leather_Light";
			case "Capybara_Leather":
				return "Leather_Light";
			case "Raccoon_Leather":
				return "Leather_Light";
			case "Rat_Leather":
				return "Leather_Light";
			case "Monkey_Leather":
				return "Leather_Light";
			case "Iguana_Leather":
				return "Leather_Lizard";
			case "Tortoise_Leather":
				return "Leather_Lizard";
			case "Cobra_Leather":
				return "Leather_Lizard";
			case "Cougar_Leather":
				return "Leather_Panthera";
			case "Panther_Leather":
				return "Leather_Panthera";
			case "Lynx_Leather":
				return "Leather_Panthera";
			case "Pig_Leather":
				return "Leather_Pig";
			case "Rhinoceros_Leather":
				return "Leather_Rhinoceros";
			case "Thrumbo_Leather":
				return "Leather_Thrumbo";
			case "Warg_Leather":
				return "Leather_Wolf";
			case "WolfTimber_Leather":
				return "Leather_Wolf";
			case "WolfArctic_Leather":
				return "Leather_Wolf";
			case "PlantRose":
				return "Plant_Rose";
			case "PlantDaylily":
				return "Plant_Daylily";
			case "PlantRice":
				return "Plant_Rice";
			case "PlantPotato":
				return "Plant_Potato";
			case "PlantCorn":
				return "Plant_Corn";
			case "PlantStrawberry":
				return "Plant_Strawberry";
			case "PlantHaygrass":
				return "Plant_Haygrass";
			case "PlantCotton":
				return "Plant_Cotton";
			case "PlantDevilstrand":
				return "Plant_Devilstrand";
			case "PlantHealroot":
				return "Plant_Healroot";
			case "PlantHops":
				return "Plant_Hops";
			case "PlantSmokeleaf":
				return "Plant_Smokeleaf";
			case "PlantPsychoid":
				return "Plant_Psychoid";
			case "PlantAmbrosia":
				return "Plant_Ambrosia";
			case "PlantAgave":
				return "Plant_Agave";
			case "PlantPincushionCactus":
				return "Plant_PincushionCactus";
			case "PlantSaguaroCactus":
				return "Plant_SaguaroCactus";
			case "PlantTreeDrago":
				return "Plant_TreeDrago";
			case "PlantGrass":
				return "Plant_Grass";
			case "PlantTallGrass":
				return "Plant_TallGrass";
			case "PlantBush":
				return "Plant_Bush";
			case "PlantBrambles":
				return "Plant_Brambles";
			case "PlantWildHealroot":
				return "Plant_HealrootWild";
			case "PlantTreeWillow":
				return "Plant_TreeWillow";
			case "PlantTreeCypress":
				return "Plant_TreeCypress";
			case "PlantTreeMaple":
				return "Plant_TreeMaple";
			case "PlantChokevine":
				return "Plant_Chokevine";
			case "PlantDandelion":
				return "Plant_Dandelion";
			case "PlantAstragalus":
				return "Plant_Astragalus";
			case "PlantMoss":
				return "Plant_Moss";
			case "PlantRaspberry":
				return "Plant_Berry";
			case "PlantTreeOak":
				return "Plant_TreeOak";
			case "PlantTreePoplar":
				return "Plant_TreePoplar";
			case "PlantTreePine":
				return "Plant_TreePine";
			case "PlantTreeBirch":
				return "Plant_TreeBirch";
			case "PlantShrubLow":
				return "Plant_ShrubLow";
			case "PlantAlocasia":
				return "Plant_Alocasia";
			case "PlantClivia":
				return "Plant_Clivia";
			case "PlantRafflesia":
				return "Plant_Rafflesia";
			case "PlantTreeTeak":
				return "Plant_TreeTeak";
			case "PlantTreeCecropia":
				return "Plant_TreeCecropia";
			case "PlantTreePalm":
				return "Plant_TreePalm";
			case "PlantTreeBamboo":
				return "Plant_TreeBamboo";
			case "Plant_Raspberry":
				return "Plant_Berry";
			case "FilthDirt":
				return "Filth_Dirt";
			case "FilthAnimalFilth":
				return "Filth_AnimalFilth";
			case "FilthSand":
				return "Filth_Sand";
			case "FilthBlood":
				return "Filth_Blood";
			case "FilthBloodInsect":
				return "Filth_BloodInsect";
			case "FilthAmnioticFluid":
				return "Filth_AmnioticFluid";
			case "FilthSlime":
				return "Filth_Slime";
			case "FilthVomit":
				return "Filth_Vomit";
			case "FilthFireFoam":
				return "Filth_FireFoam";
			case "FilthFuel":
				return "Filth_Fuel";
			case "FilthCorpseBile":
				return "Filth_CorpseBile";
			case "FilthAsh":
				return "Filth_Ash";
			case "RockRubble":
				return "Filth_RubbleRock";
			case "BuildingRubble":
				return "Filth_RubbleBuilding";
			}
			if (defName.EndsWith("_Meat") && !defName.Contains("Meal"))
			{
				return MeatSuffixExtract.Replace(defName, "Meat_$1");
			}
			if (defName.EndsWith("_Corpse"))
			{
				return CorpseSuffixExtract.Replace(defName, "Corpse_$1");
			}
			if (defName.EndsWith("_Blueprint"))
			{
				return BlueprintSuffixExtract.Replace(defName, "Blueprint_$1");
			}
			if (defName.EndsWith("_Blueprint_Install"))
			{
				return BlueprintInstallSuffixExtract.Replace(defName, "Blueprint_Install_$1");
			}
			if (defName.EndsWith("_Frame"))
			{
				return FrameSuffixExtract.Replace(defName, "Frame_$1");
			}
		}
		else if (defType == typeof(HediffDef))
		{
			if (defName == "ScytherBlade")
			{
				return "PowerClaw";
			}
			if (defName == "PekoeHigh")
			{
				return "PsychiteTeaHigh";
			}
		}
		else if (defType == typeof(WorldObjectDef))
		{
			switch (defName)
			{
			case "FactionBase":
				return "Settlement";
			case "DestroyedFactionBase":
				return "DestroyedSettlement";
			case "AbandonedFactionBase":
			case "AbandonedBase":
				return "AbandonedSettlement";
			}
		}
		else if (defType == typeof(PawnKindDef))
		{
			switch (defName)
			{
			case "SpaceSoldier":
				return "AncientSoldier";
			case "Scyther":
				return "Mech_Scyther";
			case "Centipede":
				return "Mech_Centipede";
			}
		}
		else if (defType == typeof(PawnGroupKindDef))
		{
			if (defName == "FactionBase")
			{
				return "Settlement";
			}
		}
		else if (defType == typeof(IncidentDef))
		{
			switch (defName)
			{
			case "QuestBanditCamp":
				return "Quest_BanditCamp";
			case "QuestItemStash":
				return "Quest_ItemStash";
			case "QuestItemStashGuaranteedCore":
				return "Quest_ItemStashAICore";
			case "QuestDownedRefugee":
				return "Quest_DownedRefugee";
			case "QuestPrisonerWillingToJoin":
				return "Quest_PrisonerRescue";
			case "QuestPeaceTalks":
				return "Quest_PeaceTalks";
			case "JourneyOffer":
				return "Quest_JourneyOffer";
			case "CaravanRequest":
				return "Quest_TradeRequest";
			case "RaidEnemyEscapeShip":
				return "RaidEnemyBeacon";
			}
		}
		else if (defType == typeof(DesignationDef))
		{
			if (defName == "SmoothFloor")
			{
				return "SmoothSurface";
			}
		}
		else if (defType == typeof(FactionDef))
		{
			if (defName == "Spacer")
			{
				return "Ancients";
			}
			if (defName == "SpacerHostile")
			{
				return "AncientsHostile";
			}
		}
		else if (defType == typeof(TerrainDef))
		{
			if (defName == "WaterMovingDeep")
			{
				return "WaterMovingChestDeep";
			}
		}
		else if (defType == typeof(RulePackDef))
		{
			if (defName == "NamerFactionBasePlayerColony")
			{
				return "NamerInitialSettlementColony";
			}
			if (defName == "NamerFactionBasePlayerTribe")
			{
				return "NamerInitialSettlementTribe";
			}
			switch (defName)
			{
			case "NamerFactionBasePlayerTribe":
				return "NamerSettlementPlayerTribe";
			case "NamerFactionBasePlayerColonyRandomized":
				return "NamerSettlementPlayerColonyRandomized";
			case "NamerFactionBasePirate":
				return "NamerSettlementPirate";
			case "NamerFactionBaseOutlander":
				return "NamerSettlementOutlander";
			case "NamerFactionBaseTribal":
				return "NamerSettlementTribal";
			case "ArtName_Sculpture":
				return "NamerArtSculpture";
			case "ArtName_Weapon":
				return "NamerArtWeapon";
			case "ArtName_WeaponMelee":
				return "NamerArtWeaponMelee";
			case "ArtName_WeaponGun":
				return "NamerArtWeaponGun";
			case "ArtName_Furniture":
				return "NamerArtFurniture";
			case "ArtName_SarcophagusPlate":
				return "NamerArtSarcophagusPlate";
			}
		}
		else if (defType == typeof(TraitDef))
		{
			if (defName == "Prosthophile")
			{
				return "Transhumanist";
			}
			if (defName == "Prosthophobe")
			{
				return "BodyPurist";
			}
		}
		else if (defType == typeof(SkillDef))
		{
			if (defName == "Growing")
			{
				return "Plants";
			}
		}
		else if (defType == typeof(BodyPartDef))
		{
			if (!forDefInjections)
			{
				switch (defName)
				{
				case "LeftAntenna":
					return "Antenna";
				case "RightAntenna":
					return "Antenna";
				case "LeftElytra":
					return "Elytra";
				case "RightElytra":
					return "Elytra";
				case "FrontLeftLeg":
					return "Leg";
				case "FrontRightLeg":
					return "Leg";
				case "MiddleLeftLeg":
					return "Leg";
				case "MiddleRightLeg":
					return "Leg";
				case "RearLeftLeg":
					return "Leg";
				case "RearRightLeg":
					return "Leg";
				case "FrontLeftInsectLeg":
					return "InsectLeg";
				case "FrontRightInsectLeg":
					return "InsectLeg";
				case "MiddleLeftInsectLeg":
					return "InsectLeg";
				case "MiddleRightInsectLeg":
					return "InsectLeg";
				case "RearLeftInsectLeg":
					return "InsectLeg";
				case "RearRightInsectLeg":
					return "InsectLeg";
				case "FrontLeftPaw":
					return "Paw";
				case "FrontRightPaw":
					return "Paw";
				case "RearLeftPaw":
					return "Paw";
				case "RearRightPaw":
					return "Paw";
				case "FrontLeftHoof":
					return "Hoof";
				case "FrontRightHoof":
					return "Hoof";
				case "RearLeftHoof":
					return "Hoof";
				case "RearRightHoof":
					return "Hoof";
				case "FrontLeftLegFirstClaw":
					return "FrontClaw";
				case "FrontLeftLegSecondClaw":
					return "FrontClaw";
				case "FrontLeftLegThirdClaw":
					return "FrontClaw";
				case "FrontLeftLegFourthClaw":
					return "FrontClaw";
				case "FrontLeftLegFifthClaw":
					return "FrontClaw";
				case "FrontRightLegFirstClaw":
					return "FrontClaw";
				case "FrontRightLegSecondClaw":
					return "FrontClaw";
				case "FrontRightLegThirdClaw":
					return "FrontClaw";
				case "FrontRightLegFourthClaw":
					return "FrontClaw";
				case "FrontRightLegFifthClaw":
					return "FrontClaw";
				case "RearLeftLegFirstClaw":
					return "RearClaw";
				case "RearLeftLegSecondClaw":
					return "RearClaw";
				case "RearLeftLegThirdClaw":
					return "RearClaw";
				case "RearLeftLegFourthClaw":
					return "RearClaw";
				case "RearLeftLegFifthClaw":
					return "RearClaw";
				case "RearRightLegFirstClaw":
					return "RearClaw";
				case "RearRightLegSecondClaw":
					return "RearClaw";
				case "RearRightLegThirdClaw":
					return "RearClaw";
				case "RearRightLegFourthClaw":
					return "RearClaw";
				case "RearRightLegFifthClaw":
					return "RearClaw";
				case "LeftEye":
					return "Eye";
				case "RightEye":
					return "Eye";
				case "LeftEar":
					return "Ear";
				case "RightEar":
					return "Ear";
				case "LeftLeg":
					return "Leg";
				case "RightLeg":
					return "Leg";
				case "LeftFoot":
					return "Foot";
				case "RightFoot":
					return "Foot";
				case "LeftShoulder":
					return "Shoulder";
				case "RightShoulder":
					return "Shoulder";
				case "LeftArm":
					return "Arm";
				case "RightArm":
					return "Arm";
				case "LeftHand":
					return "Hand";
				case "RightHand":
					return "Hand";
				case "LeftHandPinky":
					return "Finger";
				case "LeftHandRingFinger":
					return "Finger";
				case "LeftHandMiddleFinger":
					return "Finger";
				case "LeftHandIndexFinger":
					return "Finger";
				case "LeftHandThumb":
					return "Finger";
				case "RightHandPinky":
					return "Finger";
				case "RightHandRingFinger":
					return "Finger";
				case "RightHandMiddleFinger":
					return "Finger";
				case "RightHandIndexFinger":
					return "Finger";
				case "RightHandThumb":
					return "Finger";
				case "LeftFootLittleToe":
					return "Toe";
				case "LeftFootFourthToe":
					return "Toe";
				case "LeftFootMiddleToe":
					return "Toe";
				case "LeftFootSecondToe":
					return "Toe";
				case "LeftFootBigToe":
					return "Toe";
				case "RightFootLittleToe":
					return "Toe";
				case "RightFootFourthToe":
					return "Toe";
				case "RightFootMiddleToe":
					return "Toe";
				case "RightFootSecondToe":
					return "Toe";
				case "RightFootBigToe":
					return "Toe";
				case "LeftClavicle":
					return "Clavicle";
				case "RightClavicle":
					return "Clavicle";
				case "LeftHumerus":
					return "Humerus";
				case "RightHumerus":
					return "Humerus";
				case "LeftRadius":
					return "Radius";
				case "RightRadius":
					return "Radius";
				case "LeftFemur":
					return "Femur";
				case "RightFemur":
					return "Femur";
				case "LeftTibia":
					return "Tibia";
				case "RightTibia":
					return "Tibia";
				case "LeftSightSensor":
					return "SightSensor";
				case "RightSightSensor":
					return "SightSensor";
				case "LeftHearingSensor":
					return "HearingSensor";
				case "RightHearingSensor":
					return "HearingSensor";
				case "LeftMechanicalShoulder":
					return "MechanicalShoulder";
				case "RightMechanicalShoulder":
					return "MechanicalShoulder";
				case "LeftMechanicalArm":
					return "MechanicalArm";
				case "RightMechanicalArm":
					return "MechanicalArm";
				case "LeftMechanicalHand":
					return "MechanicalHand";
				case "RightMechanicalHand":
					return "MechanicalHand";
				case "LeftHandMechanicalPinky":
					return "MechanicalFinger";
				case "LeftHandMechanicalMiddleFinger":
					return "MechanicalFinger";
				case "LeftHandMechanicalIndexFinger":
					return "MechanicalFinger";
				case "LeftHandMechanicalThumb":
					return "MechanicalFinger";
				case "RightHandMechanicalPinky":
					return "MechanicalFinger";
				case "RightHandMechanicalMiddleFinger":
					return "MechanicalFinger";
				case "RightHandMechanicalIndexFinger":
					return "MechanicalFinger";
				case "RightHandMechanicalThumb":
					return "MechanicalFinger";
				case "LeftMechanicalLeg":
					return "MechanicalLeg";
				case "RightMechanicalLeg":
					return "MechanicalLeg";
				case "LeftMechanicalFoot":
					return "MechanicalFoot";
				case "RightMechanicalFoot":
					return "MechanicalFoot";
				case "LeftBlade":
					return "Blade";
				case "RightBlade":
					return "Blade";
				case "LeftLung":
					return "Lung";
				case "RightLung":
					return "Lung";
				case "LeftKidney":
					return "Kidney";
				case "RightKidney":
					return "Kidney";
				case "LeftTusk":
					return "Tusk";
				case "RightTusk":
					return "Tusk";
				}
			}
		}
		else if (defType == typeof(ConceptDef))
		{
			if (defName == "MaxNumberOfPlayerHomes")
			{
				return "MaxNumberOfPlayerSettlements";
			}
		}
		else if (defType == typeof(TaleDef) && defName == "RaidArrived")
		{
			return "MajorThreat";
		}
		return null;
	}

	public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (baseType == typeof(WorldObject))
		{
			switch (providedClassName)
			{
			case "RimWorld.Planet.FactionBase":
			case "FactionBase":
				return typeof(Settlement);
			case "RimWorld.Planet.DestroyedFactionBase":
			case "DestroyedFactionBase":
				return typeof(DestroyedSettlement);
			}
		}
		else if (baseType == typeof(Thing) && providedClassName == "Building_TrapRearmable" && node["def"] != null && node["def"].InnerText == "TrapDeadfall")
		{
			return ThingDefOf.TrapSpike.thingClass;
		}
		return null;
	}

	public override void PostExposeData(object obj)
	{
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (obj is Map map)
			{
				if (map.retainedCaravanData == null)
				{
					map.retainedCaravanData = new RetainedCaravanData(map);
				}
				if (map.wildAnimalSpawner == null)
				{
					map.wildAnimalSpawner = new WildAnimalSpawner(map);
				}
				if (map.wildPlantSpawner == null)
				{
					map.wildPlantSpawner = new WildPlantSpawner(map);
				}
			}
			if (obj is Thing thing && thing.def.useHitPoints && thing.MaxHitPoints != thing.HitPoints && Mathf.Abs((float)thing.HitPoints / (float)thing.MaxHitPoints - 0.617f) < 0.02f && thing.Stuff == ThingDefOf.WoodLog)
			{
				thing.HitPoints = thing.MaxHitPoints;
			}
			if (obj is Pawn { Destroyed: false, Dead: false, needs: null } pawn)
			{
				Log.Error(pawn.ToStringSafe() + " has null needs tracker even though he's not dead. Fixing...");
				pawn.needs = new Pawn_NeedsTracker(pawn);
				pawn.needs.SetInitialLevels();
			}
			if (obj is History { archive: null } history)
			{
				history.archive = new Archive();
			}
			if (obj is WorldInfo { persistentRandomValue: 0 } worldInfo)
			{
				worldInfo.persistentRandomValue = Rand.Int;
			}
			if (obj is Caravan caravan)
			{
				if (caravan.forage == null)
				{
					caravan.forage = new Caravan_ForageTracker(caravan);
				}
				if (caravan.needs == null)
				{
					caravan.needs = new Caravan_NeedsTracker(caravan);
				}
				if (caravan.carryTracker == null)
				{
					caravan.carryTracker = new Caravan_CarryTracker(caravan);
				}
				if (caravan.beds == null)
				{
					caravan.beds = new Caravan_BedsTracker(caravan);
				}
			}
			if (obj is PlaySettings playSettings)
			{
				playSettings.defaultCareForColonist = MedicalCareCategory.Best;
				playSettings.defaultCareForTamedAnimal = MedicalCareCategory.HerbalOrWorse;
				playSettings.defaultCareForPrisoner = MedicalCareCategory.HerbalOrWorse;
				playSettings.defaultCareForNeutralFaction = MedicalCareCategory.HerbalOrWorse;
				playSettings.defaultCareForWildlife = MedicalCareCategory.HerbalOrWorse;
				playSettings.defaultCareForHostileFaction = MedicalCareCategory.HerbalOrWorse;
			}
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (obj is Hediff hediff)
			{
				Scribe_Values.Look(ref hediff.temp_partIndexToSetLater, "partIndex", -1);
			}
			if (obj is Bill_Medical bill_Medical)
			{
				Scribe_Values.Look(ref bill_Medical.temp_partIndexToSetLater, "partIndex", -1);
			}
			if (obj is HediffComp_GetsPermanent hediffComp_GetsPermanent)
			{
				bool value = false;
				Scribe_Values.Look(ref value, "isOld", defaultValue: false);
				if (value)
				{
					hediffComp_GetsPermanent.isPermanentInt = true;
				}
			}
			if (obj is World)
			{
				UniqueIDsManager target = null;
				Scribe_Deep.Look(ref target, "uniqueIDsManager");
				if (target != null)
				{
					Current.Game.uniqueIDsManager = target;
				}
			}
			if (obj is WorldFeature { maxDrawSizeInTiles: 0f } worldFeature)
			{
				Vector2 value2 = Vector2.zero;
				Scribe_Values.Look(ref value2, "maxDrawSizeInTiles");
				worldFeature.maxDrawSizeInTiles = value2.x;
			}
		}
		if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
		{
			return;
		}
		if (obj is Hediff { temp_partIndexToSetLater: >=0, pawn: not null } hediff2)
		{
			if (hediff2.temp_partIndexToSetLater == 0)
			{
				hediff2.Part = hediff2.pawn.RaceProps.body.GetPartAtIndex(hediff2.temp_partIndexToSetLater);
			}
			else
			{
				hediff2.pawn.health.hediffSet.hediffs.Remove(hediff2);
			}
			hediff2.temp_partIndexToSetLater = -1;
		}
		if (obj is Bill_Medical bill_Medical2)
		{
			if (bill_Medical2.temp_partIndexToSetLater == 0)
			{
				bill_Medical2.Part = bill_Medical2.GiverPawn.RaceProps.body.GetPartAtIndex(bill_Medical2.temp_partIndexToSetLater);
			}
			else
			{
				bill_Medical2.GiverPawn.BillStack.Bills.Remove(bill_Medical2);
			}
			bill_Medical2.temp_partIndexToSetLater = -1;
		}
	}
}
