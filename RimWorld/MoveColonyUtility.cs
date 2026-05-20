using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class MoveColonyUtility
{
	private static StringBuilder cannotPlaceTileReason = new StringBuilder();

	public const int TitleAndRoleRequirementGracePeriodTicks = 600000;

	private static List<PlanetTile> playerSettlementsRemoved = new List<PlanetTile>();

	public static bool TitleAndRoleRequirementsGracePeriodActive => TitleAndRoleRequirementGracePeriodTicksLeft > 0;

	public static int TitleAndRoleRequirementGracePeriodTicksLeft
	{
		get
		{
			if (!Find.TickManager.HasSettledNewColony)
			{
				return 0;
			}
			return Mathf.Max(0, 600000 - Find.TickManager.TicksSinceSettle);
		}
	}

	public static void PickNewColonyTile(Action<PlanetTile> targetChosen, Action noTileChosen = null)
	{
		Find.TilePicker.StartTargeting_NewTemp(delegate(PlanetTile tile)
		{
			cannotPlaceTileReason.Clear();
			if (!TileFinder.IsValidTileForNewSettlement(tile, cannotPlaceTileReason))
			{
				Messages.Message(cannotPlaceTileReason.ToString(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			return true;
		}, delegate(PlanetTile tile)
		{
			Find.World.renderer.wantedMode = WorldRenderMode.None;
			targetChosen(tile);
		}, null, null, allowEscape: false, noTileChosen, "ChooseNextColonySite".Translate());
	}

	public static Settlement MoveColonyAndReset(PlanetTile tile, IEnumerable<Thing> colonyThings, Faction takeoverFaction = null, WorldObjectDef worldObjectDef = null)
	{
		foreach (Quest item in Find.QuestManager.QuestsListForReading)
		{
			if (item.IsEndOnNewArchonexusSettlement())
			{
				item.hidden = true;
				item.End(QuestEndOutcome.Unknown, sendLetter: false);
			}
		}
		List<Pawn> list = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction.ToList();
		foreach (Pawn item2 in list)
		{
			if (colonyThings.Contains(item2))
			{
				continue;
			}
			item2.DeSpawnOrDeselect();
			if (item2 == item2.Faction.leader)
			{
				item2.Faction.Notify_LeaderLost();
			}
			if (item2.IsCaravanMember())
			{
				item2.GetCaravan().RemovePawn(item2);
			}
			if (item2.holdingOwner != null)
			{
				item2.holdingOwner.Remove(item2);
			}
			if (!item2.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(item2);
			}
			if (ModsConfig.BiotechActive && item2.RaceProps.IsMechanoid)
			{
				Pawn overseer = item2.GetOverseer();
				if (overseer != null && colonyThings.Contains(overseer))
				{
					overseer.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, item2);
				}
			}
		}
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int num = caravans.Count - 1; num >= 0; num--)
		{
			if (caravans[num].IsPlayerControlled)
			{
				caravans[num].RemoveAllPawns();
				caravans[num].Destroy();
			}
		}
		List<TravellingTransporters> travellingTransporters = Find.WorldObjects.TravellingTransporters;
		for (int num2 = travellingTransporters.Count - 1; num2 >= 0; num2--)
		{
			travellingTransporters[num2].Destroy();
		}
		foreach (Thing colonyThing in colonyThings)
		{
			colonyThing.DeSpawnOrDeselect();
			if (colonyThing.holdingOwner != null)
			{
				colonyThing.holdingOwner.Remove(colonyThing);
			}
		}
		List<MapParent> mapParents = Find.World.worldObjects.MapParents;
		for (int num3 = mapParents.Count - 1; num3 >= 0; num3--)
		{
			mapParents[num3].CheckRemoveMapNow();
		}
		playerSettlementsRemoved.Clear();
		List<Map> maps = Find.Maps;
		for (int num4 = maps.Count - 1; num4 >= 0; num4--)
		{
			Map map = maps[num4];
			if (map.IsPlayerHome)
			{
				playerSettlementsRemoved.Add(map.Tile);
				map.Parent.SetFaction(null);
				Current.Game.DeinitAndRemoveMap(map, notifyPlayer: false);
				map.Parent.Destroy();
				foreach (Pawn item3 in list)
				{
					if (item3.playerSettings != null)
					{
						item3.playerSettings.Notify_MapRemoved(map);
					}
				}
			}
		}
		if (ModsConfig.IdeologyActive)
		{
			List<Site> sites = Find.WorldObjects.Sites;
			for (int num5 = sites.Count - 1; num5 >= 0; num5--)
			{
				if (sites[num5].parts.Any((SitePart p) => p.def == SitePartDefOf.AncientComplex))
				{
					Find.WorldObjects.Remove(sites[num5]);
				}
			}
		}
		if (ModsConfig.AnomalyActive)
		{
			Find.Anomaly.ResetMonolith();
			Find.Anomaly.monolith = null;
		}
		if (ModsConfig.OdysseyActive)
		{
			Find.QuestManager.QuestsListForReading.RemoveAll((Quest q) => q.root == QuestScriptDefOf.MechanoidSignal);
		}
		Find.GameInfo.startingTile = tile;
		WorldObjectDef worldObjectDef2 = worldObjectDef ?? tile.LayerDef.SettlementWorldObjectDef;
		Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(worldObjectDef2);
		settlement.SetFaction(Faction.OfPlayer);
		settlement.Tile = tile;
		settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, Faction.OfPlayer.def.playerInitialSettlementNameMaker);
		Find.WorldObjects.Add(settlement);
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, worldObjectDef2);
		IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
		List<List<Thing>> list2 = new List<List<Thing>>();
		List<Pawn> list3 = new List<Pawn>();
		foreach (Thing colonyThing2 in colonyThings)
		{
			if (colonyThing2 is Pawn)
			{
				list2.Add(new List<Thing> { colonyThing2 });
				list3.Add((Pawn)colonyThing2);
			}
		}
		int num6 = 0;
		foreach (Thing thing in colonyThings)
		{
			if (thing.def.CanHaveFaction && thing.Faction != Faction.OfPlayer)
			{
				if (thing is Pawn { IsSlaveOfColony: not false })
				{
					continue;
				}
				thing.SetFaction(Faction.OfPlayer);
			}
			if (!list2.Any((List<Thing> g) => g.Contains(thing)))
			{
				list2[num6].Add(thing);
				num6 = (num6 + 1) % list2.Count;
			}
		}
		foreach (Pawn item4 in list3)
		{
			item4.inventory.DestroyAll();
			item4.ownership.UnclaimThrone();
			item4.ownership.UnclaimDeathrestCasket();
			item4.genes?.Notify_NewColony();
		}
		RemoveWeaponsAndUtilityItems(list3, colonyThings);
		foreach (Thing abandonedRelicsCarriedByPawn in GetAbandonedRelicsCarriedByPawns(colonyThings))
		{
			if (!abandonedRelicsCarriedByPawn.DestroyedOrNull())
			{
				abandonedRelicsCarriedByPawn.Destroy();
			}
		}
		DropPodUtility.DropThingGroupsNear(playerStartSpot, orGenerateMap, list2);
		if (takeoverFaction != null)
		{
			foreach (PlanetTile item5 in playerSettlementsRemoved)
			{
				SettleUtility.AddNewHome(item5, takeoverFaction);
			}
		}
		playerSettlementsRemoved.Clear();
		List<ResearchProjectDef> list4 = DefDatabase<ResearchProjectDef>.AllDefs.Where((ResearchProjectDef proj) => proj.IsFinished && proj.HasTag(ResearchProjectTagDefOf.ClassicStart)).ToList();
		Find.ResearchManager.ResetAllProgress();
		Find.EntityCodex?.Reset();
		ResearchUtility.ApplyPlayerStartingResearch();
		foreach (ResearchProjectDef item6 in list4)
		{
			Find.ResearchManager.FinishProject(item6, doCompletionDialog: false, null, doCompletionLetter: false);
		}
		FactionUtility.ResetAllFactionRelations();
		if (ModsConfig.BiotechActive)
		{
			Current.Game.GetComponent<GameComponent_Bossgroup>()?.ResetProgress();
		}
		SetPawnThoughts(list3);
		ResetNeedLevels(list3);
		RemoveHediffs(list3);
		ResetStartingGracePeriods(list3);
		Find.FactionManager.OfPlayer.ideos.RecalculateIdeosBasedOnPlayerPawns();
		IdeoUtility.Notify_NewColonyStarted();
		return settlement;
	}

	public static IEnumerable<Thing> GetStartingThingsForNewColony()
	{
		foreach (ScenPart allPart in Find.Scenario.AllParts)
		{
			if (!(allPart is ScenPart_StartingThing_Defined scenPart_StartingThing_Defined))
			{
				continue;
			}
			foreach (Thing item in scenPart_StartingThing_Defined.PlayerStartingThings())
			{
				yield return item;
			}
		}
	}

	public static bool IsBringableItem(Thing t)
	{
		if (t.def.destroyOnDrop)
		{
			return false;
		}
		if (t.IsRelic())
		{
			return false;
		}
		if (t.Map != null && t.Position.Fogged(t.Map))
		{
			return false;
		}
		return true;
	}

	public static bool IsDistinctArchonexusItem(ThingDef td)
	{
		if (((!td.IsWeapon && !td.IsApparel) || !td.HasComp(typeof(CompQuality))) && (td == null || td.weaponTags?.Contains("Gun") != true))
		{
			if (td == null)
			{
				return false;
			}
			return td.apparel?.LastLayer?.IsUtilityLayer == true;
		}
		return true;
	}

	public static void RemoveWeaponsAndUtilityItems(List<Pawn> pawns, IEnumerable<Thing> selectedThings)
	{
		List<Thing> list = new List<Thing>();
		foreach (Pawn pawn in pawns)
		{
			if (pawn.equipment != null)
			{
				foreach (ThingWithComps item in pawn.equipment.AllEquipmentListForReading)
				{
					if (!item.DestroyedOrNull() && !selectedThings.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			if (pawn.apparel == null)
			{
				continue;
			}
			foreach (Apparel item2 in pawn.apparel.WornApparel)
			{
				ThingDef def = item2.def;
				if (def != null && def.apparel?.LastLayer?.IsUtilityLayer == true && !selectedThings.Contains(item2))
				{
					list.Add(item2);
				}
			}
		}
		foreach (Thing item3 in list)
		{
			item3.Destroy();
		}
	}

	public static List<Thing> GetAbandonedRelicsCarriedByPawns(IEnumerable<Thing> selectedThings)
	{
		List<Thing> list = new List<Thing>();
		List<Pawn> list2 = new List<Pawn>();
		foreach (Thing selectedThing in selectedThings)
		{
			if (selectedThing.IsRelic())
			{
				list.Add(selectedThing);
			}
			if (selectedThing is Pawn)
			{
				list2.Add((Pawn)selectedThing);
			}
		}
		List<Thing> list3 = new List<Thing>();
		foreach (Pawn item in list2)
		{
			foreach (Thing equippedWornOrInventoryThing in item.EquippedWornOrInventoryThings)
			{
				if (equippedWornOrInventoryThing.IsRelic() && !list.Contains(equippedWornOrInventoryThing))
				{
					list3.Add(equippedWornOrInventoryThing);
				}
			}
		}
		return list3;
	}

	private static void ResetStartingGracePeriods(List<Pawn> pawns)
	{
		Find.TickManager.ResetSettlementTicks();
		Find.StoryWatcher.watcherAdaptation.ResetAdaptDays();
		Find.StoryWatcher.watcherPopAdaptation.ResetAdaptDays();
		foreach (Pawn pawn in pawns)
		{
			if (pawn.IsColonist)
			{
				pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.Initial);
			}
		}
	}

	private static void SetPawnThoughts(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			MemoryThoughtHandler memoryThoughtHandler = pawn.needs?.mood?.thoughts?.memories;
			if (memoryThoughtHandler != null)
			{
				memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyOptimism);
				memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyHope);
				if (pawn.IsFreeNonSlaveColonist)
				{
					memoryThoughtHandler.TryGainMemory(ThoughtDefOf.NewColonyOptimism);
				}
			}
		}
	}

	private static void ResetNeedLevels(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			pawn.needs?.food?.SetInitialLevel();
			pawn.needs?.rest?.SetInitialLevel();
		}
	}

	private static void RemoveHediffs(List<Pawn> pawns)
	{
		List<Hediff> list = new List<Hediff>();
		foreach (Pawn pawn in pawns)
		{
			list.Clear();
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (hediff.def == HediffDefOf.Hypothermia || hediff.def == HediffDefOf.Heatstroke || hediff.def == HediffDefOf.Malnutrition)
				{
					list.Add(hediff);
				}
			}
			foreach (Hediff item in list)
			{
				pawn.health.RemoveHediff(item);
			}
		}
	}
}
