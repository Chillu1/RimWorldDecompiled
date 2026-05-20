using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ExecuteGuiltyColonist : WorkGiver_Scanner
{
	private static string IncapableOfViolenceLowerTrans;

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public static void ResetStaticData()
	{
		IncapableOfViolenceLowerTrans = "IncapableOfViolenceLower".Translate();
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.FreeColonistsSpawned;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return pawn.Map.mapPawns.FreeColonistsSpawnedCount == 0;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = (Pawn)t;
		if (pawn2?.guilt == null || !pawn2.guilt.IsGuilty || !pawn2.guilt.awaitingExecution || !pawn2.IsColonist || !pawn2.Spawned || pawn2.InAggroMentalState || pawn2.IsForbidden(pawn) || pawn2.IsFormingCaravan() || !pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
		{
			return null;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			JobFailReason.Is(IncapableOfViolenceLowerTrans);
			return null;
		}
		if (!new HistoryEvent(HistoryEventDefOf.ExecutedColonist, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.GuiltyColonistExecution, t);
	}
}
