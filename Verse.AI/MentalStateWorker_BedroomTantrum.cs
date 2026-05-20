using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalStateWorker_BedroomTantrum : MentalStateWorker
{
	private static List<Thing> tmpThings = new List<Thing>();

	public override bool StateCanOccur(Pawn pawn)
	{
		if (!base.StateCanOccur(pawn))
		{
			return false;
		}
		Building_Bed ownedBed = pawn.ownership.OwnedBed;
		if (ownedBed == null || ownedBed.GetRoom() == null || ownedBed.GetRoom().PsychologicallyOutdoors)
		{
			return false;
		}
		tmpThings.Clear();
		TantrumMentalStateUtility.GetSmashableThingsIn(ownedBed.GetRoom(), pawn, tmpThings);
		bool result = tmpThings.Any();
		tmpThings.Clear();
		return result;
	}
}
