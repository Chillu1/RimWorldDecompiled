using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_WanderersSkylanterns : IncidentWorker_VisitorGroup
	{
		protected override LordJob_VisitColony CreateLordJob(IncidentParms parms, List<Pawn> pawns)
		{
			LordJob_VisitColony lordJob_VisitColony = base.CreateLordJob(parms, pawns);
			lordJob_VisitColony.gifts = parms.gifts;
			parms.gifts = null;
			return lordJob_VisitColony;
		}

		protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, Pawn leader, bool traderExists)
		{
			SendStandardLetter(parms, pawns);
		}
	}
}
