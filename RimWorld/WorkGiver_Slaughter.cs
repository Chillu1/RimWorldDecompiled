using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Slaughter : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Slaughter))
		{
			yield return item.target.Thing;
		}
		foreach (Pawn item2 in pawn.Map.autoSlaughterManager.AnimalsToSlaughter)
		{
			yield return item2;
		}
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Slaughter))
		{
			return pawn.Map.autoSlaughterManager.AnimalsToSlaughter.Count == 0;
		}
		return false;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn { IsAnimal: not false } pawn2))
		{
			return false;
		}
		if (!pawn2.ShouldBeSlaughtered())
		{
			return false;
		}
		if (pawn.Faction != t.Faction)
		{
			return false;
		}
		if (pawn2.InAggroMentalState)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			JobFailReason.Is("IsIncapableOfViolenceShort".Translate(pawn));
			return false;
		}
		if (ModsConfig.IdeologyActive && !new HistoryEvent(HistoryEventDefOf.SlaughteredAnimal, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (HistoryEventUtility.IsKillingInnocentAnimal(pawn, pawn2) && !new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (pawn.Ideo != null && pawn.Ideo.IsVeneratedAnimal(pawn2) && !new HistoryEvent(HistoryEventDefOf.SlaughteredVeneratedAnimal, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.Slaughter, t);
	}
}
