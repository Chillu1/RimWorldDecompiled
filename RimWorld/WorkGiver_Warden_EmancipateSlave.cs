using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_EmancipateSlave : WorkGiver_Warden
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckIdeology("Slave imprisonment"))
		{
			return null;
		}
		if (!(t is Pawn pawn2))
		{
			return null;
		}
		if (!ShouldTakeCareOfSlave(pawn, pawn2))
		{
			return null;
		}
		if (!pawn.MapHeld.CanEverExit)
		{
			JobFailReason.Is("CannotExitMap".Translate());
			return null;
		}
		if (pawn2.guest.slaveInteractionMode != SlaveInteractionModeDefOf.Emancipate || pawn2.Downed || !pawn2.Awake())
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.SlaveEmancipation, pawn2);
		job.count = 1;
		return job;
	}
}
