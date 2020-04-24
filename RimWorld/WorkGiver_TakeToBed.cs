using Verse;

namespace RimWorld
{
	public abstract class WorkGiver_TakeToBed : WorkGiver_Scanner
	{
		protected Building_Bed FindBed(Pawn pawn, Pawn patient)
		{
			return RestUtility.FindBedFor(patient, pawn, patient.HostFaction == pawn.Faction, checkSocialProperness: false);
		}
	}
}
