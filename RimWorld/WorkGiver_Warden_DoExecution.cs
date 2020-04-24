using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DoExecution : WorkGiver_Warden
	{
		private static string IncapableOfViolenceLowerTrans;

		public override PathEndMode PathEndMode => PathEndMode.OnCell;

		public static void ResetStaticData()
		{
			IncapableOfViolenceLowerTrans = "IncapableOfViolenceLower".Translate();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			if (((Pawn)t).guest.interactionMode != PrisonerInteractionModeDefOf.Execution || !pawn.CanReserve(t))
			{
				return null;
			}
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				JobFailReason.Is(IncapableOfViolenceLowerTrans);
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.PrisonerExecution, t);
		}
	}
}
