using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class MapDeiniter
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public static void Deinit(Map map)
		{
			try
			{
				DoQueuedPowerTasks(map);
			}
			catch (Exception arg)
			{
				Log.Error("Error while deiniting map: could not execute power related tasks: " + arg);
			}
			try
			{
				PassPawnsToWorld(map);
			}
			catch (Exception arg2)
			{
				Log.Error("Error while deiniting map: could not pass pawns to world: " + arg2);
			}
			try
			{
				map.weatherManager.EndAllSustainers();
			}
			catch (Exception arg3)
			{
				Log.Error("Error while deiniting map: could not end all weather sustainers: " + arg3);
			}
			try
			{
				Find.SoundRoot.sustainerManager.EndAllInMap(map);
			}
			catch (Exception arg4)
			{
				Log.Error("Error while deiniting map: could not end all effect sustainers: " + arg4);
			}
			try
			{
				map.areaManager.Notify_MapRemoved();
			}
			catch (Exception arg5)
			{
				Log.Error("Error while deiniting map: could not remove areas: " + arg5);
			}
			try
			{
				Find.TickManager.RemoveAllFromMap(map);
			}
			catch (Exception arg6)
			{
				Log.Error("Error while deiniting map: could not remove things from the tick manager: " + arg6);
			}
			try
			{
				NotifyEverythingWhichUsesMapReference(map);
			}
			catch (Exception arg7)
			{
				Log.Error("Error while deiniting map: could not notify things/regions/rooms/etc: " + arg7);
			}
			try
			{
				map.listerThings.Clear();
				map.spawnedThings.Clear();
			}
			catch (Exception arg8)
			{
				Log.Error("Error while deiniting map: could not remove things from thing listers: " + arg8);
			}
			try
			{
				Find.Archive.Notify_MapRemoved(map);
			}
			catch (Exception arg9)
			{
				Log.Error("Error while deiniting map: could not remove look targets: " + arg9);
			}
			try
			{
				Find.Storyteller.incidentQueue.Notify_MapRemoved(map);
			}
			catch (Exception arg10)
			{
				Log.Error("Error while deiniting map: could not remove queued incidents: " + arg10);
			}
		}

		private static void DoQueuedPowerTasks(Map map)
		{
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
		}

		private static void PassPawnsToWorld(Map map)
		{
			List<Pawn> list = new List<Pawn>();
			List<Pawn> list2 = new List<Pawn>();
			bool flag = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer);
			List<Pawn> list3 = map.mapPawns.AllPawns.ToList();
			for (int i = 0; i < list3.Count; i++)
			{
				Find.Storyteller.Notify_PawnEvent(list3[i], AdaptationEvent.LostBecauseMapClosed);
				try
				{
					Pawn pawn = list3[i];
					if (pawn.Spawned)
					{
						pawn.DeSpawn();
					}
					if (pawn.IsColonist && flag)
					{
						list.Add(pawn);
						map.ParentFaction.kidnapped.Kidnap(pawn, null);
						continue;
					}
					if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
					{
						list2.Add(pawn);
						PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, null, PawnDiedOrDownedThoughtsKind.Lost);
					}
					CleanUpAndPassToWorld(pawn);
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Could not despawn and pass to world ", list3[i], ": ", ex));
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				QuestUtility.SendQuestTargetSignals(list[j].questTags, "LeftMap", list[j].Named("SUBJECT"));
			}
			for (int k = 0; k < list2.Count; k++)
			{
				QuestUtility.SendQuestTargetSignals(list2[k].questTags, "LeftMap", list2[k].Named("SUBJECT"));
			}
			if (!list.Any() && !list2.Any())
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (list.Any())
			{
				list.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
				for (int l = 0; l < list.Count; l++)
				{
					stringBuilder.AppendLine("  - " + list[l].NameShortColored.Resolve() + ": " + "capturedBy".Translate(map.ParentFaction.NameColored.Resolve()).CapitalizeFirst());
				}
			}
			if (list2.Any())
			{
				list2.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
				for (int m = 0; m < list2.Count; m++)
				{
					stringBuilder.AppendLine("  - " + list2[m].NameShortColored.Resolve());
				}
			}
			string str;
			string str2;
			if (map.IsPlayerHome)
			{
				str = "LetterLabelPawnsLostBecauseMapClosed_Home".Translate();
				str2 = "LetterPawnsLostBecauseMapClosed_Home".Translate();
			}
			else
			{
				str = "LetterLabelPawnsLostBecauseMapClosed_Caravan".Translate();
				str2 = "LetterPawnsLostBecauseMapClosed_Caravan".Translate();
			}
			str2 = str2 + ":\n\n" + stringBuilder.ToString().TrimEndNewlines();
			Find.LetterStack.ReceiveLetter(str, str2, LetterDefOf.NegativeEvent, new GlobalTargetInfo(map.Tile));
		}

		private static void CleanUpAndPassToWorld(Pawn p)
		{
			if (p.ownership != null)
			{
				p.ownership.UnclaimAll();
			}
			if (p.guest != null)
			{
				p.guest.SetGuestStatus(null);
			}
			p.inventory.UnloadEverything = false;
			Find.WorldPawns.PassToWorld(p);
		}

		private static void NotifyEverythingWhichUsesMapReference(Map map)
		{
			List<Map> maps = Find.Maps;
			int num = maps.IndexOf(map);
			ThingOwnerUtility.GetAllThingsRecursively(map, tmpThings);
			for (int i = 0; i < tmpThings.Count; i++)
			{
				tmpThings[i].Notify_MyMapRemoved();
			}
			tmpThings.Clear();
			for (int j = num; j < maps.Count; j++)
			{
				ThingOwner spawnedThings = maps[j].spawnedThings;
				for (int k = 0; k < spawnedThings.Count; k++)
				{
					if (j != num)
					{
						spawnedThings[k].DecrementMapIndex();
					}
				}
				List<Room> allRooms = maps[j].regionGrid.allRooms;
				for (int l = 0; l < allRooms.Count; l++)
				{
					if (j == num)
					{
						allRooms[l].Notify_MyMapRemoved();
					}
					else
					{
						allRooms[l].DecrementMapIndex();
					}
				}
				foreach (Region item in maps[j].regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (j == num)
					{
						item.Notify_MyMapRemoved();
					}
					else
					{
						item.DecrementMapIndex();
					}
				}
			}
		}
	}
}
