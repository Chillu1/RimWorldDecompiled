using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class GenGuest
	{
		public static void PrisonerRelease(Pawn p)
		{
			if (p.ownership != null)
			{
				p.ownership.UnclaimAll();
			}
			if (p.Faction == Faction.OfPlayer || p.IsWildMan())
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasImprisoned);
				}
				p.guest.SetGuestStatus(null);
				if (p.IsWildMan())
				{
					p.mindState.WildManEverReachedOutside = false;
				}
			}
			else
			{
				p.guest.Released = true;
				if (RCellFinder.TryFindBestExitSpot(p, out IntVec3 spot))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
					job.exitMapOnArrival = true;
					p.jobs.StartJob(job);
				}
			}
		}

		public static void AddPrisonerSoldThoughts(Pawn prisoner)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood != null)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowPrisonerSold);
				}
			}
		}

		public static void AddHealthyPrisonerReleasedThoughts(Pawn prisoner)
		{
			if (!prisoner.IsColonist)
			{
				foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood != null && allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner != prisoner)
					{
						allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
					}
				}
			}
		}

		public static void RemoveHealthyPrisonerReleasedThoughts(Pawn prisoner)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist.needs.mood != null && allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist != prisoner)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
				}
			}
		}
	}
}
