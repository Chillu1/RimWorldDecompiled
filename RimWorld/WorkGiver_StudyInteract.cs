using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_StudyInteract : WorkGiver_StudyBase
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (t == pawn)
		{
			return false;
		}
		CompStudiable compStudiable = t.TryGetComp<CompStudiable>();
		if (compStudiable == null || compStudiable.KnowledgeCategory != null)
		{
			return false;
		}
		if (!compStudiable.EverStudiable())
		{
			JobFailReason.IsSilent();
			return false;
		}
		if (!compStudiable.CurrentlyStudiable())
		{
			if (compStudiable.Props.frequencyTicks > 0 && compStudiable.TicksTilNextStudy > 0)
			{
				JobFailReason.Is("CanBeStudiedInDuration".Translate(compStudiable.TicksTilNextStudy.ToStringTicksToPeriod()).CapitalizeFirst());
			}
			else
			{
				JobFailReason.IsSilent();
			}
			return false;
		}
		if (!SocialInteractionUtility.TryGetAdjacentInteractionCell(pawn, t, forced, out var _))
		{
			JobFailReason.Is("CannotStandNear".Translate());
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.StudyInteract, t);
	}
}
