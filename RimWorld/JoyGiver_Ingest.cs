using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_Ingest : JoyGiver
{
	private static readonly List<Thing> tmpCandidates = new List<Thing>();

	public override Job TryGiveJob(Pawn pawn)
	{
		return TryGiveJobInternal(pawn, null);
	}

	public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot, float maxRadius = -1f)
	{
		return TryGiveJobInternal(pawn, (Thing x) => !x.Spawned || (GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map) && (maxRadius < 0f || x.Position.InHorDistOf(gatheringSpot, maxRadius))));
	}

	private Job TryGiveJobInternal(Pawn pawn, Predicate<Thing> extraValidator)
	{
		Thing thing = BestIngestItem(pawn, extraValidator);
		if (thing == null)
		{
			return null;
		}
		return CreateIngestJob(thing, pawn);
	}

	protected virtual Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
	{
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (SearchSetWouldInclude(innerContainer[i]) && Validator(innerContainer[i]))
			{
				return innerContainer[i];
			}
		}
		tmpCandidates.Clear();
		GetSearchSet(pawn, tmpCandidates);
		if (tmpCandidates.Count == 0)
		{
			return null;
		}
		Thing result = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, tmpCandidates, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, Validator);
		tmpCandidates.Clear();
		return result;
		bool Validator(Thing t)
		{
			if (!CanIngestForJoy(pawn, t))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(t))
			{
				return false;
			}
			return true;
		}
	}

	protected virtual bool CanIngestForJoy(Pawn pawn, Thing t)
	{
		if (!t.def.IsIngestible || t.def.ingestible.joyKind == null || t.def.ingestible.joy <= 0f || !pawn.WillEat(t))
		{
			return false;
		}
		if (t.Spawned)
		{
			if (t.Fogged())
			{
				return false;
			}
			if (!pawn.CanReserve(t))
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!t.IsSociallyProper(pawn))
			{
				return false;
			}
			if (!t.IsPoliticallyProper(pawn))
			{
				return false;
			}
			if (t.Position.VacuumConcernTo(pawn))
			{
				return false;
			}
		}
		if (t.def.IsDrug && pawn.drugs != null && !pawn.drugs.CurrentPolicy[t.def].allowedForJoy && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0 && !pawn.InMentalState)
		{
			return false;
		}
		return true;
	}

	protected virtual bool SearchSetWouldInclude(Thing thing)
	{
		if (def.thingDefs == null)
		{
			return false;
		}
		return def.thingDefs.Contains(thing.def);
	}

	protected virtual Job CreateIngestJob(Thing ingestible, Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Ingest, ingestible);
		job.count = ingestible.stackCount;
		if (ingestible.def.ingestible.maxNumToIngestAtOnce > 0)
		{
			job.count = Math.Min(job.count, ingestible.def.ingestible.maxNumToIngestAtOnce);
		}
		return job;
	}
}
