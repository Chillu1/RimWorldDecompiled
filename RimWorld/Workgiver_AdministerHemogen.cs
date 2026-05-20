using Verse;
using Verse.AI;

namespace RimWorld;

public class Workgiver_AdministerHemogen : WorkGiver_Scanner
{
	private const float MinLevelForFeedingHemogenUnforced = 0.25f;

	private const float HemogenPctMax = 0.95f;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || pawn2 == pawn)
		{
			return false;
		}
		Gene_Hemogen gene_Hemogen = pawn2.genes?.GetFirstGeneOfType<Gene_Hemogen>();
		if (gene_Hemogen == null)
		{
			return false;
		}
		if (gene_Hemogen.ValuePercent >= 0.95f)
		{
			return false;
		}
		if (!forced && gene_Hemogen.Value >= 0.25f)
		{
			return false;
		}
		if (!FeedPatientUtility.ShouldBeFed(pawn2))
		{
			return false;
		}
		if (!gene_Hemogen.hemogenPacksAllowed)
		{
			return false;
		}
		if (!gene_Hemogen.ShouldConsumeHemogenNow())
		{
			JobFailReason.Is("NotAllowedHemogen".Translate());
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.HemogenPack), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack)) == null)
		{
			JobFailReason.Is("NoIngredient".Translate(ThingDefOf.HemogenPack));
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = (Pawn)t;
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.HemogenPack), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing pack) => !pack.IsForbidden(pawn) && pawn.CanReserve(pack));
		if (thing != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, thing, pawn2);
			job.count = 1;
			return job;
		}
		return null;
	}
}
