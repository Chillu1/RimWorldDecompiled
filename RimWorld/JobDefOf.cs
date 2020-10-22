using Verse;

namespace RimWorld
{
	[DefOf]
	public static class JobDefOf
	{
		public static JobDef Goto;

		public static JobDef Wait;

		public static JobDef Wait_MaintainPosture;

		public static JobDef Wait_Downed;

		public static JobDef GotoWander;

		public static JobDef Wait_Wander;

		public static JobDef GotoSafeTemperature;

		public static JobDef Wait_SafeTemperature;

		public static JobDef Wait_Combat;

		public static JobDef Equip;

		public static JobDef AttackMelee;

		public static JobDef AttackStatic;

		public static JobDef UseVerbOnThing;

		public static JobDef UseVerbOnThingStatic;

		public static JobDef CastJump;

		public static JobDef CastAbilityOnThing;

		public static JobDef CastAbilityOnWorldTile;

		public static JobDef TakeInventory;

		public static JobDef Follow;

		public static JobDef FollowClose;

		public static JobDef Wear;

		public static JobDef RemoveApparel;

		public static JobDef DropEquipment;

		public static JobDef Strip;

		public static JobDef Open;

		public static JobDef Hunt;

		public static JobDef ManTurret;

		public static JobDef EnterCryptosleepCasket;

		public static JobDef UseNeurotrainer;

		public static JobDef UseArtifact;

		public static JobDef TriggerFirefoamPopper;

		public static JobDef ClearSnow;

		public static JobDef Vomit;

		public static JobDef Flick;

		public static JobDef DoBill;

		public static JobDef Research;

		public static JobDef Mine;

		public static JobDef OperateDeepDrill;

		public static JobDef OperateScanner;

		public static JobDef Repair;

		public static JobDef FixBrokenDownBuilding;

		public static JobDef UseCommsConsole;

		public static JobDef Clean;

		public static JobDef TradeWithPawn;

		public static JobDef Flee;

		public static JobDef FleeAndCower;

		public static JobDef Lovin;

		public static JobDef SocialFight;

		public static JobDef Maintain;

		public static JobDef GiveToPackAnimal;

		public static JobDef EnterTransporter;

		public static JobDef Resurrect;

		public static JobDef Insult;

		public static JobDef HaulCorpseToPublicPlace;

		public static JobDef InducePrisonerToEscape;

		public static JobDef OfferHelp;

		public static JobDef ApplyTechprint;

		public static JobDef GotoMindControlled;

		public static JobDef MarryAdjacentPawn;

		public static JobDef SpectateCeremony;

		public static JobDef StandAndBeSociallyActive;

		public static JobDef GiveSpeech;

		public static JobDef PrepareCaravan_GatherItems;

		public static JobDef PrepareCaravan_GatherPawns;

		public static JobDef PrepareCaravan_GatherDownedPawns;

		public static JobDef Ignite;

		public static JobDef BeatFire;

		public static JobDef ExtinguishSelf;

		public static JobDef LayDown;

		public static JobDef Ingest;

		public static JobDef SocialRelax;

		public static JobDef HaulToCell;

		public static JobDef HaulToContainer;

		public static JobDef Steal;

		public static JobDef Reload;

		public static JobDef Refuel;

		public static JobDef RefuelAtomic;

		public static JobDef RearmTurret;

		public static JobDef RearmTurretAtomic;

		public static JobDef FillFermentingBarrel;

		public static JobDef TakeBeerOutOfFermentingBarrel;

		public static JobDef UnloadInventory;

		public static JobDef UnloadYourInventory;

		public static JobDef HaulToTransporter;

		public static JobDef Rescue;

		public static JobDef Arrest;

		public static JobDef Capture;

		public static JobDef TakeWoundedPrisonerToBed;

		public static JobDef TakeToBedToOperate;

		public static JobDef EscortPrisonerToBed;

		public static JobDef CarryToCryptosleepCasket;

		public static JobDef ReleasePrisoner;

		public static JobDef Kidnap;

		public static JobDef CarryDownedPawnToExit;

		public static JobDef PlaceNoCostFrame;

		public static JobDef FinishFrame;

		public static JobDef Deconstruct;

		public static JobDef Uninstall;

		public static JobDef SmoothFloor;

		public static JobDef RemoveFloor;

		public static JobDef BuildRoof;

		public static JobDef RemoveRoof;

		public static JobDef SmoothWall;

		public static JobDef PrisonerAttemptRecruit;

		public static JobDef PrisonerExecution;

		public static JobDef DeliverFood;

		public static JobDef FeedPatient;

		public static JobDef TendPatient;

		public static JobDef VisitSickPawn;

		public static JobDef Sow;

		public static JobDef Harvest;

		public static JobDef CutPlant;

		public static JobDef HarvestDesignated;

		public static JobDef CutPlantDesignated;

		public static JobDef Slaughter;

		public static JobDef Milk;

		public static JobDef Shear;

		public static JobDef Tame;

		public static JobDef Train;

		public static JobDef Nuzzle;

		public static JobDef Mate;

		public static JobDef LayEgg;

		public static JobDef PredatorHunt;

		[MayRequireRoyalty]
		public static JobDef Reign;

		[MayRequireRoyalty]
		public static JobDef Meditate;

		[MayRequireRoyalty]
		public static JobDef Play_MusicalInstrument;

		[MayRequireRoyalty]
		public static JobDef LinkPsylinkable;

		[MayRequireRoyalty]
		public static JobDef BestowingCeremony;

		static JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
	}
}
