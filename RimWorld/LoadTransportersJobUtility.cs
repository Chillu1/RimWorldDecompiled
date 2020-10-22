using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class LoadTransportersJobUtility
	{
		private static HashSet<Thing> neededThings = new HashSet<Thing>();

		private static Dictionary<TransferableOneWay, int> tmpAlreadyLoading = new Dictionary<TransferableOneWay, int>();

		public static bool HasJobOnTransporter(Pawn pawn, CompTransporter transporter)
		{
			if (transporter.parent.IsForbidden(pawn))
			{
				return false;
			}
			if (!transporter.AnythingLeftToLoad)
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return false;
			}
			if (!pawn.CanReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				return false;
			}
			if (FindThingToLoad(pawn, transporter).Thing == null)
			{
				return false;
			}
			return true;
		}

		public static Job JobOnTransporter(Pawn p, CompTransporter transporter)
		{
			Job job = JobMaker.MakeJob(JobDefOf.HaulToTransporter, LocalTargetInfo.Invalid, transporter.parent);
			job.ignoreForbidden = true;
			return job;
		}

		public static ThingCount FindThingToLoad(Pawn p, CompTransporter transporter)
		{
			neededThings.Clear();
			List<TransferableOneWay> leftToLoad = transporter.leftToLoad;
			tmpAlreadyLoading.Clear();
			if (leftToLoad != null)
			{
				List<Pawn> allPawnsSpawned = transporter.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i] == p || allPawnsSpawned[i].CurJobDef != JobDefOf.HaulToTransporter)
					{
						continue;
					}
					JobDriver_HaulToTransporter jobDriver_HaulToTransporter = (JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver;
					if (jobDriver_HaulToTransporter.Container != transporter.parent)
					{
						continue;
					}
					TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(jobDriver_HaulToTransporter.ThingToCarry, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
					if (transferableOneWay != null)
					{
						int value = 0;
						if (tmpAlreadyLoading.TryGetValue(transferableOneWay, out value))
						{
							tmpAlreadyLoading[transferableOneWay] = value + jobDriver_HaulToTransporter.initialCount;
						}
						else
						{
							tmpAlreadyLoading.Add(transferableOneWay, jobDriver_HaulToTransporter.initialCount);
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
			Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p), 9999f, (Thing x) => neededThings.Contains(x) && p.CanReserve(x));
			if (thing == null)
			{
				foreach (Thing neededThing in neededThings)
				{
					Pawn pawn = neededThing as Pawn;
					if (pawn != null && (!pawn.IsColonist || pawn.Downed) && !pawn.inventory.UnloadEverything && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly))
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
				for (int l = 0; l < leftToLoad.Count; l++)
				{
					if (leftToLoad[l].things.Contains(thing))
					{
						transferableOneWay3 = leftToLoad[l];
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
	}
}
