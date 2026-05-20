using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ChangeTreeMode : WorkGiver_Scanner
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.DryadSpawner));
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			CompTreeConnection compTreeConnection = t.TryGetComp<CompTreeConnection>();
			if (compTreeConnection == null || compTreeConnection.ConnectedPawn != pawn || compTreeConnection.Mode == compTreeConnection.desiredMode)
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(JobDefOf.ChangeTreeMode, t);
			job.playerForced = forced;
			return job;
		}
	}
}
