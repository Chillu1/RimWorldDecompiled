using Verse.AI;

namespace RimWorld;

[DefOf]
public static class DutyDefOf
{
	public static DutyDef TravelOrLeave;

	public static DutyDef TravelOrWait;

	public static DutyDef Kidnap;

	public static DutyDef Steal;

	public static DutyDef TakeWoundedGuest;

	public static DutyDef Follow;

	public static DutyDef PrisonerEscape;

	public static DutyDef PrisonerEscapeSapper;

	public static DutyDef DefendAndExpandHive;

	public static DutyDef DefendHiveAggressively;

	public static DutyDef LoadAndEnterTransporters;

	public static DutyDef EnterTransporterAndDefendSelf;

	public static DutyDef LoadAndEnterPortal;

	public static DutyDef ManClosestTurret;

	public static DutyDef SleepForever;

	public static DutyDef Idle;

	public static DutyDef IdleNoInteraction;

	public static DutyDef WanderClose;

	public static DutyDef WanderClose_NoNeeds;

	[MayRequireAnomaly]
	public static DutyDef ShamblerSwarm;

	[MayRequireAnomaly]
	public static DutyDef SightstealerSwarm;

	[MayRequireAnomaly]
	public static DutyDef SightstealerAssault;

	[MayRequireAnomaly]
	public static DutyDef GorehulkAssault;

	[MayRequireAnomaly]
	public static DutyDef DevourerAssault;

	[MayRequireAnomaly]
	public static DutyDef FleshbeastAssault;

	[MayRequireAnomaly]
	public static DutyDef PerformHateChant;

	[MayRequireAnomaly]
	public static DutyDef ChimeraStalkFlee;

	[MayRequireAnomaly]
	public static DutyDef ChimeraStalkWander;

	[MayRequireAnomaly]
	public static DutyDef ChimeraAttack;

	[MayRequireAnomaly]
	public static DutyDef DefendFleshmassHeart;

	[MayRequireAnomaly]
	public static DutyDef VoidAwakeningWander;

	[MayRequireOdyssey]
	public static DutyDef WanderNest;

	[MayRequireOdyssey]
	public static DutyDef NestAssault;

	public static DutyDef AssaultColony;

	public static DutyDef Breaching;

	public static DutyDef Sapper;

	public static DutyDef Escort;

	public static DutyDef Defend;

	[MayRequireAnomaly]
	public static DutyDef DefendInvoker;

	public static DutyDef Build;

	public static DutyDef HuntEnemiesIndividual;

	public static DutyDef DefendBase;

	[MayRequireRoyalty]
	public static DutyDef AssaultThing;

	public static DutyDef PrisonerAssaultColony;

	public static DutyDef HuntDownColonists;

	public static DutyDef ExitMapRandom;

	public static DutyDef ExitMapBest;

	public static DutyDef ExitMapBestAndDefendSelf;

	public static DutyDef ExitMapNearDutyTarget;

	public static DutyDef MarryPawn;

	public static DutyDef GiveSpeech;

	public static DutyDef Spectate;

	public static DutyDef BestowingCeremony_MoveInPlace;

	[MayRequireRoyalty]
	public static DutyDef Bestow;

	[MayRequireIdeology]
	public static DutyDef Pilgrims_Spectate;

	[MayRequireIdeology]
	public static DutyDef PlayTargetInstrument;

	[MayRequireBiotech]
	public static DutyDef SocialMeeting;

	public static DutyDef PrepareCaravan_GatherItems;

	public static DutyDef PrepareCaravan_Wait;

	public static DutyDef PrepareCaravan_GatherAnimals;

	public static DutyDef PrepareCaravan_CollectAnimals;

	public static DutyDef PrepareCaravan_GatherDownedPawns;

	public static DutyDef PrepareCaravan_Pause;

	public static DutyDef ReturnedCaravan_PenAnimals;

	[MayRequireAnomaly]
	public static DutyDef Goto;

	[MayRequireAnomaly]
	public static DutyDef Goto_NoZeroLengthPaths;

	[MayRequireAnomaly]
	public static DutyDef Invoke;

	[MayRequireAnomaly]
	public static DutyDef PsychicRitualDance;

	[MayRequireAnomaly]
	public static DutyDef WaitForRitualParticipants;

	[MayRequireAnomaly]
	public static DutyDef DeliverPawnToPsychicRitualCell;

	[MayRequireAnomaly]
	public static DutyDef GatherOfferingsForPsychicRitual;

	static DutyDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(DutyDefOf));
	}
}
