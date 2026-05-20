using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_BreastfeedCarryToMom : WorkGiver_FeedBabyManually
	{
		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			List<Pawn> freeColonistsAndPrisonersSpawned = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < freeColonistsAndPrisonersSpawned.Count; i++)
			{
				if ((freeColonistsAndPrisonersSpawned[i].Downed || freeColonistsAndPrisonersSpawned[i].IsPrisoner) && ChildcareUtility.CanBreastfeedNow(freeColonistsAndPrisonersSpawned[i], out var reason))
				{
					flag2 = true;
				}
				if (ChildcareUtility.CanSuckle(freeColonistsAndPrisonersSpawned[i], out reason))
				{
					flag = true;
				}
				if (flag2 && flag)
				{
					return false;
				}
				if (flag && forced)
				{
					return false;
				}
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!CanCreateManualFeedingJob(pawn, t, forced))
			{
				return null;
			}
			Pawn baby = (Pawn)t;
			if (!ChildcareUtility.ImmobileBreastfeederAvailable(pawn, baby, forced, out var feeder, out var reason))
			{
				if (forced && feeder != pawn)
				{
					JobFailReason.Is(reason.Value.Translate(pawn, feeder, baby));
				}
				return null;
			}
			return ChildcareUtility.MakeBreastfeedCarryToMomJob(baby, feeder);
		}
	}
}
