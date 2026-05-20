using System;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace Verse;

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
			switch (defName)
			{
			case "Gun_SurvivalRifle":
				return "Gun_BoltActionRifle";
			case "Bullet_SurvivalRifle":
				return "Bullet_BoltActionRifle";
			case "Neurotrainer":
				return "MechSerumNeurotrainer";
			case "FueledGenerator":
				return "WoodFiredGenerator";
			case "Gun_Pistol":
				return "Gun_Revolver";
			case "Bullet_Pistol":
				return "Bullet_Revolver";
			case "TableShort":
				return "Table2x2c";
			case "TableLong":
				return "Table2x4c";
			case "TableShort_Blueprint":
				return "Table2x2c_Blueprint";
			case "TableLong_Blueprint":
				return "Table2x4c_Blueprint";
			case "TableShort_Frame":
				return "Table2x2c_Frame";
			case "TableLong_Frame":
				return "Table2x4c_Frame";
			case "TableShort_Install":
				return "Table2x2c_Install";
			case "TableLong_Install":
				return "Table2x4c_Install";
			case "Turret_MortarBomb":
				return "Turret_Mortar";
			case "Turret_Incendiary":
				return "Turret_Mortar";
			case "Turret_MortarIncendiary":
				return "Turret_Mortar";
			case "Turret_EMP":
				return "Turret_Mortar";
			case "Turret_MortarEMP":
				return "Turret_Mortar";
			case "Turret_MortarBomb_Blueprint":
				return "Turret_Mortar_Blueprint";
			case "Turret_Incendiary_Blueprint":
				return "Turret_Mortar_Blueprint";
			case "Turret_MortarIncendiary_Blueprint":
				return "Turret_Mortar_Blueprint";
			case "Turret_EMP_Blueprint":
				return "Turret_Mortar_Blueprint";
			case "Turret_MortarEMP_Blueprint":
				return "Turret_Mortar_Blueprint";
			case "Turret_MortarBomb_Frame":
				return "Turret_Mortar_Frame";
			case "Turret_Incendiary_Frame":
				return "Turret_Mortar_Frame";
			case "Turret_MortarIncendiary_Frame":
				return "Turret_Mortar_Frame";
			case "Turret_EMP_Frame":
				return "Turret_Mortar_Frame";
			case "Turret_MortarEMP_Frame":
				return "Turret_Mortar_Frame";
			case "Turret_MortarBomb_Install":
				return "Turret_Mortar_Install";
			case "Turret_Incendiary_Install":
				return "Turret_Mortar_Install";
			case "Turret_MortarIncendiary_Install":
				return "Turret_Mortar_Install";
			case "Turret_EMP_Install":
				return "Turret_Mortar_Install";
			case "Turret_MortarEMP_Install":
				return "Turret_Mortar_Install";
			case "Artillery_MortarBomb":
				return "Artillery_Mortar";
			case "Artillery_MortarIncendiary":
				return "Artillery_Mortar";
			case "Artillery_MortarEMP":
				return "Artillery_Mortar";
			case "TrapIEDBomb":
				return "TrapIED_HighExplosive";
			case "TrapIEDIncendiary":
				return "TrapIED_Incendiary";
			case "TrapIEDBomb_Blueprint":
				return "TrapIED_HighExplosive_Blueprint";
			case "TrapIEDIncendiary_Blueprint":
				return "TrapIED_Incendiary_Blueprint";
			case "TrapIEDBomb_Frame":
				return "TrapIED_HighExplosive_Frame";
			case "TrapIEDIncendiary_Frame":
				return "TrapIED_Incendiary_Frame";
			case "TrapIEDBomb_Install":
				return "TrapIED_HighExplosive_Install";
			case "TrapIEDIncendiary_Install":
				return "TrapIED_Incendiary_Install";
			case "Bullet_MortarBomb":
				return "Bullet_Shell_HighExplosive";
			case "Bullet_MortarIncendiary":
				return "Bullet_Shell_Incendiary";
			case "Bullet_MortarEMP":
				return "Bullet_Shell_EMP";
			case "MortarShell":
				return "Shell_HighExplosive";
			}
		}
		else if (defType == typeof(ResearchProjectDef))
		{
			switch (defName)
			{
			case "IEDBomb":
				return "IEDs";
			case "Greatbows":
				return "Greatbow";
			case "Refining":
				return "BiofuelRefining";
			case "ComponentAssembly":
				return "Fabrication";
			case "AdvancedAssembly":
				return "AdvancedFabrication";
			}
		}
		else if (defType == typeof(RecipeDef))
		{
			switch (defName)
			{
			case "Make_Gun_SurvivalRifle":
				return "Make_Gun_BoltActionRifle";
			case "Make_Gun_Pistol":
				return "Make_Gun_Revolver";
			case "Make_TableShort":
				return "Make_Table2x2c";
			case "Make_TableLong":
				return "Make_Table2x4c";
			case "MakeMortarShell":
				return "Make_Shell_HighExplosive";
			case "MakeWort":
				return "Make_Wort";
			case "MakeKibble":
				return "Make_Kibble";
			case "MakePemmican":
				return "Make_Pemmican";
			case "MakePemmicanCampfire":
				return "Make_PemmicanCampfire";
			case "MakeStoneBlocksAny":
				return "Make_StoneBlocksAny";
			case "MakeChemfuelFromWood":
				return "Make_ChemfuelFromWood";
			case "MakeChemfuelFromOrganics":
				return "Make_ChemfuelFromOrganics";
			case "MakeComponent":
				return "Make_ComponentIndustrial";
			case "MakeAdvancedComponent":
				return "Make_ComponentSpacer";
			case "MakePatchleather":
				return "Make_Patchleather";
			case "MakeStoneBlocksSandstone":
				return "Make_StoneBlocksSandstone";
			case "MakeStoneBlocksGranite":
				return "Make_StoneBlocksGranite";
			case "MakeStoneBlocksLimestone":
				return "Make_StoneBlocksLimestone";
			case "MakeStoneBlocksSlate":
				return "Make_StoneBlocksSlate";
			case "MakeStoneBlocksMarble":
				return "Make_StoneBlocksMarble";
			case "Make_Component":
				return "Make_ComponentIndustrial";
			case "Make_AdvancedComponent":
				return "Make_ComponentSpacer";
			}
		}
		else if (defType == typeof(StatDef))
		{
			switch (defName)
			{
			case "GiftImpact":
				return "NegotiationAbility";
			case "DiplomacyPower":
				return "NegotiationAbility";
			case "DrugProductionSpeed":
				return "DrugSynthesisSpeed";
			case "BrewingSpeed":
				return "DrugCookingSpeed";
			}
		}
		else if (defType == typeof(LetterDef))
		{
			switch (defName)
			{
			case "BadUrgent":
				return "ThreatBig";
			case "BadNonUrgent":
				return "NegativeEvent";
			case "Good":
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
			switch (defName)
			{
			case "WealthTotal":
				return "Wealth_Total";
			case "WealthItems":
				return "Wealth_Items";
			case "WealthBuildings":
				return "Wealth_Buildings";
			case "Wealth_TameAnimals":
				return "Wealth_Pawns";
			}
		}
		else if (defType == typeof(InspirationDef))
		{
			switch (defName)
			{
			case "GoFrenzy":
				return "Frenzy_Go";
			case "InspiredArt":
				return "Inspired_Creativity";
			case "InspiredRecruitment":
				return "Inspired_Recruitment";
			case "InspiredSurgery":
				return "Inspired_Surgery";
			case "InspiredTrade":
				return "Inspired_Trade";
			case "ShootFrenzy":
				return "Frenzy_Shoot";
			case "WorkFrenzy":
				return "Frenzy_Work";
			}
		}
		else if (defType == typeof(JobDef))
		{
			switch (defName)
			{
			case "PlayBilliards":
				return "Play_Billiards";
			case "PlayChess":
				return "Play_Chess";
			case "PlayHoopstone":
				return "Play_Hoopstone";
			case "PlayHorseshoes":
				return "Play_Horseshoes";
			case "PlayPoker":
				return "Play_Poker";
			case "WaitCombat":
				return "Wait_Combat";
			case "WaitDowned":
				return "Wait_Downed";
			case "WaitMaintainPosture":
				return "Wait_MaintainPosture";
			case "WaitSafeTemperature":
				return "Wait_SafeTemperature";
			case "WaitWander":
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
			switch (defName)
			{
			case "ToggleDebugActionsMenu":
				return "Dev_ToggleDebugActionsMenu";
			case "ToggleDebugLogMenu":
				return "Dev_ToggleDebugLogMenu";
			case "ToggleDebugInspector":
				return "Dev_ToggleDebugInspector";
			case "ToggleDebugSettingsMenu":
				return "Dev_ToggleDebugSettingsMenu";
			}
		}
		else if (defType == typeof(MentalBreakDef))
		{
			switch (defName)
			{
			case "BingingDrugExtreme":
				return "Binging_DrugExtreme";
			case "BingingDrugMajor":
				return "Binging_DrugMajor";
			case "BingingFood":
				return "Binging_Food";
			case "WanderOwnRoom":
				return "Wander_OwnRoom";
			case "WanderPsychotic":
				return "Wander_Psychotic";
			case "WanderSad":
				return "Wander_Sad";
			}
		}
		else if (defType == typeof(MentalStateDef))
		{
			switch (defName)
			{
			case "BingingDrugExtreme":
				return "Binging_DrugExtreme";
			case "BingingDrugMajor":
				return "Binging_DrugMajor";
			case "BingingFood":
				return "Binging_Food";
			case "WanderOwnRoom":
				return "Wander_OwnRoom";
			case "WanderPsychotic":
				return "Wander_Psychotic";
			case "WanderSad":
				return "Wander_Sad";
			}
		}
		else if (defType == typeof(MentalStateDef))
		{
			switch (defName)
			{
			case "BingingDrugExtreme":
				return "Binging_DrugExtreme";
			case "BingingDrugMajor":
				return "Binging_DrugMajor";
			case "BingingFood":
				return "Binging_Food";
			case "WanderOwnRoom":
				return "Wander_OwnRoom";
			case "WanderPsychotic":
				return "Wander_Psychotic";
			case "WanderSad":
				return "Wander_Sad";
			}
		}
		else if (defType == typeof(PawnKindDef))
		{
			switch (defName)
			{
			case "GrenadierDestructive":
				return "Grenadier_Destructive";
			case "GrenadierEMP":
				return "Grenadier_EMP";
			case "MercenaryGunner":
				return "Mercenary_Gunner";
			case "MercenarySniper":
				return "Mercenary_Sniper";
			case "MercenarySlasher":
				return "Mercenary_Slasher";
			case "MercenaryHeavy":
				return "Mercenary_Heavy";
			case "MercenaryElite":
				return "Mercenary_Elite";
			case "TownCouncilman":
				return "Town_Councilman";
			case "TownTrader":
				return "Town_Trader";
			case "TownGuard":
				return "Town_Guard";
			case "TribalWarrior":
				return "Tribal_Warrior";
			case "TribalTrader":
				return "Tribal_Trader";
			case "TribalArcher":
				return "Tribal_Archer";
			case "TribalHunter":
				return "Tribal_Hunter";
			case "TribalBerserker":
				return "Tribal_Berserker";
			case "TribalChief":
				return "Tribal_ChiefRanged";
			case "GrizzlyBear":
				return "Bear_Grizzly";
			case "PolarBear":
				return "Bear_Polar";
			case "ArcticBear":
				return "Bear_Arctic";
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
			if (obj is Game { battleLog: null } game)
			{
				game.battleLog = new BattleLog();
			}
			if (obj is BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat)
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
			if (obj is Map { pawnDestinationReservationManager: null } map)
			{
				map.pawnDestinationReservationManager = new PawnDestinationReservationManager();
			}
			if (obj is Pawn { Spawned: not false, rotationTracker: null } pawn)
			{
				pawn.rotationTracker = new Pawn_RotationTracker(pawn);
			}
			if (obj is Pawn_MindState { inspirationHandler: null } pawn_MindState)
			{
				pawn_MindState.inspirationHandler = new InspirationHandler(pawn_MindState.pawn);
			}
			if (obj is ImportantPawnComp { pawn: null } importantPawnComp)
			{
				importantPawnComp.pawn = new ThingOwner<Pawn>(importantPawnComp, oneStackOnly: true);
			}
			if (obj is WorldPawns { gc: null } worldPawns)
			{
				worldPawns.gc = new WorldPawnGC();
			}
			if (obj is GameCondition { Permanent: false, Duration: >1000000000 } gameCondition)
			{
				gameCondition.Permanent = true;
			}
			if (obj is Building_TurretGun { gun: null } building_TurretGun)
			{
				building_TurretGun.MakeGun();
			}
		}
	}
}
