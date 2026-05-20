using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_ExecuteSlave : WorkGiver_Warden
{
	private static string IncapableOfViolenceLowerTrans;

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public static void ResetStaticData()
	{
		IncapableOfViolenceLowerTrans = "IncapableOfViolenceLower".Translate();
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ShouldTakeCareOfSlave(pawn, t, forced))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.guest.slaveInteractionMode != SlaveInteractionModeDefOf.Execute || !pawn.CanReserve(t, 1, -1, null, forced))
		{
			return null;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			JobFailReason.Is(IncapableOfViolenceLowerTrans);
			return null;
		}
		if (!IsExecutionIdeoAllowed(pawn, pawn2))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.SlaveExecution, t);
	}
}
