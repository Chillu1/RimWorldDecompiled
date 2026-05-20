using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class WorkGiver_TendOther_Animal : WorkGiver_TendOther
{
	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedAnimalsWithAnyHediff;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t is Pawn { IsAnimal: false })
		{
			return false;
		}
		return base.HasJobOnThing(pawn, t, forced);
	}
}
