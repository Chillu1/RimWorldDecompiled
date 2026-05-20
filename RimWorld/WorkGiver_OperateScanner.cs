using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_OperateScanner : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ScannerDef);

	public ThingDef ScannerDef => def.scannerDef;

	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.Faction != pawn.Faction)
		{
			return false;
		}
		if (!(t is Building building))
		{
			return false;
		}
		if (!pawn.CanReserve(building, 1, -1, null, forced))
		{
			return false;
		}
		AcceptanceReport canUseNow = building.TryGetComp<CompScanner>().CanUseNow;
		if (!canUseNow)
		{
			if (!canUseNow.Reason.NullOrEmpty())
			{
				JobFailReason.Is(canUseNow.Reason);
			}
			return false;
		}
		if (building.IsBurning())
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.OperateScanner, t, 1500, checkOverrideOnExpiry: true);
	}
}
