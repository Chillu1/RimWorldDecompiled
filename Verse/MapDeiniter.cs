using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class MapDeiniter
{
	private static List<Thing> tmpThings = new List<Thing>();

	public static void Deinit(Map map, bool notifyPlayer)
	{
		try
		{
			DoQueuedPowerTasks(map);
		}
		catch (Exception ex)
		{
			Log.Error("Error while deiniting map: could not execute power related tasks: " + ex);
		}
		try
		{
			PassPawnsToWorld(map, notifyPlayer);
		}
		catch (Exception ex2)
		{
			Log.Error("Error while deiniting map: could not pass pawns to world: " + ex2);
		}
		try
		{
			foreach (Pawn item in PawnsFinder.All_AliveOrDead)
			{
				if (item.playerSettings != null)
				{
					item.playerSettings.Notify_MapRemoved(map);
				}
			}
		}
		catch (Exception ex3)
		{
			Log.Error("Error while deiniting map: could not notify pawn player settings: " + ex3);
		}
		try
		{
			map.weatherManager.EndAllSustainers();
		}
		catch (Exception ex4)
		{
			Log.Error("Error while deiniting map: could not end all weather sustainers: " + ex4);
		}
		try
		{
			Find.SoundRoot.sustainerManager.EndAllInMap(map);
		}
		catch (Exception ex5)
		{
			Log.Error("Error while deiniting map: could not end all effect sustainers: " + ex5);
		}
		try
		{
			map.areaManager.Notify_MapRemoved();
		}
		catch (Exception ex6)
		{
			Log.Error("Error while deiniting map: could not remove areas: " + ex6);
		}
		try
		{
			map.lordManager.Notify_MapRemoved();
		}
		catch (Exception ex7)
		{
			Log.Error("Error while deiniting map and notifying LordManager: " + ex7);
		}
		try
		{
			map.deferredSpawner.Notify_MapRemoved();
		}
		catch (Exception ex8)
		{
			Log.Error("Error while deiniting map and notifying DeferredSpawner: " + ex8);
		}
		try
		{
			Find.Anomaly.Notify_MapRemoved(map);
		}
		catch (Exception ex9)
		{
			Log.Error("Error while deiniting map and notifying Anomaly component: " + ex9);
		}
		try
		{
			Find.TickManager.RemoveAllFromMap(map);
		}
		catch (Exception ex10)
		{
			Log.Error("Error while deiniting map: could not remove things from the tick manager: " + ex10);
		}
		try
		{
			NotifyEverythingWhichUsesMapReference(map);
		}
		catch (Exception ex11)
		{
			Log.Error("Error while deiniting map: could not notify things/regions/rooms/etc: " + ex11);
		}
		try
		{
			map.listerThings.Clear();
			map.spawnedThings.Clear();
		}
		catch (Exception ex12)
		{
			Log.Error("Error while deiniting map: could not remove things from thing listers: " + ex12);
		}
		try
		{
			Find.Archive.Notify_MapRemoved(map);
		}
		catch (Exception ex13)
		{
			Log.Error("Error while deiniting map: could not remove look targets: " + ex13);
		}
		try
		{
			Find.Storyteller.incidentQueue.Notify_MapRemoved(map);
		}
		catch (Exception ex14)
		{
			Log.Error("Error while deiniting map: could not remove queued incidents: " + ex14);
		}
	}

	private static void DoQueuedPowerTasks(Map map)
	{
		map.powerNetManager.UpdatePowerNetsAndConnections_First();
	}

	private static void PassPawnsToWorld(Map map, bool notifyPlayer)
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
				pawn.DeSpawnOrDeselect();
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
				Log.Error("Could not despawn and pass to world " + list3[i]?.ToString() + ": " + ex);
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
		if (!notifyPlayer || (!list.Any() && !list2.Any()))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (list.Any())
		{
			list.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
			for (int num = 0; num < list.Count; num++)
			{
				stringBuilder.AppendLineTagged("  - " + list[num].NameShortColored.CapitalizeFirst() + ": " + "capturedBy".Translate(map.ParentFaction.NameColored).CapitalizeFirst());
			}
		}
		if (list2.Any())
		{
			list2.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				stringBuilder.AppendLine("  - " + list2[num2].NameShortColored.Resolve().CapitalizeFirst());
			}
		}
		string text;
		string text2;
		if (map.IsPlayerHome || map.IsPocketMap)
		{
			text = "LetterLabelPawnsLostBecauseMapClosed_Home".Translate();
			text2 = (ModsConfig.BiotechActive ? "LetterPawnsLostIncMechsBecauseMapClosed_Home".Translate() : "LetterPawnsLostBecauseMapClosed_Home".Translate());
		}
		else
		{
			text = "LetterLabelPawnsLostBecauseMapClosed_Caravan".Translate();
			text2 = (ModsConfig.BiotechActive ? "LetterPawnsLostIncMechsBecauseMapClosed_Caravan".Translate() : "LetterPawnsLostBecauseMapClosed_Caravan".Translate());
		}
		text2 = text2 + ":\n\n" + stringBuilder.ToString().TrimEndNewlines();
		Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.NegativeEvent, new GlobalTargetInfo(map.Tile));
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
		if (ModsConfig.BiotechActive && p.IsColonyMech)
		{
			Pawn overseer = p.GetOverseer();
			if (overseer != null)
			{
				p.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, overseer);
			}
		}
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
			if (tmpThings[i].Discarded)
			{
				tmpThings[i].Notify_AbandonedAtTile(map.Tile);
			}
		}
		tmpThings.Clear();
		map.spawnedThings.Clear();
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
			IReadOnlyList<Room> allRooms = maps[j].regionGrid.AllRooms;
			for (int l = 0; l < allRooms.Count; l++)
			{
				List<District> districts = allRooms[l].Districts;
				for (int m = 0; m < districts.Count; m++)
				{
					if (j == num)
					{
						districts[m].Notify_MyMapRemoved();
					}
					else
					{
						districts[m].DecrementMapIndex();
					}
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
