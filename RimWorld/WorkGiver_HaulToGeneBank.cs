using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulToGeneBank : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.Genepack);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.BiotechActive;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Genepack"))
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return FindGeneBank(pawn, t) != null;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Thing thing = FindGeneBank(pawn, t);
		if (thing != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.CarryGenepackToContainer, t, thing, thing.InteractionCell);
			job.count = t.stackCount;
			return job;
		}
		return null;
	}

	private Thing FindGeneBank(Pawn pawn, Thing genepackThing)
	{
		Genepack genepack = genepackThing as Genepack;
		if (!genepack.AutoLoad)
		{
			return null;
		}
		if (genepack.targetContainer != null)
		{
			if (genepack.targetContainer.Map == genepack.Map)
			{
				CompGenepackContainer compGenepackContainer = genepack.targetContainer.TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer != null && !compGenepackContainer.Full)
				{
					return genepack.targetContainer;
				}
			}
			return null;
		}
		return GenClosest.ClosestThingReachable(genepack.Position, genepack.Map, ThingRequest.ForGroup(ThingRequestGroup.GenepackHolder), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, delegate(Thing x)
		{
			if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			CompGenepackContainer compGenepackContainer2 = x.TryGetComp<CompGenepackContainer>();
			if (compGenepackContainer2 == null || compGenepackContainer2.Full || !compGenepackContainer2.autoLoad)
			{
				return false;
			}
			Thing targetContainer = genepack.targetContainer;
			return (targetContainer == null || targetContainer == compGenepackContainer2.parent) ? true : false;
		});
	}
}
