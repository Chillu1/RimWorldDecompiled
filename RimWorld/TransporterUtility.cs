using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class TransporterUtility
{
	public static void GetTransportersInGroup(int transportersGroup, Map map, List<CompTransporter> outTransporters)
	{
		outTransporters.Clear();
		if (transportersGroup < 0)
		{
			return;
		}
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
		for (int i = 0; i < list.Count; i++)
		{
			CompTransporter compTransporter = list[i].TryGetComp<CompTransporter>();
			if (compTransporter.groupID == transportersGroup)
			{
				outTransporters.Add(compTransporter);
			}
		}
	}

	public static Lord FindLord(int transportersGroup, Map map)
	{
		List<Lord> lords = map.lordManager.lords;
		for (int i = 0; i < lords.Count; i++)
		{
			if (lords[i].LordJob is LordJob_LoadAndEnterTransporters lordJob_LoadAndEnterTransporters && lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
			{
				return lords[i];
			}
		}
		return null;
	}

	public static bool WasLoadingCanceled(Thing transporter)
	{
		CompTransporter compTransporter = transporter.TryGetComp<CompTransporter>();
		if (compTransporter != null && !compTransporter.LoadingInProgressOrReadyToLaunch)
		{
			return true;
		}
		return false;
	}

	public static int InitiateLoading(IEnumerable<CompTransporter> transporters)
	{
		int nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
		foreach (CompTransporter transporter in transporters)
		{
			transporter.groupID = nextTransporterGroupID;
		}
		return nextTransporterGroupID;
	}

	public static IEnumerable<Pawn> AllSendablePawns(List<CompTransporter> transporters, Map map)
	{
		CompShuttle shuttle = transporters[0].parent.TryGetComp<CompShuttle>();
		int allowLoadAndEnterTransportersLordForGroupID = ((transporters[0].Props.canChangeAssignedThingsAfterStarting && transporters[0].LoadingInProgressOrReadyToLaunch) ? transporters[0].groupID : (-1));
		List<Pawn> pawns = CaravanFormingUtility.AllSendablePawns(map, allowEvenIfDowned: true, allowEvenIfInMentalState: false, allowEvenIfPrisonerNotSecure: false, allowCapturableDownedPawns: false, shuttle != null, allowLoadAndEnterTransportersLordForGroupID);
		for (int i = 0; i < pawns.Count; i++)
		{
			if (shuttle == null || shuttle.IsRequired(pawns[i]) || shuttle.IsAllowed(pawns[i]))
			{
				yield return pawns[i];
			}
		}
	}

	public static IEnumerable<Thing> AllSendableItems(List<CompTransporter> transporters, Map map)
	{
		List<Thing> items = CaravanFormingUtility.AllReachableColonyItems(map, !map.IsPlayerHome, transporters[0].Props.canChangeAssignedThingsAfterStarting && transporters[0].LoadingInProgressOrReadyToLaunch);
		CompShuttle shuttle = transporters[0].parent.TryGetComp<CompShuttle>();
		for (int i = 0; i < items.Count; i++)
		{
			if (shuttle == null || shuttle.IsRequired(items[i]) || shuttle.IsAllowed(items[i]))
			{
				yield return items[i];
			}
		}
	}

	public static IEnumerable<Thing> ThingsBeingHauledTo(List<CompTransporter> transporters, Map map)
	{
		IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].CurJobDef == JobDefOf.HaulToTransporter && transporters.Contains(((JobDriver_HaulToTransporter)pawns[i].jobs.curDriver).Transporter) && pawns[i].carryTracker.CarriedThing != null)
			{
				yield return pawns[i].carryTracker.CarriedThing;
			}
		}
	}

	public static void MakeLordsAsAppropriate(List<Pawn> pawns, List<CompTransporter> transporters, Map map)
	{
		int groupID = transporters[0].groupID;
		Lord lord = null;
		IEnumerable<Pawn> enumerable = pawns.Where((Pawn x) => (x.IsColonist || x.IsColonyMechPlayerControlled) && !x.Downed && x.Spawned);
		if (enumerable.Any())
		{
			lord = map.lordManager.lords.Find((Lord x) => x.LordJob is LordJob_LoadAndEnterTransporters lordJob_LoadAndEnterTransporters2 && lordJob_LoadAndEnterTransporters2.transportersGroup == groupID);
			if (lord == null)
			{
				lord = LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterTransporters(groupID), map);
			}
			foreach (Pawn item in enumerable)
			{
				if (!lord.ownedPawns.Contains(item))
				{
					item.GetLord()?.Notify_PawnLost(item, PawnLostCondition.ForcedToJoinOtherLord);
					lord.AddPawn(item);
					item.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
			for (int num = lord.ownedPawns.Count - 1; num >= 0; num--)
			{
				if (!enumerable.Contains(lord.ownedPawns[num]))
				{
					lord.Notify_PawnLost(lord.ownedPawns[num], PawnLostCondition.NoLongerEnteringTransportPods);
				}
			}
		}
		for (int num2 = map.lordManager.lords.Count - 1; num2 >= 0; num2--)
		{
			if (map.lordManager.lords[num2].LordJob is LordJob_LoadAndEnterTransporters lordJob_LoadAndEnterTransporters && lordJob_LoadAndEnterTransporters.transportersGroup == groupID && map.lordManager.lords[num2] != lord)
			{
				map.lordManager.RemoveLord(map.lordManager.lords[num2]);
			}
		}
	}

	public static bool IncomingTransporterPreventingMapRemoval(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			WorldComponent_GravshipController gravshipController = Find.GravshipController;
			if (gravshipController != null && gravshipController.LandingAreaConfirmationInProgress)
			{
				return true;
			}
			if (Find.CurrentGravship != null && Find.CurrentGravship.destinationTile == map.Tile)
			{
				return true;
			}
		}
		foreach (TravellingTransporters travellingTransporter in Find.WorldObjects.TravellingTransporters)
		{
			if (travellingTransporter.destinationTile == map.Tile)
			{
				return true;
			}
		}
		return false;
	}
}
