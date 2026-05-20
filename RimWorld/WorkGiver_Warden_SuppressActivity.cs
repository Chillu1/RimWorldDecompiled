using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_SuppressActivity : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Suppressable);
	}

	public override float GetPriority(Pawn pawn, TargetInfo t)
	{
		Thing thingToSuppress = GetThingToSuppress(t.Thing, playerForced: false);
		if (thingToSuppress == null)
		{
			return 0f;
		}
		return thingToSuppress.TryGetComp<CompActivity>()?.ActivityLevel ?? 0f;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return null;
		}
		Thing thingToSuppress = GetThingToSuppress(t, forced);
		CompActivity compActivity = thingToSuppress.TryGetComp<CompActivity>();
		if (thingToSuppress == null)
		{
			return null;
		}
		if (StatDefOf.ActivitySuppressionRate.Worker.IsDisabledFor(pawn))
		{
			return null;
		}
		if (StatDefOf.ActivitySuppressionRate.Worker.GetValue(pawn) <= 0f)
		{
			JobFailReason.Is("ZeroSuppressionRate".Translate());
			return null;
		}
		if (!ActivitySuppressionUtility.CanBeSuppressed(thingToSuppress, considerMinActivity: true, forced))
		{
			return null;
		}
		if (!forced && compActivity.ActivityLevel < compActivity.suppressIfAbove)
		{
			return null;
		}
		if (!pawn.CanReserve(thingToSuppress, 1, -1, null, forced))
		{
			return null;
		}
		if (thingToSuppress.ParentHolder is Building_HoldingPlatform building_HoldingPlatform && !pawn.CanReserve(building_HoldingPlatform, 1, -1, null, forced))
		{
			return null;
		}
		if (!SocialInteractionUtility.TryGetAdjacentInteractionCell(pawn, t, forced, out var _))
		{
			JobFailReason.Is("CannotStandNear".Translate());
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.ActivitySuppression, t);
		job.playerForced = forced;
		return job;
	}

	private Thing GetThingToSuppress(Thing thing, bool playerForced)
	{
		Thing thing2 = thing;
		if (thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			thing2 = building_HoldingPlatform.HeldPawn;
		}
		if (thing2 == null || !ActivitySuppressionUtility.CanBeSuppressed(thing2, considerMinActivity: true, playerForced))
		{
			return null;
		}
		return thing2;
	}
}
