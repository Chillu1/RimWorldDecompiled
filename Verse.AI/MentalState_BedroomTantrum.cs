using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalState_BedroomTantrum : MentalState_TantrumRandom
{
	protected override void GetPotentialTargets(List<Thing> outThings)
	{
		outThings.Clear();
		Building_Bed ownedBed = pawn.ownership.OwnedBed;
		if (ownedBed != null)
		{
			if (ownedBed.GetRoom() != null && !ownedBed.GetRoom().PsychologicallyOutdoors)
			{
				TantrumMentalStateUtility.GetSmashableThingsIn(ownedBed.GetRoom(), pawn, outThings, GetCustomValidator());
			}
			else
			{
				TantrumMentalStateUtility.GetSmashableThingsNear(pawn, ownedBed.Position, outThings, GetCustomValidator(), 0, 8);
			}
		}
	}
}
