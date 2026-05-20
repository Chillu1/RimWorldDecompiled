using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulToAtomizer : WorkGiver_Scanner
{
	private const float MaxFillPercent = 0.5f;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Atomizer);

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Haul to atomizer"))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		CompAtomizer compAtomizer = t.TryGetComp<CompAtomizer>();
		if (compAtomizer.Full)
		{
			JobFailReason.Is(HaulAIUtility.ContainerFullLowerTrans);
			return false;
		}
		if (!forced && !compAtomizer.AutoLoad)
		{
			return false;
		}
		if (!forced && compAtomizer.FillPercent > 0.5f)
		{
			return false;
		}
		if (HaulAIUtility.FindFixedIngredientCount(pawn, compAtomizer.Props.thingDef, compAtomizer.SpaceLeft).NullOrEmpty())
		{
			JobFailReason.Is("NoIngredient".Translate(compAtomizer.Props.thingDef));
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompAtomizer compAtomizer = t.TryGetComp<CompAtomizer>();
		if (compAtomizer == null)
		{
			return null;
		}
		if (compAtomizer.Full)
		{
			JobFailReason.Is(HaulAIUtility.ContainerFullLowerTrans);
			return null;
		}
		if (!forced && !compAtomizer.AutoLoad)
		{
			return null;
		}
		List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, compAtomizer.Props.thingDef, compAtomizer.SpaceLeft);
		if (list.NullOrEmpty())
		{
			JobFailReason.Is("NoIngredient".Translate(compAtomizer.Props.thingDef));
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.HaulToAtomizer, t);
		job.targetQueueB = list.Select((Thing f) => new LocalTargetInfo(f)).ToList();
		job.count = compAtomizer.SpaceLeft;
		return job;
	}
}
