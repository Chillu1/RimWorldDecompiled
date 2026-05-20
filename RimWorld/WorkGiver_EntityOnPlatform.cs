using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_EntityOnPlatform : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override string PostProcessedGerund(Job job)
	{
		Pawn entity = GetEntity(job.targetA.Thing);
		return "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), entity.LabelShort.Named("TARGETLABEL"));
	}

	protected abstract Pawn GetEntity(Thing potentialPlatform);
}
