using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ExtractSkull : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public static bool CanExtractSkull(Ideo ideo)
	{
		if (!ideo.classicMode && !ideo.HasPrecept(PreceptDefOf.Skullspike_Desired))
		{
			if (ModsConfig.AnomalyActive)
			{
				return ResearchProjectDefOf.AdvancedPsychicRituals.IsFinished;
			}
			return false;
		}
		return true;
	}

	public static bool CanPlayerExtractSkull()
	{
		if (Find.IdeoManager.classicMode)
		{
			return true;
		}
		if (CanExtractSkull(Faction.OfPlayer.ideos.PrimaryIdeo))
		{
			return true;
		}
		foreach (Ideo item in Faction.OfPlayer.ideos.IdeosMinorListForReading)
		{
			if (CanExtractSkull(item))
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.ExtractSkull))
		{
			yield return item.target.Thing;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.ExtractSkull);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Corpse { Destroyed: false } corpse))
		{
			return false;
		}
		if (corpse.Map.designationManager.DesignationOn(t, DesignationDefOf.ExtractSkull) == null)
		{
			return false;
		}
		if (!corpse.InnerPawn.health.hediffSet.HasHead)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (ModsConfig.IdeologyActive && (pawn.Ideo == null || !CanExtractSkull(pawn.Ideo)))
		{
			JobFailReason.Is("CannotExtractSkull".Translate());
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.ExtractSkull, t);
		job.count = 1;
		return job;
	}
}
