using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public static class DropPodUtility
	{
		private static List<List<Thing>> tempList = new List<List<Thing>>();

		public static void MakeDropPodAt(IntVec3 c, Map map, ActiveDropPodInfo info)
		{
			ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
			activeDropPod.Contents = info;
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.DropPodIncoming, activeDropPod, c, map);
			foreach (Thing item in (IEnumerable<Thing>)activeDropPod.Contents.innerContainer)
			{
				Pawn pawn;
				if ((pawn = item as Pawn) != null && pawn.IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(pawn);
					pawn.psychicEntropy?.SetInitialPsyfocusLevel();
				}
			}
		}

		public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true)
		{
			tempList.Clear();
			foreach (Thing thing in things)
			{
				List<Thing> list = new List<Thing>();
				list.Add(thing);
				tempList.Add(list);
			}
			DropThingGroupsNear(dropCenter, map, tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch, forbid);
			tempList.Clear();
		}

		public static void DropThingGroupsNear_NewTmp(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true, bool allowFogged = true)
		{
			foreach (List<Thing> thingsGroup in thingsGroups)
			{
				if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out var result, allowFogged, canRoofPunch) && (canRoofPunch || !DropCellFinder.TryFindDropSpotNear(dropCenter, map, out result, allowFogged, canRoofPunch: true)))
				{
					Log.Warning(string.Concat("DropThingsNear failed to find a place to drop ", thingsGroup.FirstOrDefault(), " near ", dropCenter, ". Dropping on random square instead."));
					result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map), map);
				}
				if (forbid)
				{
					for (int i = 0; i < thingsGroup.Count; i++)
					{
						thingsGroup[i].SetForbidden(value: true, warnOnFail: false);
					}
				}
				if (instaDrop)
				{
					foreach (Thing item in thingsGroup)
					{
						GenPlace.TryPlaceThing(item, result, map, ThingPlaceMode.Near);
					}
					continue;
				}
				ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
				foreach (Thing item2 in thingsGroup)
				{
					activeDropPodInfo.innerContainer.TryAdd(item2);
				}
				activeDropPodInfo.openDelay = openDelay;
				activeDropPodInfo.leaveSlag = leaveSlag;
				MakeDropPodAt(result, map, activeDropPodInfo);
			}
		}

		public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true)
		{
			DropThingGroupsNear_NewTmp(dropCenter, map, thingsGroups, openDelay, instaDrop, leaveSlag, canRoofPunch, forbid);
		}
	}
}
