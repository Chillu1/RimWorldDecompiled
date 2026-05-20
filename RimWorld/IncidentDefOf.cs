namespace RimWorld;

[DefOf]
public static class IncidentDefOf
{
	public static IncidentDef RaidEnemy;

	public static IncidentDef RaidFriendly;

	public static IncidentDef VisitorGroup;

	public static IncidentDef TravelerGroup;

	public static IncidentDef TraderCaravanArrival;

	public static IncidentDef Eclipse;

	public static IncidentDef ToxicFallout;

	public static IncidentDef SolarFlare;

	public static IncidentDef ManhunterPack;

	public static IncidentDef ShipChunkDrop;

	public static IncidentDef OrbitalTraderArrival;

	public static IncidentDef WandererJoin;

	public static IncidentDef GameEndedWanderersJoin;

	public static IncidentDef Infestation;

	public static IncidentDef GiveQuest_Random;

	public static IncidentDef FarmAnimalsWanderIn;

	public static IncidentDef HerdMigration;

	[MayRequireRoyalty]
	public static IncidentDef MechCluster;

	[MayRequireIdeology]
	public static IncidentDef WanderersSkylantern;

	[MayRequireIdeology]
	public static IncidentDef GauranlenPodSpawn;

	[MayRequireIdeology]
	public static IncidentDef Infestation_Jelly;

	[MayRequireBiotech]
	public static IncidentDef NoxiousHaze;

	[MayRequireAnomaly]
	public static IncidentDef HarbingerTreeSpawn;

	[MayRequireAnomaly]
	public static IncidentDef FrenziedAnimals;

	[MayRequireAnomaly]
	public static IncidentDef PitGate;

	[MayRequireAnomaly]
	public static IncidentDef ShamblerSwarm;

	[MayRequireAnomaly]
	public static IncidentDef PsychicRitualSiege;

	[MayRequireAnomaly]
	public static IncidentDef VoidCuriosity;

	[MayRequireOdyssey]
	public static IncidentDef Drought;

	[MayRequireOdyssey]
	public static IncidentDef LavaFlow;

	static IncidentDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
	}
}
