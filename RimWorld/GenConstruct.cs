using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class GenConstruct
{
	public const float ConstructionSpeedGlobalFactor = 1.7f;

	private static string SkillTooLowTrans;

	private static string IncapableOfDeconstruction;

	private static string IncapableOfMining;

	private static string TreeMarkedForExtraction;

	private static readonly List<string> tmpIdeoMemberNames = new List<string>();

	public static void Reset()
	{
		SkillTooLowTrans = "SkillTooLowForConstruction".Translate();
		IncapableOfDeconstruction = "IncapableOfDeconstruction".Translate();
		IncapableOfMining = "IncapableOfMining".Translate();
		TreeMarkedForExtraction = "TreeMarkedForExtraction".Translate();
	}

	public static Blueprint_Build PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff, Precept_ThingStyle styleSource = null, ThingStyleDef styleDef = null, bool sendBPSpawnedSignal = true)
	{
		Blueprint_Build blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(sourceDef.blueprintDef);
		blueprint_Build.SetFactionDirect(faction);
		blueprint_Build.stuffToUse = stuff;
		blueprint_Build.InheritStyle(styleSource, styleDef);
		GenSpawn.Spawn(blueprint_Build, center, map, rotation);
		if (faction != null && sendBPSpawnedSignal)
		{
			QuestUtility.SendQuestTargetSignals(faction.questTags, "PlacedBlueprint", blueprint_Build.Named("SUBJECT"));
		}
		return blueprint_Build;
	}

	public static Blueprint_Install PlaceBlueprintForInstall(MinifiedThing itemToInstall, IntVec3 center, Map map, Rot4 rotation, Faction faction, bool sendBPSpawnedSignal = true)
	{
		Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(itemToInstall.InnerThing.def.installBlueprintDef);
		blueprint_Install.SetThingToInstallFromMinified(itemToInstall);
		blueprint_Install.SetFactionDirect(faction);
		GenSpawn.Spawn(blueprint_Install, center, map, rotation);
		if (faction != null && sendBPSpawnedSignal)
		{
			QuestUtility.SendQuestTargetSignals(faction.questTags, "PlacedBlueprint", blueprint_Install.Named("SUBJECT"));
		}
		return blueprint_Install;
	}

	public static Blueprint_Install PlaceBlueprintForReinstall(Building buildingToReinstall, IntVec3 center, Map map, Rot4 rotation, Faction faction, bool sendBPSpawnedSignal = true)
	{
		Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(buildingToReinstall.def.installBlueprintDef);
		blueprint_Install.SetBuildingToReinstall(buildingToReinstall);
		blueprint_Install.SetFactionDirect(faction);
		GenSpawn.Spawn(blueprint_Install, center, map, rotation);
		if (faction != null && sendBPSpawnedSignal)
		{
			QuestUtility.SendQuestTargetSignals(faction.questTags, "PlacedBlueprint", blueprint_Install.Named("SUBJECT"));
		}
		return blueprint_Install;
	}

	public static bool CanBuildOnTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null, ThingDef stuffDef = null)
	{
		if (entDef is TerrainDef && !c.GetTerrain(map).changeable)
		{
			return false;
		}
		TerrainAffordanceDef terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
		if (terrainAffordanceNeed != null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
			cellRect.ClipInsideMap(map);
			foreach (IntVec3 item in cellRect)
			{
				TerrainDef terrainDef = map.terrainGrid.TerrainAt(item);
				if (!item.GetAffordances(map).Contains(terrainAffordanceNeed))
				{
					return false;
				}
				if (entDef is ThingDef thingDef && thingDef.building.crater && terrainDef.preventCraters)
				{
					return false;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] != thingToIgnore && thingList[i].def.entityDefToBuild is TerrainDef terrainDef2 && !terrainDef2.affordances.Contains(terrainAffordanceNeed))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static Thing MiniToInstallOrBuildingToReinstall(Blueprint b)
	{
		if (b is Blueprint_Install blueprint_Install)
		{
			return blueprint_Install.MiniToInstallOrBuildingToReinstall;
		}
		return null;
	}

	public static bool CanConstruct(Thing t, Pawn pawn, WorkTypeDef workType, bool forced = false, JobDef jobForReservation = null)
	{
		if (!forced && !pawn.workSettings.WorkIsActive(workType))
		{
			JobFailReason.Is("NotAssignedToWorkType".Translate(workType.gerundLabel).CapitalizeFirst());
			return false;
		}
		return CanConstruct(t, pawn, workType == WorkTypeDefOf.Construction, forced, jobForReservation);
	}

	public static bool CanConstruct(Thing t, Pawn p, bool checkSkills = true, bool forced = false, JobDef jobForReservation = null)
	{
		tmpIdeoMemberNames.Clear();
		if (FirstBlockingThing(t, p) != null)
		{
			return false;
		}
		if (!CanTouchTargetFromValidCell(t, p))
		{
			return false;
		}
		if (jobForReservation != null)
		{
			if (!p.Spawned)
			{
				return false;
			}
			if (!p.Map.reservationManager.OnlyReservationsForJobDef(t, jobForReservation))
			{
				Pawn pawn = p.Map.reservationManager.FirstRespectedReserver(t, p);
				if (pawn != null)
				{
					JobFailReason.Is("ReservedBy".Translate(pawn.LabelShort, pawn));
				}
				return false;
			}
			if (!p.CanReach(t, PathEndMode.Touch, forced ? Danger.Deadly : p.NormalMaxDanger()))
			{
				JobFailReason.Is("NoPath".Translate());
				return false;
			}
		}
		else if (!p.CanReserveAndReach(t, PathEndMode.Touch, forced ? Danger.Deadly : p.NormalMaxDanger(), 1, -1, null, forced))
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (checkSkills)
		{
			if (p.skills != null)
			{
				if (p.skills.GetSkill(SkillDefOf.Construction).Level < t.def.constructionSkillPrerequisite)
				{
					JobFailReason.Is(SkillTooLowTrans.Formatted(SkillDefOf.Construction.LabelCap));
					return false;
				}
				if (p.skills.GetSkill(SkillDefOf.Artistic).Level < t.def.artisticSkillPrerequisite)
				{
					JobFailReason.Is(SkillTooLowTrans.Formatted(SkillDefOf.Artistic.LabelCap));
					return false;
				}
			}
			if (p.IsColonyMech)
			{
				if (p.RaceProps.mechFixedSkillLevel < t.def.constructionSkillPrerequisite)
				{
					JobFailReason.Is(SkillTooLowTrans.Formatted(SkillDefOf.Construction.LabelCap));
					return false;
				}
				if (p.RaceProps.mechFixedSkillLevel < t.def.artisticSkillPrerequisite)
				{
					JobFailReason.Is(SkillTooLowTrans.Formatted(SkillDefOf.Artistic.LabelCap));
					return false;
				}
			}
		}
		bool flag = t is Blueprint_Install;
		if (p.Ideo != null && !p.Ideo.MembersCanBuild(t) && !flag)
		{
			foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
			{
				if (item.MembersCanBuild(t))
				{
					tmpIdeoMemberNames.Add(item.memberName);
				}
			}
			if (tmpIdeoMemberNames.Any())
			{
				JobFailReason.Is("OnlyMembersCanBuild".Translate(tmpIdeoMemberNames.ToCommaList(useAnd: true)));
			}
			return false;
		}
		if ((t.def.IsBlueprint || t.def.IsFrame) && t.def.entityDefToBuild is ThingDef thingDef)
		{
			if (thingDef.building != null && thingDef.building.isAttachment)
			{
				Thing wallAttachedTo = GetWallAttachedTo(t);
				if (wallAttachedTo == null || wallAttachedTo.def.IsBlueprint || wallAttachedTo.def.IsFrame)
				{
					return false;
				}
			}
			NamedArgument arg = p.Named(HistoryEventArgsNames.Doer);
			if (!new HistoryEvent(HistoryEventDefOf.BuildSpecificDef, arg, NamedArgumentUtility.Named(thingDef, HistoryEventArgsNames.Building)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
			if (thingDef.building != null && thingDef.building.IsTurret && !thingDef.HasComp(typeof(CompMannable)) && !new HistoryEvent(HistoryEventDefOf.BuiltAutomatedTurret, arg).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
		}
		return true;
	}

	public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null, Thing thing = null, ThingDef stuffDef = null, bool ignoreEdgeArea = false, bool ignoreInteractionSpots = false, bool ignoreClearableFreeBuildings = false)
	{
		CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
		if (stuffDef == null && thing != null)
		{
			stuffDef = thing.Stuff;
		}
		foreach (IntVec3 item in cellRect)
		{
			if (!item.InBounds(map))
			{
				return new AcceptanceReport("OutOfBounds".Translate());
			}
			if (item.InNoBuildEdgeArea(map) && !godMode && !ignoreEdgeArea)
			{
				return "TooCloseToMapEdge".Translate();
			}
		}
		if (center.Fogged(map))
		{
			return "CannotPlaceInUndiscovered".Translate();
		}
		List<Thing> thingList = center.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing2 = thingList[i];
			if (thing2 == thingToIgnore || !(thing2.Position == center) || !(thing2.Rotation == rot))
			{
				continue;
			}
			BuildableDef def = thing2.def;
			if (def != null && CanReplace(def, entDef, thing2.Stuff, stuffDef))
			{
				continue;
			}
			if (thing2.def == entDef)
			{
				return new AcceptanceReport("IdenticalThingExists".Translate());
			}
			if (thing2.def.entityDefToBuild == entDef)
			{
				if (thing2 is Blueprint)
				{
					return new AcceptanceReport("IdenticalBlueprintExists".Translate());
				}
				return new AcceptanceReport("IdenticalThingExists".Translate());
			}
		}
		if (entDef is ThingDef { HasSingleOrMultipleInteractionCells: not false } thingDef)
		{
			AcceptanceReport result = InteractionCellStandable(thingDef, center, rot, map, thingToIgnore);
			if (!result.Accepted)
			{
				return result;
			}
		}
		if (!ignoreInteractionSpots)
		{
			AcceptanceReport result2 = NotBlockingAnyInteractionCells(entDef, center, rot, map, thingToIgnore);
			if (!result2.Accepted)
			{
				return result2;
			}
		}
		if (entDef is TerrainDef terrainDef)
		{
			if (terrainDef.isFoundation && map.terrainGrid.FoundationAt(center) != null)
			{
				return new AcceptanceReport("FoundationAlreadyExists".Translate());
			}
			if (map.terrainGrid.TerrainAt(center) == terrainDef)
			{
				return new AcceptanceReport("TerrainIsAlready".Translate(terrainDef.label));
			}
			TerrainDef terrainDef2 = map.terrainGrid.TempTerrainAt(center);
			if (terrainDef2 != null && terrainDef2.Removable && (!terrainDef.bridge || !terrainDef2.tempTerrain.replaceableByBridge))
			{
				return new AcceptanceReport("TerrainUnremovable".Translate());
			}
			if (map.designationManager.DesignationAt(center, DesignationDefOf.SmoothFloor) != null)
			{
				return new AcceptanceReport("SpaceBeingSmoothed".Translate());
			}
		}
		if (!CanBuildOnTerrain(entDef, center, map, rot, thingToIgnore, stuffDef))
		{
			if (entDef.GetTerrainAffordanceNeed(stuffDef) != null)
			{
				if (entDef.useStuffTerrainAffordance && stuffDef != null)
				{
					return new AcceptanceReport("TerrainCannotSupport_TerrainAffordanceFromStuff".Translate(entDef, entDef.GetTerrainAffordanceNeed(stuffDef), stuffDef).CapitalizeFirst());
				}
				return new AcceptanceReport("TerrainCannotSupport_TerrainAffordance".Translate(entDef, entDef.GetTerrainAffordanceNeed(stuffDef)).CapitalizeFirst());
			}
			return new AcceptanceReport("TerrainCannotSupport".Translate(entDef).CapitalizeFirst());
		}
		if (ModsConfig.RoyaltyActive)
		{
			List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.MonumentMarker);
			for (int j = 0; j < list.Count; j++)
			{
				MonumentMarker monumentMarker = (MonumentMarker)list[j];
				if (!monumentMarker.complete && !monumentMarker.AllowsPlacingBlueprint(entDef, center, rot, stuffDef))
				{
					return new AcceptanceReport("BlueprintWouldCollideWithMonument".Translate());
				}
			}
		}
		if (!godMode)
		{
			foreach (IntVec3 item2 in cellRect)
			{
				thingList = item2.GetThingList(map);
				for (int k = 0; k < thingList.Count; k++)
				{
					Thing thing3 = thingList[k];
					if (thing3 == thingToIgnore || (thing3 is Building building && building.IsClearableFreeBuilding && ignoreClearableFreeBuildings))
					{
						continue;
					}
					if (entDef is TerrainDef terrainDef3 && thing3.def.category == ThingCategory.Building && thing3.def.terrainAffordanceNeeded != null && !terrainDef3.affordances.Contains(thing3.def.terrainAffordanceNeeded))
					{
						TerrainDef terrainDef4 = map.terrainGrid.FoundationAt(item2);
						if (terrainDef4 != null && terrainDef4.affordances.Contains(thing3.def.terrainAffordanceNeeded))
						{
							continue;
						}
					}
					if (!CanPlaceBlueprintOver(entDef, thing3.def, stuffDef, thing3.Stuff))
					{
						return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
					}
				}
			}
		}
		if (entDef.PlaceWorkers != null)
		{
			for (int l = 0; l < entDef.PlaceWorkers.Count; l++)
			{
				AcceptanceReport result3 = entDef.PlaceWorkers[l].AllowsPlacing(entDef, center, rot, map, thingToIgnore, thing);
				if (!result3.Accepted)
				{
					return result3;
				}
			}
		}
		return AcceptanceReport.WasAccepted;
	}

	public static bool CanReplace(BuildableDef placing, BuildableDef existing, ThingDef placingStuff = null, ThingDef existingStuff = null)
	{
		ThingDef thingDef = placing as ThingDef;
		ThingDef thingDef2 = existing as ThingDef;
		if (thingDef == null || thingDef2 == null)
		{
			return false;
		}
		BuildableDef buildableDef = thingDef.entityDefToBuild ?? thingDef;
		BuildableDef buildableDef2 = thingDef2.entityDefToBuild ?? thingDef2;
		if (buildableDef == buildableDef2 && (!buildableDef.MadeFromStuff || placingStuff == existingStuff))
		{
			return false;
		}
		return HasMatchingReplacementTag(thingDef, thingDef2);
	}

	public static bool HasMatchingReplacementTag(ThingDef a, ThingDef b)
	{
		if (a.replaceTags == null || b.replaceTags == null)
		{
			return false;
		}
		foreach (string replaceTag in a.replaceTags)
		{
			if (b.replaceTags.Contains(replaceTag))
			{
				return true;
			}
		}
		return false;
	}

	public static AcceptanceReport InteractionCellStandable(ThingDef thingDef, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
	{
		foreach (IntVec3 item in ThingUtility.InteractionCellsWhenAt(thingDef, center, rot, map))
		{
			if (!item.InBounds(map))
			{
				return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
			}
			List<Thing> list = map.thingGrid.ThingsListAtFast(item);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != thingToIgnore)
				{
					if (list[i].def.passability != Traversability.Standable || list[i].def == thingDef)
					{
						return new AcceptanceReport("InteractionSpotBlocked".Translate(list[i].LabelNoCount, list[i]).CapitalizeFirst());
					}
					BuildableDef entityDefToBuild = list[i].def.entityDefToBuild;
					if (entityDefToBuild != null && (entityDefToBuild.passability != Traversability.Standable || entityDefToBuild == thingDef))
					{
						return new AcceptanceReport("InteractionSpotWillBeBlocked".Translate(list[i].LabelNoCount, list[i]).CapitalizeFirst());
					}
				}
			}
		}
		return true;
	}

	public static AcceptanceReport NotBlockingAnyInteractionCells(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
	{
		CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
		foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(center, rot, entDef.Size))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing == thingToIgnore)
				{
					continue;
				}
				ThingDef thingDef = null;
				if (thing is Blueprint blueprint)
				{
					if (!(blueprint.def.entityDefToBuild is ThingDef thingDef2))
					{
						continue;
					}
					thingDef = thingDef2;
				}
				else if (thing is Frame frame)
				{
					if (!(frame.def.entityDefToBuild is ThingDef thingDef3))
					{
						continue;
					}
					thingDef = thingDef3;
				}
				else
				{
					thingDef = thing.def;
				}
				if (!thingDef.HasSingleOrMultipleInteractionCells || (entDef.passability == Traversability.Standable && entDef != thingDef))
				{
					continue;
				}
				foreach (IntVec3 item2 in ThingUtility.InteractionCellsWhenAt(thingDef, thing.Position, thing.Rotation, thing.Map))
				{
					if (cellRect.Contains(item2))
					{
						return new AcceptanceReport("WouldBlockInteractionSpot".Translate(entDef.label, thingDef.label).CapitalizeFirst());
					}
				}
			}
		}
		return true;
	}

	public static BuildableDef BuiltDefOf(ThingDef def)
	{
		return def.entityDefToBuild ?? def;
	}

	public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef, ThingDef newStuff = null, ThingDef oldStuff = null)
	{
		if (oldDef.EverHaulable)
		{
			return true;
		}
		if (newDef is TerrainDef terrainDef)
		{
			if (oldDef.IsBlueprint || oldDef.IsFrame)
			{
				if (oldDef.entityDefToBuild.terrainAffordanceNeeded != null && !terrainDef.affordances.Contains(oldDef.entityDefToBuild.terrainAffordanceNeeded))
				{
					return false;
				}
			}
			else if (oldDef.category == ThingCategory.Building && oldDef.terrainAffordanceNeeded != null && !terrainDef.affordances.Contains(oldDef.terrainAffordanceNeeded))
			{
				return false;
			}
		}
		ThingDef newThingDef = newDef as ThingDef;
		BuildableDef oldDefBuilt = BuiltDefOf(oldDef);
		ThingDef thingDef = oldDefBuilt as ThingDef;
		if (newDef.blocksAltitudes != null && newDef.blocksAltitudes.Contains(oldDef.altitudeLayer))
		{
			return false;
		}
		if (oldDefBuilt?.blocksAltitudes != null && oldDefBuilt.blocksAltitudes.Contains(newDef.altitudeLayer))
		{
			return false;
		}
		if (CanReplace(newThingDef, oldDef, newStuff, oldStuff))
		{
			return true;
		}
		if (newDef.ForceAllowPlaceOver(oldDef))
		{
			return true;
		}
		if (oldDef.category == ThingCategory.Plant && oldDef.passability == Traversability.Impassable && newThingDef != null && newThingDef.category == ThingCategory.Building && !newThingDef.building.canPlaceOverImpassablePlant)
		{
			return false;
		}
		if (oldDef.category == ThingCategory.Building || oldDef.IsBlueprint || oldDef.IsFrame)
		{
			if (newThingDef != null)
			{
				if (!newThingDef.IsEdifice())
				{
					if (oldDef.building != null && !oldDef.building.canBuildNonEdificesUnder)
					{
						return false;
					}
					if (newThingDef.EverTransmitsPower && oldDef.EverTransmitsPower)
					{
						return false;
					}
					return true;
				}
				if (IsEdificeOverNonEdifice())
				{
					if (newThingDef.building != null && !newThingDef.building.canBuildNonEdificesUnder)
					{
						return false;
					}
					if (oldDefBuilt is ThingDef thingDef2 && thingDef2.building.isAttachment && newThingDef.Fillage == FillCategory.Full)
					{
						return false;
					}
					return true;
				}
				if (thingDef?.building != null && (thingDef.building.isPlaceOverableWall || thingDef.IsSmoothed) && newThingDef.building != null && newThingDef.building.canPlaceOverWall)
				{
					return true;
				}
				BuildingProperties building = newThingDef.building;
				if ((building == null || !building.isPowerConduit) && thingDef != null && thingDef.building?.isPowerConduit == true)
				{
					return true;
				}
			}
			if (newDef is TerrainDef && oldDefBuilt is ThingDef { CoexistsWithFloors: not false })
			{
				return true;
			}
			if (oldDefBuilt is TerrainDef && !(newDef is TerrainDef))
			{
				return true;
			}
			return false;
		}
		return true;
		bool IsEdificeOverNonEdifice()
		{
			if (!newThingDef.IsEdifice())
			{
				return false;
			}
			if (oldDef != null && oldDef.category == ThingCategory.Building && !oldDef.IsEdifice())
			{
				return true;
			}
			if (oldDefBuilt is ThingDef { category: ThingCategory.Building } thingDef4 && !thingDef4.IsEdifice())
			{
				return true;
			}
			return false;
		}
	}

	public static Thing FirstBlockingThing(Thing constructible, Pawn pawnToIgnore)
	{
		Thing thing = ((!(constructible is Blueprint b)) ? null : MiniToInstallOrBuildingToReinstall(b));
		foreach (IntVec3 item in constructible.OccupiedRect())
		{
			List<Thing> thingList = item.GetThingList(constructible.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing2 = thingList[i];
				if (BlocksConstruction(constructible, thing2) && thing2 != pawnToIgnore && thing2 != thing)
				{
					return thing2;
				}
			}
		}
		return null;
	}

	public static bool CanTouchTargetFromValidCell(Thing constructible, Pawn worker)
	{
		IntVec3 result;
		return RCellFinder.TryFindGoodAdjacentSpotToTouch(worker, constructible, out result);
	}

	public static Job HandleBlockingThingJob(Thing constructible, Pawn worker, bool forced = false)
	{
		Thing thing = FirstBlockingThing(constructible, worker);
		if (thing == null)
		{
			return null;
		}
		if (thing.def.category == ThingCategory.Plant)
		{
			if (!PlantUtility.PawnWillingToCutPlant_Job(thing, worker))
			{
				return null;
			}
			if (PlantUtility.TreeMarkedForExtraction(thing))
			{
				JobFailReason.Is(TreeMarkedForExtraction);
				return null;
			}
			if (worker.CanReserveAndReach(thing, PathEndMode.ClosestTouch, worker.NormalMaxDanger(), 1, -1, null, forced))
			{
				return JobMaker.MakeJob(JobDefOf.CutPlant, thing);
			}
		}
		else if (thing.def.category == ThingCategory.Item)
		{
			if (thing.def.EverHaulable)
			{
				return HaulAIUtility.HaulAsideJobFor(worker, thing);
			}
			Log.ErrorOnce("Never haulable " + thing?.ToString() + " blocking " + constructible.ToStringSafe() + " at " + constructible.Position.ToString(), 6429262);
		}
		else if (thing.def.category == ThingCategory.Building)
		{
			if ((bool)((Building)thing).DeconstructibleBy(worker.Faction))
			{
				if (worker.WorkTypeIsDisabled(WorkTypeDefOf.Construction) || (worker.workSettings != null && !worker.workSettings.WorkIsActive(WorkTypeDefOf.Construction)))
				{
					JobFailReason.Is(IncapableOfDeconstruction);
					return null;
				}
				if (!forced && thing.IsForbidden(worker))
				{
					return null;
				}
				if (worker.CanReserveAndReach(thing, PathEndMode.Touch, worker.NormalMaxDanger(), 1, -1, null, forced))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Deconstruct, thing);
					job.ignoreDesignations = true;
					return job;
				}
			}
			if (thing.def.mineable)
			{
				if (worker.WorkTypeIsDisabled(WorkTypeDefOf.Mining) || (worker.workSettings != null && !worker.workSettings.WorkIsActive(WorkTypeDefOf.Mining)))
				{
					JobFailReason.Is(IncapableOfMining);
					return null;
				}
				if (worker.CanReserveAndReach(thing, PathEndMode.Touch, worker.NormalMaxDanger(), 1, -1, null, forced))
				{
					Job job2 = JobMaker.MakeJob(JobDefOf.Mine, thing);
					job2.ignoreDesignations = true;
					return job2;
				}
			}
		}
		return null;
	}

	public static bool BlocksConstruction(Thing constructible, Thing t)
	{
		if (t == constructible)
		{
			return false;
		}
		ThingDef thingDef = BlueprintDefOf(constructible);
		if (t.def.category == ThingCategory.Building && GenSpawn.SpawningWipes(thingDef.entityDefToBuild, t.def))
		{
			return true;
		}
		if (t.def.category == ThingCategory.Plant)
		{
			if (t.def.plant.harvestWork > ThingDefOf.Plant_Dandelion.plant.harvestWork)
			{
				return !(thingDef.entityDefToBuild is TerrainDef) || !t.Spawned || !(t.Position.GetEdifice(t.Map) is IPlantToGrowSettable);
			}
			return false;
		}
		if (!thingDef.clearBuildingArea)
		{
			return false;
		}
		if (thingDef.entityDefToBuild.ForceAllowPlaceOver(t.def))
		{
			return false;
		}
		ThingDef thingDef2 = thingDef.entityDefToBuild as ThingDef;
		if (thingDef2 != null)
		{
			if (thingDef2.EverTransmitsPower)
			{
				BuildingProperties building = t.def.building;
				if (building != null && building.isPowerConduit)
				{
					BuildingProperties building2 = thingDef2.building;
					if (building2 != null && !building2.isPowerConduit)
					{
						return false;
					}
				}
			}
			if (t.def == ThingDefOf.Wall && thingDef2.building != null && thingDef2.building.canPlaceOverWall)
			{
				return false;
			}
			if (t.def.Fillage != FillCategory.Full && thingDef2.building != null && thingDef2.building.isAttachment)
			{
				return false;
			}
			if (t.def.category == ThingCategory.Item && thingDef2.passability != Traversability.Standable && thingDef2.surfaceType == SurfaceType.None)
			{
				return true;
			}
		}
		if (t.def.IsEdifice() && thingDef2.IsEdifice())
		{
			return true;
		}
		bool flag = thingDef.entityDefToBuild.passability == Traversability.Impassable || thingDef.forceMoveItemsBeforeConstruction;
		if (t.def.category == ThingCategory.Item && flag)
		{
			return true;
		}
		if (t is Pawn pawn && !pawn.IsHiddenFromPlayer())
		{
			return true;
		}
		if ((int)t.def.Fillage >= 1 && (t.def.IsEdifice() || (t.def.entityDefToBuild != null && t.def.entityDefToBuild.IsEdifice())))
		{
			if (thingDef2.blocksAltitudes == null)
			{
				return true;
			}
			return (t.def.blocksAltitudes ?? t.def.entityDefToBuild?.blocksAltitudes)?.SharesElementWith(thingDef2.blocksAltitudes) ?? false;
		}
		return false;
	}

	private static ThingDef BlueprintDefOf(Thing constructible)
	{
		if (constructible is Blueprint)
		{
			return constructible.def;
		}
		if (constructible is Frame)
		{
			return constructible.def.entityDefToBuild.blueprintDef;
		}
		return constructible.def.blueprintDef;
	}

	public static bool TerrainCanSupport(CellRect rect, Map map, ThingDef thing)
	{
		foreach (IntVec3 item in rect)
		{
			if (!item.SupportsStructureType(map, thing.terrainAffordanceNeeded))
			{
				return false;
			}
		}
		return true;
	}

	public static List<Thing> GetAttachedBuildings(Thing thing)
	{
		List<Thing> list = new List<Thing>();
		if (thing == null)
		{
			return list;
		}
		ThingDef thingDef = BuiltDefOf(thing.def) as ThingDef;
		if (thingDef?.building == null || !thingDef.building.supportsWallAttachments)
		{
			return list;
		}
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = thing.Position + GenAdj.CardinalDirections[i];
			if (!c.InBounds(thing.Map))
			{
				continue;
			}
			foreach (Thing thing2 in c.GetThingList(thing.Map))
			{
				ThingDef thingDef2 = BuiltDefOf(thing2.def) as ThingDef;
				if (thingDef2?.building != null && thingDef2.building.isAttachment && GenMath.PositiveMod(thing2.Rotation.AsInt - 2, 4) == i)
				{
					list.Add(thing2);
				}
			}
		}
		return list;
	}

	public static Thing GetWallAttachedTo(Thing thing)
	{
		if (thing == null)
		{
			return null;
		}
		ThingDef thingDef = BuiltDefOf(thing.def) as ThingDef;
		if (thingDef?.building == null || !thingDef.building.isAttachment)
		{
			return null;
		}
		return GetWallAttachedTo(thing.Position, thing.Rotation, thing.Map);
	}

	public static Thing GetWallAttachedTo(IntVec3 pos, Rot4 rot, Map map)
	{
		IntVec3 c = pos + GenAdj.CardinalDirections[rot.AsInt];
		if (!c.InBounds(map))
		{
			return null;
		}
		foreach (Thing thing in c.GetThingList(map))
		{
			if (BuiltDefOf(thing.def) is ThingDef { building: not null } thingDef && thingDef.building.supportsWallAttachments)
			{
				return thing;
			}
		}
		return null;
	}
}
