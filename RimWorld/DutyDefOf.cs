using Verse.AI;

namespace RimWorld
{
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

		public static DutyDef ManClosestTurret;

		public static DutyDef SleepForever;

		public static DutyDef AssaultColony;

		public static DutyDef Sapper;

		public static DutyDef Escort;

		public static DutyDef Defend;

		public static DutyDef Build;

		public static DutyDef HuntEnemiesIndividual;

		public static DutyDef DefendBase;

		public static DutyDef ExitMapRandom;

		public static DutyDef ExitMapBest;

		public static DutyDef ExitMapBestAndDefendSelf;

		public static DutyDef ExitMapNearDutyTarget;

		public static DutyDef MarryPawn;

		public static DutyDef GiveSpeech;

		public static DutyDef Spectate;

		public static DutyDef Party;

		public static DutyDef PrepareCaravan_GatherItems;

		public static DutyDef PrepareCaravan_Wait;

		public static DutyDef PrepareCaravan_GatherPawns;

		public static DutyDef PrepareCaravan_GatherDownedPawns;

		public static DutyDef PrepareCaravan_Pause;

		static DutyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DutyDefOf));
		}
	}
}
