using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_EnterBuilding : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (pawn.IsPrisonerOfColony)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
		{
			return false;
		}
		if (!(t is Building_Enterable building_Enterable))
		{
			return false;
		}
		if (building_Enterable.SelectedPawn != pawn)
		{
			return false;
		}
		AcceptanceReport acceptanceReport = building_Enterable.CanAcceptPawn(pawn);
		if (!acceptanceReport.Accepted)
		{
			if (!acceptanceReport.Reason.NullOrEmpty())
			{
				JobFailReason.Is(acceptanceReport.Reason.CapitalizeFirst());
			}
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_Enterable))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.EnterBuilding, t);
	}
}
