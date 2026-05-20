using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PruneGauranlenTree : WorkGiver_Scanner
{
	public const float MaxConnectionStrengthForAutoPruning = 0.99f;

	public override bool Prioritized => true;

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		if (!t.HasThing)
		{
			return 0f;
		}
		CompTreeConnection compTreeConnection = t.Thing.TryGetComp<CompTreeConnection>();
		if (compTreeConnection == null)
		{
			return 0f;
		}
		return compTreeConnection.DesiredConnectionStrength - compTreeConnection.ConnectionStrength;
	}

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
		if (compTreeConnection == null)
		{
			return false;
		}
		if (compTreeConnection.ConnectedPawn != pawn)
		{
			return false;
		}
		if (!compTreeConnection.ShouldBePrunedNow(forced))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.PruneGauranlenTree, t);
		job.playerForced = forced;
		return job;
	}
}
