namespace RimWorld;

[DefOf]
public static class ConceptDefOf
{
	public static ConceptDef CameraDolly;

	public static ConceptDef CameraZoom;

	public static ConceptDef TimeControls;

	public static ConceptDef Pause;

	public static ConceptDef Mining;

	public static ConceptDef Forbidding;

	public static ConceptDef ForbiddingDoors;

	public static ConceptDef EquippingWeapons;

	public static ConceptDef Stockpiles;

	public static ConceptDef GrowingFood;

	public static ConceptDef InfoCard;

	public static ConceptDef AnimalTaming;

	public static ConceptDef AnimalTraining;

	public static ConceptDef TileInspector;

	public static ConceptDef TimeAssignments;

	public static ConceptDef Outfits;

	public static ConceptDef DrugPolicies;

	public static ConceptDef ClickingMessages;

	public static ConceptDef HostilityResponse;

	public static ConceptDef WorkTab;

	public static ConceptDef StorageTab;

	public static ConceptDef HistoryTab;

	public static ConceptDef BillsTab;

	public static ConceptDef Alerts;

	public static ConceptDef AllowedAreas;

	public static ConceptDef ManualWorkPriorities;

	public static ConceptDef SpoilageAndFreezers;

	public static ConceptDef FormCaravan;

	public static ConceptDef ReformCaravan;

	public static ConceptDef Capturing;

	public static ConceptDef Rescuing;

	public static ConceptDef Drafting;

	public static ConceptDef GroupGotoHereDragging;

	public static ConceptDef HomeArea;

	public static ConceptDef PrisonerTab;

	public static ConceptDef OpeningComms;

	public static ConceptDef BuildOrbitalTradeBeacon;

	public static ConceptDef MedicalOperations;

	public static ConceptDef WorldCameraMovement;

	public static ConceptDef SetGrowingZonePlant;

	public static ConceptDef AnimalsDontAttackDoors;

	public static ConceptDef InteractingWithTraders;

	public static ConceptDef DrugAddiction;

	public static ConceptDef ShieldBelts;

	public static ConceptDef DrugBurning;

	public static ConceptDef DoorOpenSpeed;

	public static ConceptDef QueueOrders;

	public static ConceptDef Shelves;

	public static ConceptDef Books;

	[MayRequireRoyalty]
	public static ConceptDef MeditationSchedule;

	[MayRequireRoyalty]
	public static ConceptDef MeditationDesiredPsyfocus;

	[MayRequireIdeology]
	public static ConceptDef EditingMemes;

	[MayRequireIdeology]
	public static ConceptDef EditingPrecepts;

	[MayRequireBiotech]
	public static ConceptDef PollutedTerrain;

	[MayRequireBiotech]
	public static ConceptDef Babies;

	[MayRequireBiotech]
	public static ConceptDef Children;

	[MayRequireBiotech]
	public static ConceptDef Deathrest;

	[MayRequireBiotech]
	public static ConceptDef GenesAndXenotypes;

	[MayRequireBiotech]
	public static ConceptDef Mechanitors;

	[MayRequireBiotech]
	public static ConceptDef MechsInCaravans;

	[MayRequireAnomaly]
	public static ConceptDef CapturingEntities;

	[MayRequireAnomaly]
	public static ConceptDef ContainingEntities;

	[MayRequireAnomaly]
	public static ConceptDef StudyingEntities;

	[MayRequireAnomaly]
	public static ConceptDef EntityCodex;

	[MayRequireAnomaly]
	public static ConceptDef SuppressingEntities;

	[MayRequireAnomaly]
	public static ConceptDef AnomalyResearch;

	[MayRequireAnomaly]
	public static ConceptDef ColonyGhouls;

	[MayRequireOdyssey]
	public static ConceptDef Gravship;

	[MayRequireOdyssey]
	public static ConceptDef Orbit;

	public static ConceptDef ArrestingCreatesEnemies;

	public static ConceptDef TradeGoodsMustBeNearBeacon;

	public static ConceptDef SwitchFlickingDesignation;

	public static ConceptDef MaxNumberOfPlayerSettlements;

	public static ConceptDef TradingRequiresPermit;

	public static ConceptDef SteamDeckControlsMainMenu;

	public static ConceptDef SteamDeckControlsGame;

	static ConceptDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ConceptDefOf));
	}
}
