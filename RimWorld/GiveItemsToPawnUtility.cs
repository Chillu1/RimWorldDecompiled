using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class GiveItemsToPawnUtility
{
	public static Thing FindItemToGive(Pawn hauler, ThingDef thingDef)
	{
		return GenClosest.ClosestThingReachable(hauler.Position, hauler.Map, ThingRequest.ForDef(thingDef), PathEndMode.Touch, TraverseParms.For(hauler), 9999f, (Thing x) => !x.IsForbidden(hauler) && hauler.CanReserve(x));
	}

	public static bool IsWaitingForItems(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			return lord.CurLordToil is IWaitForItemsLordToil;
		}
		return false;
	}

	public static int ItemCountWaitingFor(Pawn pawn)
	{
		if (!IsWaitingForItems(pawn))
		{
			return -1;
		}
		return ((IWaitForItemsLordToil)pawn.GetLord().CurLordToil).CountRemaining;
	}

	public static int ItemCountBeingHauled(Pawn requester)
	{
		IReadOnlyList<Pawn> allPawnsSpawned = requester.Map.mapPawns.AllPawnsSpawned;
		Lord lord = requester.GetLord();
		int num = 0;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn = allPawnsSpawned[i];
			if (pawn.CurJob != null && pawn.jobs.curDriver is JobDriver_GiveToPawn jobDriver_GiveToPawn && jobDriver_GiveToPawn.job.lord == lord)
			{
				num += jobDriver_GiveToPawn.CountBeingHauled;
			}
		}
		return num;
	}

	public static int ItemCountLeftToCollect(Pawn requester)
	{
		return ItemCountWaitingFor(requester) - ItemCountBeingHauled(requester);
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptionsForPawn(Pawn requester, Pawn current, ThingDef requestedThingDef, int countRequired)
	{
		Thing reachableThing = FindItemToGive(current, requestedThingDef);
		string text = $"{GetCountRemaining(requester, requestedThingDef, countRequired)}x {requestedThingDef.label}";
		if (reachableThing != null)
		{
			TaggedString taggedString = "GiveItemsTo".Translate(text, requester);
			yield return new FloatMenuOption(taggedString, delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.GiveToPawn, reachableThing, requester);
				job.haulMode = HaulMode.ToContainer;
				job.lord = requester.GetLord();
				current.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High, null, requester);
		}
		else
		{
			TaggedString taggedString2 = "CannotGiveItemsTo".Translate(text, requester) + ": " + "NoItemFound".Translate();
			yield return new FloatMenuOption(taggedString2, null);
		}
	}

	public static int GetCountRemaining(Pawn pawn, ThingDef requestedThingDef, int requestedThingCount)
	{
		int num = 0;
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (innerContainer[i].def == requestedThingDef)
			{
				num += innerContainer[i].stackCount;
			}
		}
		return requestedThingCount - num;
	}
}
