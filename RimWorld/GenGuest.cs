using Verse;
using Verse.AI;

namespace RimWorld;

public static class GenGuest
{
	public static void PrisonerRelease(Pawn p)
	{
		if ((p.Faction == Faction.OfPlayer || p.IsWildMan()) && p.needs.mood != null)
		{
			p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasImprisoned);
		}
		if (p.SlaveFaction != null)
		{
			Faction hostFaction = p.HostFaction;
			p.SetFaction(p.SlaveFaction);
			p.guest.SetGuestStatus(hostFaction, GuestStatus.Prisoner);
		}
		GuestRelease(p);
	}

	public static void SlaveRelease(Pawn p)
	{
		if ((p.Faction == Faction.OfPlayer || p.IsWildMan()) && p.needs.mood != null)
		{
			p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasEnslaved);
		}
		GuestRelease(p);
	}

	private static bool ShouldStayOnMapOnRelease(Pawn pawn)
	{
		if (pawn.IsWildMan())
		{
			return true;
		}
		if (pawn.HomeFaction != null)
		{
			return pawn.HomeFaction.IsPlayer;
		}
		return false;
	}

	private static void GuestRelease(Pawn p)
	{
		p.ownership?.UnclaimAll();
		if (p.Drafted)
		{
			p.drafter.Drafted = false;
		}
		p.guest.Released = true;
		p.guest.SetNoInteraction();
		IntVec3 spot;
		if (ShouldStayOnMapOnRelease(p))
		{
			int interactionsToday = p.mindState.interactionsToday;
			int lastAssignedInteractTime = p.mindState.lastAssignedInteractTime;
			if (p.HomeFaction != null)
			{
				p.guest.SetGuestStatus(null);
			}
			p.mindState.interactionsToday = interactionsToday;
			p.mindState.lastAssignedInteractTime = lastAssignedInteractTime;
			if (p.IsWildMan())
			{
				p.mindState.WildManEverReachedOutside = false;
			}
			if (ModsConfig.IdeologyActive && p.Ideo != null && p.IsColonist)
			{
				p.Ideo.RecacheColonistBelieverCount();
			}
		}
		else if (RCellFinder.TryFindBestExitSpot(p, out spot))
		{
			Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
			job.exitMapOnArrival = true;
			p.jobs.StartJob(job, JobCondition.InterruptForced);
		}
		p.Notify_Released();
	}

	public static void AddHealthyPrisonerReleasedThoughts(Pawn prisoner)
	{
		if (prisoner.IsColonist)
		{
			return;
		}
		foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners)
		{
			if (allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoner.needs.mood != null && allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoner != prisoner)
			{
				allMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
			}
		}
	}

	public static void RemoveHealthyPrisonerReleasedThoughts(Pawn prisoner)
	{
		foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_FreeColonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
		{
			if (allMapsCaravansAndTravellingTransporters_Alive_FreeColonist.needs.mood != null && allMapsCaravansAndTravellingTransporters_Alive_FreeColonist != prisoner)
			{
				allMapsCaravansAndTravellingTransporters_Alive_FreeColonist.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
			}
		}
	}

	public static void EmancipateSlave(Pawn warden, Pawn slave)
	{
		if (slave.IsSlave)
		{
			SlaveRelease(slave);
			if (slave.IsWildMan())
			{
				slave.mindState.WildManEverReachedOutside = false;
			}
			Messages.Message("MessageSlaveEmancipated".Translate(slave, warden), new LookTargets(slave, warden), MessageTypeDefOf.NeutralEvent);
		}
	}

	public static void EnslavePrisoner(Pawn warden, Pawn prisoner)
	{
		TryEnslavePrisoner(warden, prisoner);
	}

	public static bool TryEnslavePrisoner(Pawn warden, Pawn prisoner)
	{
		if (prisoner.IsSlave)
		{
			return false;
		}
		if (prisoner.IsCreepJoiner && prisoner.creepjoiner.CanTriggerAggressive)
		{
			prisoner.creepjoiner.DoAggressive();
			return false;
		}
		if (prisoner.Faction != null && prisoner.Faction.def.hidden)
		{
			prisoner.SetFactionDirect(null);
		}
		bool everEnslaved = prisoner.guest.EverEnslaved;
		prisoner.guest.SetGuestStatus(warden.Faction, GuestStatus.Slave);
		Messages.Message("MessagePrisonerEnslaved".Translate(prisoner, warden), new LookTargets(prisoner, warden), MessageTypeDefOf.NeutralEvent);
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, warden.Named(HistoryEventArgsNames.Doer)));
		if (!everEnslaved)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.EnslavedPrisonerNotPreviouslyEnslaved, warden.Named(HistoryEventArgsNames.Doer)));
		}
		prisoner.apparel.UnlockAll();
		return true;
	}
}
