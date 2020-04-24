using RimWorld;
using RimWorld.Planet;
using System;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace Verse
{
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
				if (defName == "Apparel_VestPlate")
				{
					return "Apparel_FlakVest";
				}
				if (defName == "Refinery")
				{
					return "BiofuelRefinery";
				}
				if (defName == "TrapDeadfall")
				{
					return "TrapSpike";
				}
				if (defName == "PsychoidPekoe")
				{
					return "PsychiteTea";
				}
				if (defName == "ExplosiveDropPodIncoming")
				{
					return "DropPodIncoming";
				}
				if (defName == "MeleeWeapon_Shiv")
				{
					return "MeleeWeapon_Knife";
				}
				if (defName == "ScytherBlade")
				{
					return "PowerClaw";
				}
				if (defName == "GrizzlyBear")
				{
					return "Bear_Grizzly";
				}
				if (defName == "PolarBear")
				{
					return "Bear_Polar";
				}
				if (defName == "WolfTimber")
				{
					return "Wolf_Timber";
				}
				if (defName == "WolfArctic")
				{
					return "Wolf_Arctic";
				}
				if (defName == "FoxFennec")
				{
					return "Fox_Fennec";
				}
				if (defName == "FoxRed")
				{
					return "Fox_Red";
				}
				if (defName == "FoxArctic")
				{
					return "Fox_Arctic";
				}
				if (defName == "GrizzlyBear_Meat")
				{
					return "Meat_Bear_Grizzly";
				}
				if (defName == "WolfTimber_Meat")
				{
					return "Meat_Wolf_Timber";
				}
				if (defName == "FoxFennec_Meat")
				{
					return "Meat_Fox_Fennec";
				}
				if (defName == "GrizzlyBear_Corpse")
				{
					return "Corpse_Bear_Grizzly";
				}
				if (defName == "PolarBear_Corpse")
				{
					return "Corpse_Bear_Polar";
				}
				if (defName == "WolfTimber_Corpse")
				{
					return "Corpse_Wolf_Timber";
				}
				if (defName == "WolfArctic_Corpse")
				{
					return "Corpse_Wolf_Arctic";
				}
				if (defName == "FoxFennec_Corpse")
				{
					return "Corpse_Fox_Fennec";
				}
				if (defName == "FoxRed_Corpse")
				{
					return "Corpse_Fox_Red";
				}
				if (defName == "FoxArctic_Corpse")
				{
					return "Corpse_Fox_Arctic";
				}
				if (defName == "TurretGun")
				{
					return "Turret_MiniTurret";
				}
				if (defName == "Gun_TurretImprovised")
				{
					return "Gun_MiniTurret";
				}
				if (defName == "Bullet_TurretImprovised")
				{
					return "Bullet_MiniTurret";
				}
				if (defName == "MinifiedFurniture")
				{
					return "MinifiedThing";
				}
				if (defName == "MinifiedSculpture")
				{
					return "MinifiedThing";
				}
				if (defName == "HerbalMedicine")
				{
					return "MedicineHerbal";
				}
				if (defName == "Medicine")
				{
					return "MedicineIndustrial";
				}
				if (defName == "GlitterworldMedicine")
				{
					return "MedicineUltratech";
				}
				if (defName == "Component")
				{
					return "ComponentIndustrial";
				}
				if (defName == "AdvancedComponent")
				{
					return "ComponentSpacer";
				}
				if (defName == "MineableComponents")
				{
					return "MineableComponentsIndustrial";
				}
				if (defName == "Mechanoid_Scyther")
				{
					return "Mech_Scyther";
				}
				if (defName == "Mechanoid_Centipede")
				{
					return "Mech_Centipede";
				}
				if (defName == "Mechanoid_Scyther_Corpse")
				{
					return "Corpse_Mech_Scyther";
				}
				if (defName == "Mechanoid_Centipede_Corpse")
				{
					return "Corpse_Mech_Centipede";
				}
				if (defName == "ArcticBear")
				{
					return "Bear_Arctic";
				}
				if (defName == "ArcticBear_Corpse")
				{
					return "Corpse_Bear_Arctic";
				}
				if (defName == "GrizzlyBear_Leather")
				{
					return "Leather_Bear";
				}
				if (defName == "PolarBear_Leather")
				{
					return "Leather_Bear";
				}
				if (defName == "Cassowary_Leather")
				{
					return "Leather_Bird";
				}
				if (defName == "Emu_Leather")
				{
					return "Leather_Bird";
				}
				if (defName == "Ostrich_Leather")
				{
					return "Leather_Bird";
				}
				if (defName == "Turkey_Leather")
				{
					return "Leather_Bird";
				}
				if (defName == "Muffalo_Leather")
				{
					return "Leather_Bluefur";
				}
				if (defName == "Dromedary_Leather")
				{
					return "Leather_Camel";
				}
				if (defName == "Alpaca_Leather")
				{
					return "Leather_Camel";
				}
				if (defName == "Chinchilla_Leather")
				{
					return "Leather_Chinchilla";
				}
				if (defName == "Boomalope_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Cow_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Gazelle_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Ibex_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Deer_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Elk_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "Caribou_Leather")
				{
					return "Leather_Plain";
				}
				if (defName == "YorkshireTerrier_Leather")
				{
					return "Leather_Dog";
				}
				if (defName == "Husky_Leather")
				{
					return "Leather_Dog";
				}
				if (defName == "LabradorRetriever_Leather")
				{
					return "Leather_Dog";
				}
				if (defName == "Elephant_Leather")
				{
					return "Leather_Elephant";
				}
				if (defName == "FoxFennec_Leather")
				{
					return "Leather_Fox";
				}
				if (defName == "FoxRed_Leather")
				{
					return "Leather_Fox";
				}
				if (defName == "FoxArctic_Leather")
				{
					return "Leather_Fox";
				}
				if (defName == "Megasloth_Leather")
				{
					return "Leather_Heavy";
				}
				if (defName == "Human_Leather")
				{
					return "Leather_Human";
				}
				if (defName == "Boomrat_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Cat_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Hare_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Snowhare_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Squirrel_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Alphabeaver_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Capybara_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Raccoon_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Rat_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Monkey_Leather")
				{
					return "Leather_Light";
				}
				if (defName == "Iguana_Leather")
				{
					return "Leather_Lizard";
				}
				if (defName == "Tortoise_Leather")
				{
					return "Leather_Lizard";
				}
				if (defName == "Cobra_Leather")
				{
					return "Leather_Lizard";
				}
				if (defName == "Cougar_Leather")
				{
					return "Leather_Panthera";
				}
				if (defName == "Panther_Leather")
				{
					return "Leather_Panthera";
				}
				if (defName == "Lynx_Leather")
				{
					return "Leather_Panthera";
				}
				if (defName == "Pig_Leather")
				{
					return "Leather_Pig";
				}
				if (defName == "Rhinoceros_Leather")
				{
					return "Leather_Rhinoceros";
				}
				if (defName == "Thrumbo_Leather")
				{
					return "Leather_Thrumbo";
				}
				if (defName == "Warg_Leather")
				{
					return "Leather_Wolf";
				}
				if (defName == "WolfTimber_Leather")
				{
					return "Leather_Wolf";
				}
				if (defName == "WolfArctic_Leather")
				{
					return "Leather_Wolf";
				}
				if (defName == "PlantRose")
				{
					return "Plant_Rose";
				}
				if (defName == "PlantDaylily")
				{
					return "Plant_Daylily";
				}
				if (defName == "PlantRice")
				{
					return "Plant_Rice";
				}
				if (defName == "PlantPotato")
				{
					return "Plant_Potato";
				}
				if (defName == "PlantCorn")
				{
					return "Plant_Corn";
				}
				if (defName == "PlantStrawberry")
				{
					return "Plant_Strawberry";
				}
				if (defName == "PlantHaygrass")
				{
					return "Plant_Haygrass";
				}
				if (defName == "PlantCotton")
				{
					return "Plant_Cotton";
				}
				if (defName == "PlantDevilstrand")
				{
					return "Plant_Devilstrand";
				}
				if (defName == "PlantHealroot")
				{
					return "Plant_Healroot";
				}
				if (defName == "PlantHops")
				{
					return "Plant_Hops";
				}
				if (defName == "PlantSmokeleaf")
				{
					return "Plant_Smokeleaf";
				}
				if (defName == "PlantPsychoid")
				{
					return "Plant_Psychoid";
				}
				if (defName == "PlantAmbrosia")
				{
					return "Plant_Ambrosia";
				}
				if (defName == "PlantAgave")
				{
					return "Plant_Agave";
				}
				if (defName == "PlantPincushionCactus")
				{
					return "Plant_PincushionCactus";
				}
				if (defName == "PlantSaguaroCactus")
				{
					return "Plant_SaguaroCactus";
				}
				if (defName == "PlantTreeDrago")
				{
					return "Plant_TreeDrago";
				}
				if (defName == "PlantGrass")
				{
					return "Plant_Grass";
				}
				if (defName == "PlantTallGrass")
				{
					return "Plant_TallGrass";
				}
				if (defName == "PlantBush")
				{
					return "Plant_Bush";
				}
				if (defName == "PlantBrambles")
				{
					return "Plant_Brambles";
				}
				if (defName == "PlantWildHealroot")
				{
					return "Plant_HealrootWild";
				}
				if (defName == "PlantTreeWillow")
				{
					return "Plant_TreeWillow";
				}
				if (defName == "PlantTreeCypress")
				{
					return "Plant_TreeCypress";
				}
				if (defName == "PlantTreeMaple")
				{
					return "Plant_TreeMaple";
				}
				if (defName == "PlantChokevine")
				{
					return "Plant_Chokevine";
				}
				if (defName == "PlantDandelion")
				{
					return "Plant_Dandelion";
				}
				if (defName == "PlantAstragalus")
				{
					return "Plant_Astragalus";
				}
				if (defName == "PlantMoss")
				{
					return "Plant_Moss";
				}
				if (defName == "PlantRaspberry")
				{
					return "Plant_Berry";
				}
				if (defName == "PlantTreeOak")
				{
					return "Plant_TreeOak";
				}
				if (defName == "PlantTreePoplar")
				{
					return "Plant_TreePoplar";
				}
				if (defName == "PlantTreePine")
				{
					return "Plant_TreePine";
				}
				if (defName == "PlantTreeBirch")
				{
					return "Plant_TreeBirch";
				}
				if (defName == "PlantShrubLow")
				{
					return "Plant_ShrubLow";
				}
				if (defName == "PlantAlocasia")
				{
					return "Plant_Alocasia";
				}
				if (defName == "PlantClivia")
				{
					return "Plant_Clivia";
				}
				if (defName == "PlantRafflesia")
				{
					return "Plant_Rafflesia";
				}
				if (defName == "PlantTreeTeak")
				{
					return "Plant_TreeTeak";
				}
				if (defName == "PlantTreeCecropia")
				{
					return "Plant_TreeCecropia";
				}
				if (defName == "PlantTreePalm")
				{
					return "Plant_TreePalm";
				}
				if (defName == "PlantTreeBamboo")
				{
					return "Plant_TreeBamboo";
				}
				if (defName == "Plant_Raspberry")
				{
					return "Plant_Berry";
				}
				if (defName == "FilthDirt")
				{
					return "Filth_Dirt";
				}
				if (defName == "FilthAnimalFilth")
				{
					return "Filth_AnimalFilth";
				}
				if (defName == "FilthSand")
				{
					return "Filth_Sand";
				}
				if (defName == "FilthBlood")
				{
					return "Filth_Blood";
				}
				if (defName == "FilthBloodInsect")
				{
					return "Filth_BloodInsect";
				}
				if (defName == "FilthAmnioticFluid")
				{
					return "Filth_AmnioticFluid";
				}
				if (defName == "FilthSlime")
				{
					return "Filth_Slime";
				}
				if (defName == "FilthVomit")
				{
					return "Filth_Vomit";
				}
				if (defName == "FilthFireFoam")
				{
					return "Filth_FireFoam";
				}
				if (defName == "FilthFuel")
				{
					return "Filth_Fuel";
				}
				if (defName == "FilthCorpseBile")
				{
					return "Filth_CorpseBile";
				}
				if (defName == "FilthAsh")
				{
					return "Filth_Ash";
				}
				if (defName == "RockRubble")
				{
					return "Filth_RubbleRock";
				}
				if (defName == "BuildingRubble")
				{
					return "Filth_RubbleBuilding";
				}
				if (defName.EndsWith("_Meat"))
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
				if (defName == "FactionBase")
				{
					return "Settlement";
				}
				if (defName == "DestroyedFactionBase")
				{
					return "DestroyedSettlement";
				}
				if (defName == "AbandonedFactionBase" || defName == "AbandonedBase")
				{
					return "AbandonedSettlement";
				}
			}
			else if (defType == typeof(PawnKindDef))
			{
				if (defName == "SpaceSoldier")
				{
					return "AncientSoldier";
				}
				if (defName == "Scyther")
				{
					return "Mech_Scyther";
				}
				if (defName == "Centipede")
				{
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
				if (defName == "QuestBanditCamp")
				{
					return "Quest_BanditCamp";
				}
				if (defName == "QuestItemStash")
				{
					return "Quest_ItemStash";
				}
				if (defName == "QuestItemStashGuaranteedCore")
				{
					return "Quest_ItemStashAICore";
				}
				if (defName == "QuestDownedRefugee")
				{
					return "Quest_DownedRefugee";
				}
				if (defName == "QuestPrisonerWillingToJoin")
				{
					return "Quest_PrisonerRescue";
				}
				if (defName == "QuestPeaceTalks")
				{
					return "Quest_PeaceTalks";
				}
				if (defName == "JourneyOffer")
				{
					return "Quest_JourneyOffer";
				}
				if (defName == "CaravanRequest")
				{
					return "Quest_TradeRequest";
				}
				if (defName == "RaidEnemyEscapeShip")
				{
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
				if (defName == "NamerFactionBasePlayerTribe")
				{
					return "NamerSettlementPlayerTribe";
				}
				if (defName == "NamerFactionBasePlayerColonyRandomized")
				{
					return "NamerSettlementPlayerColonyRandomized";
				}
				if (defName == "NamerFactionBasePirate")
				{
					return "NamerSettlementPirate";
				}
				if (defName == "NamerFactionBaseOutlander")
				{
					return "NamerSettlementOutlander";
				}
				if (defName == "NamerFactionBaseTribal")
				{
					return "NamerSettlementTribal";
				}
				if (defName == "ArtName_Sculpture")
				{
					return "NamerArtSculpture";
				}
				if (defName == "ArtName_Weapon")
				{
					return "NamerArtWeapon";
				}
				if (defName == "ArtName_WeaponMelee")
				{
					return "NamerArtWeaponMelee";
				}
				if (defName == "ArtName_WeaponGun")
				{
					return "NamerArtWeaponGun";
				}
				if (defName == "ArtName_Furniture")
				{
					return "NamerArtFurniture";
				}
				if (defName == "ArtName_SarcophagusPlate")
				{
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
					if (defName == "LeftAntenna")
					{
						return "Antenna";
					}
					if (defName == "RightAntenna")
					{
						return "Antenna";
					}
					if (defName == "LeftElytra")
					{
						return "Elytra";
					}
					if (defName == "RightElytra")
					{
						return "Elytra";
					}
					if (defName == "FrontLeftLeg")
					{
						return "Leg";
					}
					if (defName == "FrontRightLeg")
					{
						return "Leg";
					}
					if (defName == "MiddleLeftLeg")
					{
						return "Leg";
					}
					if (defName == "MiddleRightLeg")
					{
						return "Leg";
					}
					if (defName == "RearLeftLeg")
					{
						return "Leg";
					}
					if (defName == "RearRightLeg")
					{
						return "Leg";
					}
					if (defName == "FrontLeftInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "FrontRightInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "MiddleLeftInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "MiddleRightInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "RearLeftInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "RearRightInsectLeg")
					{
						return "InsectLeg";
					}
					if (defName == "FrontLeftPaw")
					{
						return "Paw";
					}
					if (defName == "FrontRightPaw")
					{
						return "Paw";
					}
					if (defName == "RearLeftPaw")
					{
						return "Paw";
					}
					if (defName == "RearRightPaw")
					{
						return "Paw";
					}
					if (defName == "FrontLeftHoof")
					{
						return "Hoof";
					}
					if (defName == "FrontRightHoof")
					{
						return "Hoof";
					}
					if (defName == "RearLeftHoof")
					{
						return "Hoof";
					}
					if (defName == "RearRightHoof")
					{
						return "Hoof";
					}
					if (defName == "FrontLeftLegFirstClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontLeftLegSecondClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontLeftLegThirdClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontLeftLegFourthClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontLeftLegFifthClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontRightLegFirstClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontRightLegSecondClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontRightLegThirdClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontRightLegFourthClaw")
					{
						return "FrontClaw";
					}
					if (defName == "FrontRightLegFifthClaw")
					{
						return "FrontClaw";
					}
					if (defName == "RearLeftLegFirstClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearLeftLegSecondClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearLeftLegThirdClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearLeftLegFourthClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearLeftLegFifthClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearRightLegFirstClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearRightLegSecondClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearRightLegThirdClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearRightLegFourthClaw")
					{
						return "RearClaw";
					}
					if (defName == "RearRightLegFifthClaw")
					{
						return "RearClaw";
					}
					if (defName == "LeftEye")
					{
						return "Eye";
					}
					if (defName == "RightEye")
					{
						return "Eye";
					}
					if (defName == "LeftEar")
					{
						return "Ear";
					}
					if (defName == "RightEar")
					{
						return "Ear";
					}
					if (defName == "LeftLeg")
					{
						return "Leg";
					}
					if (defName == "RightLeg")
					{
						return "Leg";
					}
					if (defName == "LeftFoot")
					{
						return "Foot";
					}
					if (defName == "RightFoot")
					{
						return "Foot";
					}
					if (defName == "LeftShoulder")
					{
						return "Shoulder";
					}
					if (defName == "RightShoulder")
					{
						return "Shoulder";
					}
					if (defName == "LeftArm")
					{
						return "Arm";
					}
					if (defName == "RightArm")
					{
						return "Arm";
					}
					if (defName == "LeftHand")
					{
						return "Hand";
					}
					if (defName == "RightHand")
					{
						return "Hand";
					}
					if (defName == "LeftHandPinky")
					{
						return "Finger";
					}
					if (defName == "LeftHandRingFinger")
					{
						return "Finger";
					}
					if (defName == "LeftHandMiddleFinger")
					{
						return "Finger";
					}
					if (defName == "LeftHandIndexFinger")
					{
						return "Finger";
					}
					if (defName == "LeftHandThumb")
					{
						return "Finger";
					}
					if (defName == "RightHandPinky")
					{
						return "Finger";
					}
					if (defName == "RightHandRingFinger")
					{
						return "Finger";
					}
					if (defName == "RightHandMiddleFinger")
					{
						return "Finger";
					}
					if (defName == "RightHandIndexFinger")
					{
						return "Finger";
					}
					if (defName == "RightHandThumb")
					{
						return "Finger";
					}
					if (defName == "LeftFootLittleToe")
					{
						return "Toe";
					}
					if (defName == "LeftFootFourthToe")
					{
						return "Toe";
					}
					if (defName == "LeftFootMiddleToe")
					{
						return "Toe";
					}
					if (defName == "LeftFootSecondToe")
					{
						return "Toe";
					}
					if (defName == "LeftFootBigToe")
					{
						return "Toe";
					}
					if (defName == "RightFootLittleToe")
					{
						return "Toe";
					}
					if (defName == "RightFootFourthToe")
					{
						return "Toe";
					}
					if (defName == "RightFootMiddleToe")
					{
						return "Toe";
					}
					if (defName == "RightFootSecondToe")
					{
						return "Toe";
					}
					if (defName == "RightFootBigToe")
					{
						return "Toe";
					}
					if (defName == "LeftClavicle")
					{
						return "Clavicle";
					}
					if (defName == "RightClavicle")
					{
						return "Clavicle";
					}
					if (defName == "LeftHumerus")
					{
						return "Humerus";
					}
					if (defName == "RightHumerus")
					{
						return "Humerus";
					}
					if (defName == "LeftRadius")
					{
						return "Radius";
					}
					if (defName == "RightRadius")
					{
						return "Radius";
					}
					if (defName == "LeftFemur")
					{
						return "Femur";
					}
					if (defName == "RightFemur")
					{
						return "Femur";
					}
					if (defName == "LeftTibia")
					{
						return "Tibia";
					}
					if (defName == "RightTibia")
					{
						return "Tibia";
					}
					if (defName == "LeftSightSensor")
					{
						return "SightSensor";
					}
					if (defName == "RightSightSensor")
					{
						return "SightSensor";
					}
					if (defName == "LeftHearingSensor")
					{
						return "HearingSensor";
					}
					if (defName == "RightHearingSensor")
					{
						return "HearingSensor";
					}
					if (defName == "LeftMechanicalShoulder")
					{
						return "MechanicalShoulder";
					}
					if (defName == "RightMechanicalShoulder")
					{
						return "MechanicalShoulder";
					}
					if (defName == "LeftMechanicalArm")
					{
						return "MechanicalArm";
					}
					if (defName == "RightMechanicalArm")
					{
						return "MechanicalArm";
					}
					if (defName == "LeftMechanicalHand")
					{
						return "MechanicalHand";
					}
					if (defName == "RightMechanicalHand")
					{
						return "MechanicalHand";
					}
					if (defName == "LeftHandMechanicalPinky")
					{
						return "MechanicalFinger";
					}
					if (defName == "LeftHandMechanicalMiddleFinger")
					{
						return "MechanicalFinger";
					}
					if (defName == "LeftHandMechanicalIndexFinger")
					{
						return "MechanicalFinger";
					}
					if (defName == "LeftHandMechanicalThumb")
					{
						return "MechanicalFinger";
					}
					if (defName == "RightHandMechanicalPinky")
					{
						return "MechanicalFinger";
					}
					if (defName == "RightHandMechanicalMiddleFinger")
					{
						return "MechanicalFinger";
					}
					if (defName == "RightHandMechanicalIndexFinger")
					{
						return "MechanicalFinger";
					}
					if (defName == "RightHandMechanicalThumb")
					{
						return "MechanicalFinger";
					}
					if (defName == "LeftMechanicalLeg")
					{
						return "MechanicalLeg";
					}
					if (defName == "RightMechanicalLeg")
					{
						return "MechanicalLeg";
					}
					if (defName == "LeftMechanicalFoot")
					{
						return "MechanicalFoot";
					}
					if (defName == "RightMechanicalFoot")
					{
						return "MechanicalFoot";
					}
					if (defName == "LeftBlade")
					{
						return "Blade";
					}
					if (defName == "RightBlade")
					{
						return "Blade";
					}
					if (defName == "LeftLung")
					{
						return "Lung";
					}
					if (defName == "RightLung")
					{
						return "Lung";
					}
					if (defName == "LeftKidney")
					{
						return "Kidney";
					}
					if (defName == "RightKidney")
					{
						return "Kidney";
					}
					if (defName == "LeftTusk")
					{
						return "Tusk";
					}
					if (defName == "RightTusk")
					{
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
				if (providedClassName == "RimWorld.Planet.FactionBase" || providedClassName == "FactionBase")
				{
					return typeof(Settlement);
				}
				if (providedClassName == "RimWorld.Planet.DestroyedFactionBase" || providedClassName == "DestroyedFactionBase")
				{
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
				Map map = obj as Map;
				if (map != null)
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
				Thing thing = obj as Thing;
				if (thing != null && thing.def.useHitPoints && thing.MaxHitPoints != thing.HitPoints && Mathf.Abs((float)thing.HitPoints / (float)thing.MaxHitPoints - 0.617f) < 0.02f && thing.Stuff == ThingDefOf.WoodLog)
				{
					thing.HitPoints = thing.MaxHitPoints;
				}
				Pawn pawn = obj as Pawn;
				if (pawn != null && !pawn.Destroyed && !pawn.Dead && pawn.needs == null)
				{
					Log.Error(pawn.ToStringSafe() + " has null needs tracker even though he's not dead. Fixing...");
					pawn.needs = new Pawn_NeedsTracker(pawn);
					pawn.needs.SetInitialLevels();
				}
				History history = obj as History;
				if (history != null && history.archive == null)
				{
					history.archive = new Archive();
				}
				WorldInfo worldInfo = obj as WorldInfo;
				if (worldInfo != null && worldInfo.persistentRandomValue == 0)
				{
					worldInfo.persistentRandomValue = Rand.Int;
				}
				Caravan caravan = obj as Caravan;
				if (caravan != null)
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
				PlaySettings playSettings = obj as PlaySettings;
				if (playSettings != null)
				{
					playSettings.defaultCareForColonyHumanlike = MedicalCareCategory.Best;
					playSettings.defaultCareForColonyAnimal = MedicalCareCategory.HerbalOrWorse;
					playSettings.defaultCareForColonyPrisoner = MedicalCareCategory.HerbalOrWorse;
					playSettings.defaultCareForNeutralFaction = MedicalCareCategory.HerbalOrWorse;
					playSettings.defaultCareForNeutralAnimal = MedicalCareCategory.HerbalOrWorse;
					playSettings.defaultCareForHostileFaction = MedicalCareCategory.HerbalOrWorse;
				}
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Hediff hediff = obj as Hediff;
				if (hediff != null)
				{
					Scribe_Values.Look(ref hediff.temp_partIndexToSetLater, "partIndex", -1);
				}
				Bill_Medical bill_Medical = obj as Bill_Medical;
				if (bill_Medical != null)
				{
					Scribe_Values.Look(ref bill_Medical.temp_partIndexToSetLater, "partIndex", -1);
				}
				FactionRelation factionRelation = obj as FactionRelation;
				if (factionRelation != null)
				{
					bool value = false;
					Scribe_Values.Look(ref value, "hostile", defaultValue: false);
					if (value || factionRelation.goodwill <= -75)
					{
						factionRelation.kind = FactionRelationKind.Hostile;
					}
					else if (factionRelation.goodwill >= 75)
					{
						factionRelation.kind = FactionRelationKind.Ally;
					}
				}
				HediffComp_GetsPermanent hediffComp_GetsPermanent = obj as HediffComp_GetsPermanent;
				if (hediffComp_GetsPermanent != null)
				{
					bool value2 = false;
					Scribe_Values.Look(ref value2, "isOld", defaultValue: false);
					if (value2)
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
				WorldFeature worldFeature = obj as WorldFeature;
				if (worldFeature != null && worldFeature.maxDrawSizeInTiles == 0f)
				{
					Vector2 value3 = Vector2.zero;
					Scribe_Values.Look(ref value3, "maxDrawSizeInTiles");
					worldFeature.maxDrawSizeInTiles = value3.x;
				}
			}
			if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
			{
				return;
			}
			Hediff hediff2 = obj as Hediff;
			if (hediff2 != null && hediff2.temp_partIndexToSetLater >= 0 && hediff2.pawn != null)
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
			Bill_Medical bill_Medical2 = obj as Bill_Medical;
			if (bill_Medical2 != null)
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
}
