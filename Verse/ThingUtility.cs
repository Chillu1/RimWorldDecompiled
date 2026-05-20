using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class ThingUtility
{
	private static List<IntVec3> tmpInteractionCells = new List<IntVec3>();

	public static bool DestroyedOrNull(this Thing t)
	{
		return t?.Destroyed ?? true;
	}

	public static void DestroyOrPassToWorld(this Thing t, DestroyMode mode = DestroyMode.Vanish)
	{
		if (t is Pawn pawn)
		{
			if (!Find.WorldPawns.Contains(pawn))
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
		}
		else
		{
			t.Destroy(mode);
		}
	}

	public static int TryAbsorbStackNumToTake(Thing thing, Thing other, bool respectStackLimit)
	{
		if (respectStackLimit)
		{
			return Mathf.Min(other.stackCount, thing.def.stackLimit - thing.stackCount);
		}
		return other.stackCount;
	}

	public static int RoundedResourceStackCount(int stackCount)
	{
		if (stackCount > 200)
		{
			return GenMath.RoundTo(stackCount, 10);
		}
		if (stackCount > 100)
		{
			return GenMath.RoundTo(stackCount, 5);
		}
		return stackCount;
	}

	public static IntVec3 InteractionCell(IntVec3 interactionOffset, IntVec3 thingCenter, Rot4 rot)
	{
		return interactionOffset.RotatedBy(rot) + thingCenter;
	}

	public static List<IntVec3> InteractionCellsWhenAt(ThingDef def, IntVec3 center, Rot4 rot, Map map, bool allowFallbackCell = false)
	{
		InteractionCellsWhenAt(tmpInteractionCells, def, center, rot, map, allowFallbackCell);
		return tmpInteractionCells;
	}

	public static void InteractionCellsWhenAt(List<IntVec3> listToPopulate, ThingDef def, IntVec3 center, Rot4 rot, Map map, bool allowFallbackCell = false)
	{
		listToPopulate.Clear();
		if (!def.multipleInteractionCellOffsets.NullOrEmpty())
		{
			foreach (IntVec3 multipleInteractionCellOffset in def.multipleInteractionCellOffsets)
			{
				listToPopulate.Add(InteractionCell(multipleInteractionCellOffset, center, rot));
			}
			return;
		}
		if (def.hasInteractionCell || allowFallbackCell)
		{
			listToPopulate.Add(InteractionCellWhenAt(def, center, rot, map));
		}
	}

	public static IntVec3 InteractionCellWhenAt(ThingDef def, IntVec3 center, Rot4 rot, Map map)
	{
		if (def.hasInteractionCell)
		{
			return InteractionCell(def.interactionCellOffset, center, rot);
		}
		if (def.Size.x == 1 && def.Size.z == 1)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = center + GenAdj.AdjacentCells[i];
				if (intVec.Standable(map) && intVec.GetDoor(map) == null && ReachabilityImmediate.CanReachImmediate(intVec, center, map, PathEndMode.Touch, null))
				{
					return intVec;
				}
			}
			for (int j = 0; j < 8; j++)
			{
				IntVec3 intVec2 = center + GenAdj.AdjacentCells[j];
				if (intVec2.Standable(map) && ReachabilityImmediate.CanReachImmediate(intVec2, center, map, PathEndMode.Touch, null))
				{
					return intVec2;
				}
			}
			for (int k = 0; k < 8; k++)
			{
				IntVec3 intVec3 = center + GenAdj.AdjacentCells[k];
				if (intVec3.Walkable(map) && ReachabilityImmediate.CanReachImmediate(intVec3, center, map, PathEndMode.Touch, null))
				{
					return intVec3;
				}
			}
			return center;
		}
		List<IntVec3> list = GenAdjFast.AdjacentCells8Way(center, rot, def.size);
		CellRect rect = GenAdj.OccupiedRect(center, rot, def.size);
		for (int l = 0; l < list.Count; l++)
		{
			if (list[l].Standable(map) && list[l].GetDoor(map) == null && ReachabilityImmediate.CanReachImmediate(list[l], rect, map, PathEndMode.Touch, null))
			{
				return list[l];
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			if (list[m].Standable(map) && ReachabilityImmediate.CanReachImmediate(list[m], rect, map, PathEndMode.Touch, null))
			{
				return list[m];
			}
		}
		for (int n = 0; n < list.Count; n++)
		{
			if (list[n].Walkable(map) && ReachabilityImmediate.CanReachImmediate(list[n], rect, map, PathEndMode.Touch, null))
			{
				return list[n];
			}
		}
		return center;
	}

	public static DamageDef PrimaryMeleeWeaponDamageType(ThingDef thing)
	{
		return PrimaryMeleeWeaponDamageType(thing.tools);
	}

	public static DamageDef PrimaryMeleeWeaponDamageType(List<Tool> tools)
	{
		if (tools.NullOrEmpty())
		{
			return null;
		}
		return tools.MaxBy((Tool tool) => tool.power).Maneuvers.FirstOrDefault()?.verb.meleeDamageDef;
	}

	public static Blueprint_Build CheckAutoRebuildOnDestroyed(Thing thing, DestroyMode mode, Map map, BuildableDef buildingDef)
	{
		if (Find.PlaySettings.autoRebuild && mode == DestroyMode.KillFinalize && thing.Faction == Faction.OfPlayer && buildingDef.blueprintDef != null && buildingDef.IsResearchFinished && map.areaManager.Home[thing.Position] && GenConstruct.CanPlaceBlueprintAt(buildingDef, thing.Position, thing.Rotation, map, godMode: false, null, null, thing.Stuff).Accepted)
		{
			return GenConstruct.PlaceBlueprintForBuild(buildingDef, thing.Position, map, thing.Rotation, Faction.OfPlayer, thing.Stuff, thing.StyleSourcePrecept, thing.StyleDef);
		}
		return null;
	}

	public static void CheckAutoRebuildTerrainOnDestroyed(TerrainDef terrainDef, IntVec3 pos, Map map)
	{
		if (Find.PlaySettings.autoRebuild && terrainDef.autoRebuildable && terrainDef.blueprintDef != null && terrainDef.IsResearchFinished && map.areaManager.Home[pos] && GenConstruct.CanPlaceBlueprintAt(terrainDef, pos, Rot4.South, map).Accepted)
		{
			GenConstruct.PlaceBlueprintForBuild(terrainDef, pos, map, Rot4.South, Faction.OfPlayer, null);
		}
	}

	public static Pawn FindPawn(List<Thing> things)
	{
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i] is Pawn result)
			{
				return result;
			}
			if (things[i] is Corpse corpse)
			{
				return corpse.InnerPawn;
			}
		}
		return null;
	}

	public static TerrainAffordanceDef GetTerrainAffordanceNeed(this BuildableDef def, ThingDef stuffDef = null)
	{
		TerrainAffordanceDef terrainAffordanceNeeded = def.terrainAffordanceNeeded;
		if (stuffDef != null && def.useStuffTerrainAffordance && stuffDef.terrainAffordanceNeeded != null)
		{
			terrainAffordanceNeeded = stuffDef.terrainAffordanceNeeded;
		}
		return terrainAffordanceNeeded;
	}

	public static bool HasThingCategory(this Thing thing, ThingCategoryDef thingCategory)
	{
		if (thing.def.thingCategories.NullOrEmpty())
		{
			return false;
		}
		return thing.def.thingCategories.Contains(thingCategory);
	}

	public static int GetStackCountFromThingList(IEnumerable<Thing> things)
	{
		int num = 0;
		foreach (Thing thing in things)
		{
			num += thing.stackCount;
		}
		return num;
	}

	public static void FindAllOfType<T>(ThingRequest request, List<T> list) where T : Thing
	{
		list.Clear();
		List<Thing> list2 = Find.CurrentMap.listerThings.ThingsMatching(request);
		for (int i = 0; i < list2.Count; i++)
		{
			if (list2[i] is T item)
			{
				list.Add(item);
			}
		}
		List<Pawn> freeColonists = Find.CurrentMap.mapPawns.FreeColonists;
		for (int j = 0; j < freeColonists.Count; j++)
		{
			if (freeColonists[j].carryTracker.CarriedThing is T item2)
			{
				list.Add(item2);
			}
			freeColonists[j].inventory.innerContainer.GetThingsOfType(list);
		}
	}

	public static void FindAllOfType<T>(List<T> list) where T : Thing
	{
		list.Clear();
		Find.CurrentMap.listerThings.GetThingsOfType(list);
		List<Pawn> freeColonists = Find.CurrentMap.mapPawns.FreeColonists;
		for (int i = 0; i < freeColonists.Count; i++)
		{
			if (freeColonists[i].carryTracker.CarriedThing is T item)
			{
				list.Add(item);
			}
			freeColonists[i].inventory.innerContainer.GetThingsOfType(list);
		}
	}
}
