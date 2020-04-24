using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatherItemsForCaravanUtility
	{
		private static HashSet<Thing> neededItems = new HashSet<Thing>();

		public static Thing FindThingToHaul(Pawn p, Lord lord)
		{
			neededItems.Clear();
			List<TransferableOneWay> transferables = ((LordJob_FormAndSendCaravan)lord.LordJob).transferables;
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (CountLeftToTransfer(p, transferableOneWay, lord) > 0)
				{
					for (int j = 0; j < transferableOneWay.things.Count; j++)
					{
						neededItems.Add(transferableOneWay.things[j]);
					}
				}
			}
			if (!neededItems.Any())
			{
				return null;
			}
			Thing result = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p), 9999f, (Thing x) => neededItems.Contains(x) && p.CanReserve(x));
			neededItems.Clear();
			return result;
		}

		public static int CountLeftToTransfer(Pawn pawn, TransferableOneWay transferable, Lord lord)
		{
			if (transferable.CountToTransfer <= 0 || !transferable.HasAnyThing)
			{
				return 0;
			}
			return Mathf.Max(transferable.CountToTransfer - TransferableCountHauledByOthers(pawn, transferable, lord), 0);
		}

		private static int TransferableCountHauledByOthers(Pawn pawn, TransferableOneWay transferable, Lord lord)
		{
			if (!transferable.HasAnyThing)
			{
				Log.Warning("Can't determine transferable count hauled by others because transferable has 0 things.");
				return 0;
			}
			List<Pawn> allPawnsSpawned = lord.Map.mapPawns.AllPawnsSpawned;
			int num = 0;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn2 = allPawnsSpawned[i];
				if (pawn2 != pawn && pawn2.CurJob != null && pawn2.CurJob.def == JobDefOf.PrepareCaravan_GatherItems && pawn2.CurJob.lord == lord)
				{
					Thing toHaul = ((JobDriver_PrepareCaravan_GatherItems)pawn2.jobs.curDriver).ToHaul;
					if (transferable.things.Contains(toHaul) || TransferableUtility.TransferAsOne(transferable.AnyThing, toHaul, TransferAsOneMode.PodsOrCaravanPacking))
					{
						num += toHaul.stackCount;
					}
				}
			}
			return num;
		}
	}
}
