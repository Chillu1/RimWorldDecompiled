using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class EnterPortalUtility
{
	private static HashSet<Thing> neededThings = new HashSet<Thing>();

	private static Dictionary<TransferableOneWay, int> tmpAlreadyLoading = new Dictionary<TransferableOneWay, int>();

	public static bool HasJobOnPortal(Pawn pawn, MapPortal portal)
	{
		if (portal == null)
		{
			return false;
		}
		if (portal.leftToLoad.NullOrEmpty())
		{
			return false;
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return false;
		}
		if (!pawn.CanReach(portal, PathEndMode.Touch, pawn.NormalMaxDanger()))
		{
			return false;
		}
		if (FindThingToLoad(pawn, portal).Thing == null)
		{
			return false;
		}
		return true;
	}

	public static Job JobOnPortal(Pawn p, MapPortal portal)
	{
		Job job = JobMaker.MakeJob(JobDefOf.HaulToPortal, LocalTargetInfo.Invalid, portal);
		job.ignoreForbidden = true;
		return job;
	}

	public static ThingCount FindThingToLoad(Pawn p, MapPortal portal)
	{
		neededThings.Clear();
		List<TransferableOneWay> leftToLoad = portal.leftToLoad;
		tmpAlreadyLoading.Clear();
		if (leftToLoad != null)
		{
			List<Pawn> list = portal.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == p || list[i].CurJobDef != JobDefOf.HaulToPortal)
				{
					continue;
				}
				JobDriver_HaulToPortal jobDriver_HaulToPortal = (JobDriver_HaulToPortal)list[i].jobs.curDriver;
				if (jobDriver_HaulToPortal.Container != portal)
				{
					continue;
				}
				TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(jobDriver_HaulToPortal.ThingToCarry, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
				if (transferableOneWay != null)
				{
					int value = 0;
					if (tmpAlreadyLoading.TryGetValue(transferableOneWay, out value))
					{
						tmpAlreadyLoading[transferableOneWay] = value + jobDriver_HaulToPortal.initialCount;
					}
					else
					{
						tmpAlreadyLoading.Add(transferableOneWay, jobDriver_HaulToPortal.initialCount);
					}
				}
			}
			for (int j = 0; j < leftToLoad.Count; j++)
			{
				TransferableOneWay transferableOneWay2 = leftToLoad[j];
				if (!tmpAlreadyLoading.TryGetValue(leftToLoad[j], out var value2))
				{
					value2 = 0;
				}
				if (transferableOneWay2.CountToTransfer - value2 > 0)
				{
					for (int k = 0; k < transferableOneWay2.things.Count; k++)
					{
						neededThings.Add(transferableOneWay2.things[k]);
					}
				}
			}
		}
		if (!neededThings.Any())
		{
			tmpAlreadyLoading.Clear();
			return default(ThingCount);
		}
		Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p), 9999f, (Thing x) => neededThings.Contains(x) && p.CanReserve(x) && !x.IsForbidden(p) && p.carryTracker.AvailableStackSpace(x.def) > 0, null, 0, -1, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: false, lookInHaulSources: true);
		if (thing == null)
		{
			foreach (Thing neededThing in neededThings)
			{
				if (neededThing is Pawn pawn && ((!pawn.IsColonist && !pawn.IsColonyMech) || pawn.Downed || pawn.IsSelfShutdown()) && !pawn.inventory.UnloadEverything && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly))
				{
					neededThings.Clear();
					tmpAlreadyLoading.Clear();
					return new ThingCount(pawn, 1);
				}
			}
		}
		neededThings.Clear();
		if (thing != null)
		{
			TransferableOneWay transferableOneWay3 = null;
			for (int num = 0; num < leftToLoad.Count; num++)
			{
				if (leftToLoad[num].things.Contains(thing))
				{
					transferableOneWay3 = leftToLoad[num];
					break;
				}
			}
			if (!tmpAlreadyLoading.TryGetValue(transferableOneWay3, out var value3))
			{
				value3 = 0;
			}
			tmpAlreadyLoading.Clear();
			return new ThingCount(thing, Mathf.Min(transferableOneWay3.CountToTransfer - value3, thing.stackCount));
		}
		tmpAlreadyLoading.Clear();
		return default(ThingCount);
	}

	public static IEnumerable<Thing> ThingsBeingHauledTo(MapPortal portal)
	{
		IReadOnlyList<Pawn> pawns = portal.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].CurJobDef == JobDefOf.HaulToPortal && ((JobDriver_HaulToPortal)pawns[i].jobs.curDriver).MapPortal == portal && pawns[i].carryTracker.CarriedThing != null)
			{
				yield return pawns[i].carryTracker.CarriedThing;
			}
		}
	}

	public static void MakeLordsAsAppropriate(List<Pawn> pawns, MapPortal portal)
	{
		Lord lord = null;
		IEnumerable<Pawn> enumerable = pawns.Where(delegate(Pawn x)
		{
			if ((x.IsColonist || x.IsColonyMechPlayerControlled) && !x.Downed)
			{
				Pawn_NeedsTracker needs = x.needs;
				if (needs == null || needs.energy?.IsSelfShutdown != true)
				{
					return x.Spawned;
				}
			}
			return false;
		});
		if (enumerable.Any())
		{
			lord = portal.Map.lordManager.lords.Find((Lord x) => x.LordJob is LordJob_LoadAndEnterPortal && ((LordJob_LoadAndEnterPortal)x.LordJob).portal == portal);
			if (lord == null)
			{
				lord = LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterPortal(portal), portal.Map);
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
					lord.Notify_PawnLost(lord.ownedPawns[num], PawnLostCondition.LordRejected);
				}
			}
		}
		for (int num2 = portal.Map.lordManager.lords.Count - 1; num2 >= 0; num2--)
		{
			if (portal.Map.lordManager.lords[num2].LordJob is LordJob_LoadAndEnterPortal lordJob_LoadAndEnterPortal && lordJob_LoadAndEnterPortal.portal == portal && portal.Map.lordManager.lords[num2] != lord)
			{
				portal.Map.lordManager.RemoveLord(portal.Map.lordManager.lords[num2]);
			}
		}
	}

	public static bool WasLoadingCanceled(Thing thing)
	{
		if (thing is MapPortal { LoadInProgress: false })
		{
			return true;
		}
		return false;
	}
}
