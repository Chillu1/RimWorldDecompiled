using Verse;

namespace RimWorld
{
	public abstract class WorkGiver_TakeToBed : WorkGiver_Scanner
	{
		protected Building_Bed FindBed(Pawn pawn, Pawn patient)
		{
			return RestUtility.FindBedFor(patient, pawn, checkSocialProperness: false, ignoreOtherReservations: false, patient.GuestStatus);
		}
	}
}
