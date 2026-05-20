using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class WorkGiver_TendOther_Humanlike : WorkGiver_TendOther
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedHumanlikesWithAnyHediff;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t is Pawn pawn2 && !pawn2.RaceProps.Humanlike)
		{
			return false;
		}
		return base.HasJobOnThing(pawn, t, forced);
	}
}
