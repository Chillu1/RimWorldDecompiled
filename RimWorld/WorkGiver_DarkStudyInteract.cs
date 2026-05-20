using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_DarkStudyInteract : WorkGiver_StudyBase
{
	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override string PostProcessedGerund(Job job)
	{
		if (job.targetC == null)
		{
			return base.PostProcessedGerund(job);
		}
		return "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), job.targetC.Label.Named("TARGETLABEL"));
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		Thing thing = t;
		if (ModsConfig.AnomalyActive && t is Building_HoldingPlatform building_HoldingPlatform)
		{
			thing = building_HoldingPlatform.HeldPawn;
			if (thing == null || !pawn.CanReserve(thing, 1, -1, null, forced))
			{
				return false;
			}
		}
		if (thing == null)
		{
			return false;
		}
		if (thing == pawn)
		{
			return false;
		}
		CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
		if (compStudiable.KnowledgeCategory == null)
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
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.StudyInteract, t, null, (t as Building_HoldingPlatform)?.HeldPawn);
	}
}
