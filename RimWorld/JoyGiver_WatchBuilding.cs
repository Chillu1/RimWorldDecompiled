using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_WatchBuilding : JoyGiver_InteractBuilding
{
	protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
	{
		if (!base.CanInteractWith(pawn, t, inBed))
		{
			return false;
		}
		if (inBed)
		{
			Building_Bed bed = pawn.CurrentBed();
			return WatchBuildingUtility.CanWatchFromBed(pawn, bed, t);
		}
		return true;
	}

	protected override Job TryGivePlayJob(Pawn pawn, Thing t)
	{
		if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, def.desireSit, out var result, out var chair))
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, t, result, chair);
	}
}
