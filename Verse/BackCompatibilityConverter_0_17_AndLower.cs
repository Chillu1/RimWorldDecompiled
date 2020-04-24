using RimWorld;
using RimWorld.Planet;
using System;
using System.Xml;
using Verse.AI;

namespace Verse
{
	public class BackCompatibilityConverter_0_17_AndLower : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			if (majorVer == 0)
			{
				return minorVer <= 17;
			}
			return false;
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef))
			{
				if (defName == "Gun_SurvivalRifle")
				{
					return "Gun_BoltActionRifle";
				}
				if (defName == "Bullet_SurvivalRifle")
				{
					return "Bullet_BoltActionRifle";
				}
				if (defName == "Neurotrainer")
				{
					return "MechSerumNeurotrainer";
				}
				if (defName == "FueledGenerator")
				{
					return "WoodFiredGenerator";
				}
				if (defName == "Gun_Pistol")
				{
					return "Gun_Revolver";
				}
				if (defName == "Bullet_Pistol")
				{
					return "Bullet_Revolver";
				}
				if (defName == "TableShort")
				{
					return "Table2x2c";
				}
				if (defName == "TableLong")
				{
					return "Table2x4c";
				}
				if (defName == "TableShort_Blueprint")
				{
					return "Table2x2c_Blueprint";
				}
				if (defName == "TableLong_Blueprint")
				{
					return "Table2x4c_Blueprint";
				}
				if (defName == "TableShort_Frame")
				{
					return "Table2x2c_Frame";
				}
				if (defName == "TableLong_Frame")
				{
					return "Table2x4c_Frame";
				}
				if (defName == "TableShort_Install")
				{
					return "Table2x2c_Install";
				}
				if (defName == "TableLong_Install")
				{
					return "Table2x4c_Install";
				}
				if (defName == "Turret_MortarBomb")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_Incendiary")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarIncendiary")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_EMP")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarEMP")
				{
					return "Turret_Mortar";
				}
				if (defName == "Turret_MortarBomb_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_Incendiary_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarIncendiary_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_EMP_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarEMP_Blueprint")
				{
					return "Turret_Mortar_Blueprint";
				}
				if (defName == "Turret_MortarBomb_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_Incendiary_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarIncendiary_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_EMP_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarEMP_Frame")
				{
					return "Turret_Mortar_Frame";
				}
				if (defName == "Turret_MortarBomb_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_Incendiary_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_MortarIncendiary_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_EMP_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Turret_MortarEMP_Install")
				{
					return "Turret_Mortar_Install";
				}
				if (defName == "Artillery_MortarBomb")
				{
					return "Artillery_Mortar";
				}
				if (defName == "Artillery_MortarIncendiary")
				{
					return "Artillery_Mortar";
				}
				if (defName == "Artillery_MortarEMP")
				{
					return "Artillery_Mortar";
				}
				if (defName == "TrapIEDBomb")
				{
					return "TrapIED_HighExplosive";
				}
				if (defName == "TrapIEDIncendiary")
				{
					return "TrapIED_Incendiary";
				}
				if (defName == "TrapIEDBomb_Blueprint")
				{
					return "TrapIED_HighExplosive_Blueprint";
				}
				if (defName == "TrapIEDIncendiary_Blueprint")
				{
					return "TrapIED_Incendiary_Blueprint";
				}
				if (defName == "TrapIEDBomb_Frame")
				{
					return "TrapIED_HighExplosive_Frame";
				}
				if (defName == "TrapIEDIncendiary_Frame")
				{
					return "TrapIED_Incendiary_Frame";
				}
				if (defName == "TrapIEDBomb_Install")
				{
					return "TrapIED_HighExplosive_Install";
				}
				if (defName == "TrapIEDIncendiary_Install")
				{
					return "TrapIED_Incendiary_Install";
				}
				if (defName == "Bullet_MortarBomb")
				{
					return "Bullet_Shell_HighExplosive";
				}
				if (defName == "Bullet_MortarIncendiary")
				{
					return "Bullet_Shell_Incendiary";
				}
				if (defName == "Bullet_MortarEMP")
				{
					return "Bullet_Shell_EMP";
				}
				if (defName == "MortarShell")
				{
					return "Shell_HighExplosive";
				}
			}
			else if (defType == typeof(ResearchProjectDef))
			{
				if (defName == "IEDBomb")
				{
					return "IEDs";
				}
				if (defName == "Greatbows")
				{
					return "Greatbow";
				}
				if (defName == "Refining")
				{
					return "BiofuelRefining";
				}
				if (defName == "ComponentAssembly")
				{
					return "Fabrication";
				}
				if (defName == "AdvancedAssembly")
				{
					return "AdvancedFabrication";
				}
			}
			else if (defType == typeof(RecipeDef))
			{
				if (defName == "Make_Gun_SurvivalRifle")
				{
					return "Make_Gun_BoltActionRifle";
				}
				if (defName == "Make_Gun_Pistol")
				{
					return "Make_Gun_Revolver";
				}
				if (defName == "Make_TableShort")
				{
					return "Make_Table2x2c";
				}
				if (defName == "Make_TableLong")
				{
					return "Make_Table2x4c";
				}
				if (defName == "MakeMortarShell")
				{
					return "Make_Shell_HighExplosive";
				}
				if (defName == "MakeWort")
				{
					return "Make_Wort";
				}
				if (defName == "MakeKibble")
				{
					return "Make_Kibble";
				}
				if (defName == "MakePemmican")
				{
					return "Make_Pemmican";
				}
				if (defName == "MakePemmicanCampfire")
				{
					return "Make_PemmicanCampfire";
				}
				if (defName == "MakeStoneBlocksAny")
				{
					return "Make_StoneBlocksAny";
				}
				if (defName == "MakeChemfuelFromWood")
				{
					return "Make_ChemfuelFromWood";
				}
				if (defName == "MakeChemfuelFromOrganics")
				{
					return "Make_ChemfuelFromOrganics";
				}
				if (defName == "MakeComponent")
				{
					return "Make_ComponentIndustrial";
				}
				if (defName == "MakeAdvancedComponent")
				{
					return "Make_ComponentSpacer";
				}
				if (defName == "MakePatchleather")
				{
					return "Make_Patchleather";
				}
				if (defName == "MakeStoneBlocksSandstone")
				{
					return "Make_StoneBlocksSandstone";
				}
				if (defName == "MakeStoneBlocksGranite")
				{
					return "Make_StoneBlocksGranite";
				}
				if (defName == "MakeStoneBlocksLimestone")
				{
					return "Make_StoneBlocksLimestone";
				}
				if (defName == "MakeStoneBlocksSlate")
				{
					return "Make_StoneBlocksSlate";
				}
				if (defName == "MakeStoneBlocksMarble")
				{
					return "Make_StoneBlocksMarble";
				}
				if (defName == "Make_Component")
				{
					return "Make_ComponentIndustrial";
				}
				if (defName == "Make_AdvancedComponent")
				{
					return "Make_ComponentSpacer";
				}
			}
			else if (defType == typeof(StatDef))
			{
				if (defName == "GiftImpact")
				{
					return "NegotiationAbility";
				}
				if (defName == "DiplomacyPower")
				{
					return "NegotiationAbility";
				}
				if (defName == "DrugProductionSpeed")
				{
					return "DrugSynthesisSpeed";
				}
				if (defName == "BrewingSpeed")
				{
					return "DrugCookingSpeed";
				}
			}
			else if (defType == typeof(LetterDef))
			{
				if (defName == "BadUrgent")
				{
					return "ThreatBig";
				}
				if (defName == "BadNonUrgent")
				{
					return "NegativeEvent";
				}
				if (defName == "Good")
				{
					return "PositiveEvent";
				}
			}
			else if (defType == typeof(WorldObjectDef))
			{
				if (defName == "JourneyDestination")
				{
					return "EscapeShip";
				}
				if (defName == "AttackedCaravan")
				{
					return "AttackedNonPlayerCaravan";
				}
			}
			else if (defType == typeof(HistoryAutoRecorderDef))
			{
				if (defName == "WealthTotal")
				{
					return "Wealth_Total";
				}
				if (defName == "WealthItems")
				{
					return "Wealth_Items";
				}
				if (defName == "WealthBuildings")
				{
					return "Wealth_Buildings";
				}
				if (defName == "Wealth_TameAnimals")
				{
					return "Wealth_Pawns";
				}
			}
			else if (defType == typeof(InspirationDef))
			{
				if (defName == "GoFrenzy")
				{
					return "Frenzy_Go";
				}
				if (defName == "InspiredArt")
				{
					return "Inspired_Creativity";
				}
				if (defName == "InspiredRecruitment")
				{
					return "Inspired_Recruitment";
				}
				if (defName == "InspiredSurgery")
				{
					return "Inspired_Surgery";
				}
				if (defName == "InspiredTrade")
				{
					return "Inspired_Trade";
				}
				if (defName == "ShootFrenzy")
				{
					return "Frenzy_Shoot";
				}
				if (defName == "WorkFrenzy")
				{
					return "Frenzy_Work";
				}
			}
			else if (defType == typeof(JobDef))
			{
				if (defName == "PlayBilliards")
				{
					return "Play_Billiards";
				}
				if (defName == "PlayChess")
				{
					return "Play_Chess";
				}
				if (defName == "PlayHoopstone")
				{
					return "Play_Hoopstone";
				}
				if (defName == "PlayHorseshoes")
				{
					return "Play_Horseshoes";
				}
				if (defName == "PlayPoker")
				{
					return "Play_Poker";
				}
				if (defName == "WaitCombat")
				{
					return "Wait_Combat";
				}
				if (defName == "WaitDowned")
				{
					return "Wait_Downed";
				}
				if (defName == "WaitMaintainPosture")
				{
					return "Wait_MaintainPosture";
				}
				if (defName == "WaitSafeTemperature")
				{
					return "Wait_SafeTemperature";
				}
				if (defName == "WaitWander")
				{
					return "Wait_Wander";
				}
			}
			else if (defType == typeof(JoyKindDef))
			{
				if (defName == "GamingDexterity")
				{
					return "Gaming_Dexterity";
				}
				if (defName == "GamingCerebral")
				{
					return "Gaming_Cerebral";
				}
			}
			else if (defType == typeof(KeyBindingDef))
			{
				if (defName == "MapDollyUp")
				{
					return "MapDolly_Up";
				}
				if (defName == "MapDollyDown")
				{
					return "MapDolly_Down";
				}
				if (defName == "MapDollyLeft")
				{
					return "MapDolly_Left";
				}
				if (defName == "MapDollyRight")
				{
					return "MapDolly_Right";
				}
				if (defName == "MapZoomIn")
				{
					return "MapZoom_In";
				}
				if (defName == "MapZoomOut")
				{
					return "MapZoom_Out";
				}
				if (defName == "TimeSpeedNormal")
				{
					return "TimeSpeed_Normal";
				}
				if (defName == "TimeSpeedFast")
				{
					return "TimeSpeed_Fast";
				}
				if (defName == "TimeSpeedSuperfast")
				{
					return "TimeSpeed_Superfast";
				}
				if (defName == "TimeSpeedUltrafast")
				{
					return "TimeSpeed_Ultrafast";
				}
				if (defName == "CommandTogglePower")
				{
					return "Command_TogglePower";
				}
				if (defName == "CommandItemForbid")
				{
					return "Command_ItemForbid";
				}
				if (defName == "CommandColonistDraft")
				{
					return "Command_ColonistDraft";
				}
				if (defName == "DesignatorCancel")
				{
					return "Designator_Cancel";
				}
				if (defName == "DesignatorDeconstruct")
				{
					return "Designator_Deconstruct";
				}
				if (defName == "DesignatorRotateLeft")
				{
					return "Designator_RotateLeft";
				}
				if (defName == "DesignatorRotateRight")
				{
					return "Designator_RotateRight";
				}
				if (defName == "ModifierIncrement10x")
				{
					return "ModifierIncrement_10x";
				}
				if (defName == "ModifierIncrement100x")
				{
					return "ModifierIncrement_100x";
				}
				if (defName == "TickOnce")
				{
					return "Dev_TickOnce";
				}
				if (defName == "ToggleGodMode")
				{
					return "Dev_ToggleGodMode";
				}
				if (defName == "ToggleDebugLog")
				{
					return "Dev_ToggleDebugLog";
				}
				if (defName == "ToggleDebugActionsMenu")
				{
					return "Dev_ToggleDebugActionsMenu";
				}
				if (defName == "ToggleDebugActionsMenu")
				{
					return "Dev_ToggleDebugActionsMenu";
				}
				if (defName == "ToggleDebugLogMenu")
				{
					return "Dev_ToggleDebugLogMenu";
				}
				if (defName == "ToggleDebugInspector")
				{
					return "Dev_ToggleDebugInspector";
				}
				if (defName == "ToggleDebugSettingsMenu")
				{
					return "Dev_ToggleDebugSettingsMenu";
				}
			}
			else if (defType == typeof(MentalBreakDef))
			{
				if (defName == "BingingDrugExtreme")
				{
					return "Binging_DrugExtreme";
				}
				if (defName == "BingingDrugMajor")
				{
					return "Binging_DrugMajor";
				}
				if (defName == "BingingFood")
				{
					return "Binging_Food";
				}
				if (defName == "WanderOwnRoom")
				{
					return "Wander_OwnRoom";
				}
				if (defName == "WanderPsychotic")
				{
					return "Wander_Psychotic";
				}
				if (defName == "WanderSad")
				{
					return "Wander_Sad";
				}
			}
			else if (defType == typeof(MentalStateDef))
			{
				if (defName == "BingingDrugExtreme")
				{
					return "Binging_DrugExtreme";
				}
				if (defName == "BingingDrugMajor")
				{
					return "Binging_DrugMajor";
				}
				if (defName == "BingingFood")
				{
					return "Binging_Food";
				}
				if (defName == "WanderOwnRoom")
				{
					return "Wander_OwnRoom";
				}
				if (defName == "WanderPsychotic")
				{
					return "Wander_Psychotic";
				}
				if (defName == "WanderSad")
				{
					return "Wander_Sad";
				}
			}
			else if (defType == typeof(MentalStateDef))
			{
				if (defName == "BingingDrugExtreme")
				{
					return "Binging_DrugExtreme";
				}
				if (defName == "BingingDrugMajor")
				{
					return "Binging_DrugMajor";
				}
				if (defName == "BingingFood")
				{
					return "Binging_Food";
				}
				if (defName == "WanderOwnRoom")
				{
					return "Wander_OwnRoom";
				}
				if (defName == "WanderPsychotic")
				{
					return "Wander_Psychotic";
				}
				if (defName == "WanderSad")
				{
					return "Wander_Sad";
				}
			}
			else if (defType == typeof(PawnKindDef))
			{
				if (defName == "GrenadierDestructive")
				{
					return "Grenadier_Destructive";
				}
				if (defName == "GrenadierEMP")
				{
					return "Grenadier_EMP";
				}
				if (defName == "MercenaryGunner")
				{
					return "Mercenary_Gunner";
				}
				if (defName == "MercenarySniper")
				{
					return "Mercenary_Sniper";
				}
				if (defName == "MercenarySlasher")
				{
					return "Mercenary_Slasher";
				}
				if (defName == "MercenaryHeavy")
				{
					return "Mercenary_Heavy";
				}
				if (defName == "MercenaryElite")
				{
					return "Mercenary_Elite";
				}
				if (defName == "TownCouncilman")
				{
					return "Town_Councilman";
				}
				if (defName == "TownTrader")
				{
					return "Town_Trader";
				}
				if (defName == "TownGuard")
				{
					return "Town_Guard";
				}
				if (defName == "TribalWarrior")
				{
					return "Tribal_Warrior";
				}
				if (defName == "TribalTrader")
				{
					return "Tribal_Trader";
				}
				if (defName == "TribalArcher")
				{
					return "Tribal_Archer";
				}
				if (defName == "TribalHunter")
				{
					return "Tribal_Hunter";
				}
				if (defName == "TribalBerserker")
				{
					return "Tribal_Berserker";
				}
				if (defName == "TribalChief")
				{
					return "Tribal_ChiefRanged";
				}
				if (defName == "GrizzlyBear")
				{
					return "Bear_Grizzly";
				}
				if (defName == "PolarBear")
				{
					return "Bear_Polar";
				}
				if (defName == "ArcticBear")
				{
					return "Bear_Arctic";
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
			}
			else if (defType == typeof(FactionDef))
			{
				if (defName == "Outlander")
				{
					return "OutlanderCivil";
				}
				if (defName == "Tribe")
				{
					return "TribeCivil";
				}
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if (baseType == typeof(WorldObject))
			{
				if ((providedClassName == "RimWorld.Planet.WorldObject" || providedClassName == "WorldObject") && node["def"] != null && node["def"].InnerText == "JourneyDestination")
				{
					return WorldObjectDefOf.EscapeShip.worldObjectClass;
				}
			}
			else if (baseType == typeof(Thing))
			{
				if (providedClassName == "Building_PoisonShipPart" && node["def"] != null && node["def"].InnerText == "CrashedPoisonShipPart")
				{
					return ThingDefOf.MechCapsule.thingClass;
				}
				if (providedClassName == "Building_PsychicEmanator" && node["def"] != null && node["def"].InnerText == "CrashedPsychicEmanatorShipPart")
				{
					return ThingDefOf.MechCapsule.thingClass;
				}
			}
			if (providedClassName == "RimWorld.Planet.FactionBase_TraderTracker" || providedClassName == "FactionBase_TraderTracker")
			{
				return typeof(Settlement_TraderTracker);
			}
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Game game = obj as Game;
				if (game != null && game.battleLog == null)
				{
					game.battleLog = new BattleLog();
				}
				BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = obj as BattleLogEntry_MeleeCombat;
				if (battleLogEntry_MeleeCombat != null)
				{
					if (battleLogEntry_MeleeCombat.RuleDef == null)
					{
						RulePackDef value = null;
						RulePackDef value2 = null;
						Scribe_Defs.Look(ref value, "outcomeRuleDef");
						Scribe_Defs.Look(ref value2, "maneuverRuleDef");
						if (value != null && value2 != null)
						{
							foreach (RulePackDef item in DefDatabase<RulePackDef>.AllDefsListForReading)
							{
								if (!item.include.NullOrEmpty() && item.include.Count == 2 && ((item.include[0] == value && item.include[1] == value2) || (item.include[1] == value && item.include[0] == value2)))
								{
									battleLogEntry_MeleeCombat.RuleDef = item;
									break;
								}
							}
						}
					}
					if (battleLogEntry_MeleeCombat.def == null)
					{
						battleLogEntry_MeleeCombat.def = LogEntryDefOf.MeleeAttack;
					}
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				Map map = obj as Map;
				if (map != null && map.pawnDestinationReservationManager == null)
				{
					map.pawnDestinationReservationManager = new PawnDestinationReservationManager();
				}
				Pawn pawn = obj as Pawn;
				if (pawn != null && pawn.Spawned && pawn.rotationTracker == null)
				{
					pawn.rotationTracker = new Pawn_RotationTracker(pawn);
				}
				Pawn_MindState pawn_MindState = obj as Pawn_MindState;
				if (pawn_MindState != null && pawn_MindState.inspirationHandler == null)
				{
					pawn_MindState.inspirationHandler = new InspirationHandler(pawn_MindState.pawn);
				}
				ImportantPawnComp importantPawnComp = obj as ImportantPawnComp;
				if (importantPawnComp != null && importantPawnComp.pawn == null)
				{
					importantPawnComp.pawn = new ThingOwner<Pawn>(importantPawnComp, oneStackOnly: true);
				}
				Pawn_RecordsTracker pawn_RecordsTracker = obj as Pawn_RecordsTracker;
				if (pawn_RecordsTracker != null && Find.TaleManager.AnyTaleConcerns(pawn_RecordsTracker.pawn))
				{
					pawn_RecordsTracker.AccumulateStoryEvent(StoryEventDefOf.TaleCreated);
				}
				WorldPawns worldPawns = obj as WorldPawns;
				if (worldPawns != null && worldPawns.gc == null)
				{
					worldPawns.gc = new WorldPawnGC();
				}
				GameCondition gameCondition = obj as GameCondition;
				if (gameCondition != null && !gameCondition.Permanent && gameCondition.Duration > 1000000000)
				{
					gameCondition.Permanent = true;
				}
				Building_TurretGun building_TurretGun = obj as Building_TurretGun;
				if (building_TurretGun != null && building_TurretGun.gun == null)
				{
					building_TurretGun.MakeGun();
				}
			}
		}
	}
}
