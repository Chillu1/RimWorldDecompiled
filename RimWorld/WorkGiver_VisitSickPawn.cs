using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_VisitSickPawn : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (!InteractionUtility.CanInitiateInteraction(pawn))
			{
				return true;
			}
			List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].InBed())
				{
					return false;
				}
			}
			return true;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null)
			{
				return false;
			}
			return SickPawnVisitUtility.CanVisit(pawn, pawn2, JoyCategory.VeryLow);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			Job job = JobMaker.MakeJob(JobDefOf.VisitSickPawn, pawn2, SickPawnVisitUtility.FindChair(pawn, pawn2));
			job.ignoreJoyTimeAssignment = true;
			return job;
		}
	}
}
