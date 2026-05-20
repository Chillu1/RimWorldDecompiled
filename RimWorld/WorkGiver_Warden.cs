using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_Warden : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override PathEndMode PathEndMode => PathEndMode.OnCell;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SlavesAndPrisonersOfColonySpawned;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return pawn.Map.mapPawns.SlavesAndPrisonersOfColonySpawnedCount == 0;
	}

	protected bool ShouldTakeCareOfPrisoner(Pawn warden, Thing prisoner, bool forced = false)
	{
		if (!(prisoner is Pawn { IsPrisonerOfColony: not false } pawn) || !pawn.guest.PrisonerIsSecure || !pawn.Spawned || pawn.InAggroMentalState || prisoner.IsForbidden(warden) || pawn.IsFormingCaravan() || !warden.CanReserveAndReach(pawn, PathEndMode.OnCell, warden.NormalMaxDanger(), 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	protected bool ShouldTakeCareOfSlave(Pawn warden, Thing slave, bool forced = false)
	{
		if (!(slave is Pawn { IsSlaveOfColony: not false } pawn) || !pawn.guest.SlaveIsSecure || !pawn.Spawned || pawn.InAggroMentalState || pawn.IsForbidden(warden) || pawn.IsFormingCaravan() || !warden.CanReserveAndReach(pawn, PathEndMode.Touch, warden.NormalMaxDanger(), 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	protected bool IsExecutionIdeoAllowed(Pawn warden, Pawn victim)
	{
		if (!new HistoryEvent(HistoryEventDefOf.ExecutedPrisoner, warden.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (victim.guilt.IsGuilty && !new HistoryEvent(HistoryEventDefOf.ExecutedPrisonerGuilty, warden.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		if (!victim.guilt.IsGuilty && !new HistoryEvent(HistoryEventDefOf.ExecutedPrisonerInnocent, warden.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			return false;
		}
		return true;
	}
}
