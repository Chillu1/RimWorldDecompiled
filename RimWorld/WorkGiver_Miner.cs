using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Miner : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return MineAIUtility.PotentialMineables(pawn);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Mine))
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.MineVein);
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return MineAIUtility.JobOnThing(pawn, t, forced);
	}
}
