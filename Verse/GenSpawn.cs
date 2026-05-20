using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public static class GenSpawn
{
	private static readonly List<Thing> leavings = new List<Thing>();

	public static Thing Spawn(ThingDef def, IntVec3 loc, Map map, WipeMode wipeMode = WipeMode.Vanish)
	{
		return Spawn(def, loc, map, Rot4.North, wipeMode);
	}

	public static Thing Spawn(ThingDef def, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish)
	{
		if (!def.CanSpawnAt(loc, rot, map))
		{
			return null;
		}
		return Spawn(ThingMaker.MakeThing(def), loc, map, rot, wipeMode);
	}

	public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, WipeMode wipeMode = WipeMode.Vanish)
	{
		return Spawn(newThing, loc, map, Rot4.North, wipeMode);
	}

	public static bool TrySpawn(ThingDef def, IntVec3 loc, Map map, out Thing thing, WipeMode wipeMode = WipeMode.Vanish, bool canWipeEdifices = true)
	{
		bool canWipeEdifices2 = canWipeEdifices;
		if (!CanSpawnAt(def, loc, map, null, canWipeEdifices2))
		{
			thing = null;
			return false;
		}
		thing = Spawn(def, loc, map, wipeMode);
		return thing != null;
	}

	public static bool TrySpawn(ThingDef def, IntVec3 loc, Map map, Rot4 rot, out Thing thing, WipeMode wipeMode = WipeMode.Vanish, bool canWipeEdifices = true)
	{
		if (!CanSpawnAt(def, loc, map, rot, canWipeEdifices))
		{
			thing = null;
			return false;
		}
		thing = Spawn(def, loc, map, rot, wipeMode);
		return thing != null;
	}

	public static void SpawnIrregularLump(ThingDef thing, IntVec3 pos, Map map, IntRange countRange, IntRange distRange, WipeMode wipeMode = WipeMode.Vanish, Predicate<IntVec3> validator = null, List<IntVec3> area = null, List<Thing> spawned = null, ThingDef stuff = null, Faction faction = null)
	{
		int randomInRange = distRange.RandomInRange;
		int randomInRange2 = countRange.RandomInRange;
		int num = 0;
		List<IntVec3> list = GridShapeMaker.IrregularLump(pos, map, randomInRange, validator);
		for (int i = 0; i < list.Count; i++)
		{
			IntVec3 intVec = list[i];
			if (!Rand.DynamicChance(num, randomInRange2, list.Count - i))
			{
				continue;
			}
			num++;
			if (thing.IsFilth)
			{
				FilthMaker.TryMakeFilth(intVec, map, thing);
			}
			else if (CanSpawnAt(thing, intVec, map))
			{
				Thing thing2 = ThingMaker.MakeThing(thing, stuff);
				if (thing2.def.CanHaveFaction)
				{
					thing2.SetFactionDirect(faction);
				}
				Spawn(thing2, intVec, map, wipeMode);
				spawned?.Add(thing2);
			}
			area?.Add(intVec);
		}
	}

	public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false, bool forbidLeavings = false)
	{
		if (map == null)
		{
			Log.Error("Tried to spawn " + newThing.ToStringSafe() + " in a null map.");
			return null;
		}
		if (!loc.InBounds(map))
		{
			string[] obj = new string[5]
			{
				"Tried to spawn ",
				newThing.ToStringSafe(),
				" out of bounds at ",
				null,
				null
			};
			IntVec3 intVec = loc;
			obj[3] = intVec.ToString();
			obj[4] = ".";
			Log.Error(string.Concat(obj));
			return null;
		}
		if (newThing.def.randomizeRotationOnSpawn)
		{
			rot = Rot4.Random;
		}
		CellRect occupiedRect = GenAdj.OccupiedRect(loc, rot, newThing.def.Size);
		if (!occupiedRect.InBounds(map))
		{
			string[] obj2 = new string[7]
			{
				"Tried to spawn ",
				newThing.ToStringSafe(),
				" out of bounds at ",
				null,
				null,
				null,
				null
			};
			IntVec3 intVec = loc;
			obj2[3] = intVec.ToString();
			obj2[4] = " (out of bounds because size is ";
			obj2[5] = newThing.def.Size.ToString();
			obj2[6] = ").";
			Log.Error(string.Concat(obj2));
			return null;
		}
		if (newThing.Spawned)
		{
			Log.Error("Tried to spawn " + newThing?.ToString() + " but it's already spawned.");
			return newThing;
		}
		switch (wipeMode)
		{
		case WipeMode.Vanish:
			WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
			break;
		case WipeMode.FullRefund:
			WipeAndRefundExistingThings(loc, rot, newThing.def, map, forbidLeavings);
			break;
		case WipeMode.VanishOrMoveAside:
			CheckMoveItemsAside(loc, rot, newThing.def, map);
			WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
			break;
		}
		if (newThing.def.category == ThingCategory.Item && Current.ProgramState == ProgramState.Playing && loc.GetItemCount(map) >= loc.GetMaxItemsAllowedInCell(map))
		{
			foreach (Thing item in loc.GetThingList(map).ToList())
			{
				if (item != newThing && item.def.category == ThingCategory.Item)
				{
					item.DeSpawn();
					if (!GenPlace.TryPlaceThing(item, loc, map, ThingPlaceMode.Near, null, (IntVec3 x) => !occupiedRect.Contains(x)))
					{
						item.Destroy();
					}
					break;
				}
			}
		}
		newThing.Rotation = rot;
		newThing.Position = loc;
		if (newThing.holdingOwner != null)
		{
			newThing.holdingOwner.Remove(newThing);
		}
		newThing.SpawnSetup(map, respawningAfterLoad);
		if (newThing.Spawned && newThing.stackCount == 0)
		{
			Log.Error("Spawned thing with 0 stackCount: " + newThing);
			newThing.Destroy();
			return null;
		}
		if (newThing.def.passability == Traversability.Impassable)
		{
			foreach (IntVec3 item2 in occupiedRect)
			{
				foreach (Thing item3 in item2.GetThingList(map).ToList())
				{
					if (item3 != newThing && item3 is Pawn pawn)
					{
						pawn.pather.TryRecoverFromUnwalkablePosition(error: false);
					}
				}
			}
		}
		return newThing;
	}

	public static void SpawnBuildingAsPossible(Building building, Map map, bool respawningAfterLoad = false)
	{
		bool flag = false;
		if (!building.OccupiedRect().InBounds(map))
		{
			flag = true;
		}
		else
		{
			foreach (IntVec3 item in building.OccupiedRect())
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] is Pawn && building.def.passability == Traversability.Impassable)
					{
						flag = true;
						break;
					}
					if ((thingList[i].def.category == ThingCategory.Building || thingList[i].def.category == ThingCategory.Item) && SpawningWipes(building.def, thingList[i].def))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (flag)
		{
			Refund(building, map, CellRect.Empty);
		}
		else
		{
			Spawn(building, building.Position, map, building.Rotation, WipeMode.FullRefund, respawningAfterLoad);
		}
	}

	public static void Refund(Thing thing, Map map, CellRect avoidThisRect, bool forbid = false, bool willReplace = false)
	{
		bool flag = false;
		if (thing.def.Minifiable && !thing.def.IsPlant)
		{
			MinifiedThing minifiedThing = thing.MakeMinified(willReplace ? DestroyMode.WillReplace : DestroyMode.Vanish);
			if (GenPlace.TryPlaceThing(minifiedThing, thing.Position, map, ThingPlaceMode.Near, null, (IntVec3 x) => !avoidThisRect.Contains(x)))
			{
				flag = true;
				minifiedThing.SetForbidden(forbid);
			}
			else
			{
				minifiedThing.GetDirectlyHeldThings().Clear();
				minifiedThing.Destroy();
			}
		}
		if (flag)
		{
			return;
		}
		leavings.Clear();
		GenLeaving.DoLeavingsFor(thing, map, DestroyMode.Refund, thing.OccupiedRect(), (IntVec3 x) => !avoidThisRect.Contains(x), leavings);
		thing.Destroy(willReplace ? DestroyMode.WillReplace : DestroyMode.Vanish);
		foreach (Thing leaving in leavings)
		{
			leaving.SetForbidden(forbid);
		}
	}

	public static void WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, DestroyMode mode)
	{
		foreach (IntVec3 item in GenAdj.CellsOccupiedBy(thingPos, thingRot, thingDef.Size))
		{
			foreach (Thing item2 in map.thingGrid.ThingsAt(item).ToList())
			{
				if (SpawningWipes(thingDef, item2.def))
				{
					item2.Destroy(mode);
				}
			}
		}
	}

	public static void WipeAndRefundExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, bool forbid)
	{
		CellRect occupiedRect = GenAdj.OccupiedRect(thingPos, thingRot, thingDef.Size);
		foreach (IntVec3 item in occupiedRect)
		{
			foreach (Thing item2 in item.GetThingList(map).ToList())
			{
				if (!SpawningWipes(thingDef, item2.def))
				{
					continue;
				}
				if (item2.def.category == ThingCategory.Item)
				{
					item2.DeSpawn();
					if (!GenPlace.TryPlaceThing(item2, item, map, ThingPlaceMode.Near, null, (IntVec3 x) => !occupiedRect.Contains(x)))
					{
						item2.Destroy();
					}
					else
					{
						item2.SetForbidden(item2.IsForbidden(Faction.OfPlayer) || forbid, warnOnFail: false);
					}
				}
				else
				{
					Refund(item2, map, occupiedRect, forbid, thingDef.IsEdifice());
				}
			}
		}
	}

	public static void CheckMoveItemsAside(IntVec3 thingPos, Rot4 thingRot, ThingDef thingDef, Map map)
	{
		if (thingDef.surfaceType != SurfaceType.None || thingDef.passability == Traversability.Standable)
		{
			return;
		}
		CellRect occupiedRect = GenAdj.OccupiedRect(thingPos, thingRot, thingDef.Size);
		foreach (IntVec3 item in occupiedRect)
		{
			foreach (Thing item2 in item.GetThingList(map).ToList())
			{
				if (item2.def.category == ThingCategory.Item)
				{
					item2.DeSpawn();
					if (!GenPlace.TryPlaceThing(item2, item, map, ThingPlaceMode.Near, null, (IntVec3 x) => !occupiedRect.Contains(x)))
					{
						item2.Destroy();
					}
				}
			}
		}
	}

	public static bool WouldWipeAnythingWith(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, Predicate<Thing> predicate)
	{
		return WouldWipeAnythingWith(GenAdj.OccupiedRect(thingPos, thingRot, thingDef.Size), thingDef, map, predicate);
	}

	public static bool WouldWipeAnythingWith(CellRect cellRect, BuildableDef thingDef, Map map, Predicate<Thing> predicate)
	{
		foreach (IntVec3 item in cellRect)
		{
			foreach (Thing item2 in map.thingGrid.ThingsAt(item).ToList())
			{
				if (SpawningWipes(thingDef, item2.def) && predicate(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef, bool ignoreDestroyable = false)
	{
		ThingDef thingDef = newEntDef as ThingDef;
		ThingDef thingDef2 = oldEntDef as ThingDef;
		if (thingDef == null || thingDef2 == null)
		{
			return false;
		}
		if (thingDef.category == ThingCategory.Attachment || thingDef.category == ThingCategory.Mote || thingDef.category == ThingCategory.Filth || thingDef.category == ThingCategory.Projectile)
		{
			return false;
		}
		if (!ignoreDestroyable && !thingDef2.destroyable)
		{
			return false;
		}
		if (thingDef.category == ThingCategory.Plant)
		{
			return false;
		}
		if (thingDef.category == ThingCategory.PsychicEmitter)
		{
			return thingDef2.category == ThingCategory.PsychicEmitter;
		}
		if (thingDef2.category == ThingCategory.Filth && thingDef.passability != Traversability.Standable)
		{
			return true;
		}
		if (thingDef2.category == ThingCategory.Item && thingDef.passability == Traversability.Impassable && thingDef.surfaceType == SurfaceType.None)
		{
			return true;
		}
		if (thingDef.EverTransmitsPower && thingDef2.building != null && thingDef2.building.isPowerConduit)
		{
			return true;
		}
		if (thingDef.IsFrame && SpawningWipes(thingDef.entityDefToBuild, oldEntDef))
		{
			return true;
		}
		BuildableDef buildableDef = GenConstruct.BuiltDefOf(thingDef);
		BuildableDef buildableDef2 = GenConstruct.BuiltDefOf(thingDef2);
		if (buildableDef == null || buildableDef2 == null)
		{
			return false;
		}
		ThingDef thingDef3 = thingDef2.entityDefToBuild as ThingDef;
		ThingDef thingDef4 = thingDef.entityDefToBuild as ThingDef;
		if (thingDef2.IsBlueprint)
		{
			if (thingDef.IsBlueprint)
			{
				if (thingDef4 != null && thingDef4.building != null && thingDef4.building.canPlaceOverWall && thingDef2.entityDefToBuild is ThingDef { building: not null } thingDef5 && thingDef5.building.isPlaceOverableWall)
				{
					return true;
				}
				if (thingDef4 != null && thingDef4.replaceTags.NotNullAndContainsAnyElement(thingDef2.replaceTags))
				{
					return true;
				}
				if (thingDef2.entityDefToBuild is TerrainDef)
				{
					if (thingDef.entityDefToBuild is ThingDef && ((ThingDef)thingDef.entityDefToBuild).coversFloor)
					{
						return true;
					}
					if (thingDef.entityDefToBuild is TerrainDef)
					{
						return true;
					}
				}
			}
			if (thingDef3 != null && thingDef3.building?.isPowerConduit == true && thingDef4 != null && thingDef4.EverTransmitsPower)
			{
				return true;
			}
			return false;
		}
		if ((thingDef2.IsFrame || thingDef2.IsBlueprint) && thingDef2.entityDefToBuild is TerrainDef && buildableDef is ThingDef { CoexistsWithFloors: false })
		{
			return true;
		}
		if (thingDef2 == ThingDefOf.ActiveDropPod || thingDef == ThingDefOf.ActiveDropPod)
		{
			return false;
		}
		if (thingDef.wipesPlants && thingDef.category == ThingCategory.Building && thingDef2.category == ThingCategory.Plant)
		{
			return true;
		}
		if (thingDef.BlocksPlanting() && thingDef2.category == ThingCategory.Plant)
		{
			return true;
		}
		if (thingDef.IsEdifice())
		{
			if (thingDef2.category == ThingCategory.PsychicEmitter)
			{
				return true;
			}
			if (!(buildableDef is TerrainDef) && buildableDef2.IsEdifice())
			{
				return true;
			}
		}
		if (thingDef.blocksAltitudes != null && thingDef.blocksAltitudes.Contains(thingDef2.altitudeLayer))
		{
			return true;
		}
		if (thingDef2.blocksAltitudes != null && thingDef2.blocksAltitudes.Contains(thingDef.altitudeLayer))
		{
			return true;
		}
		return false;
	}

	public static bool CanSpawnAt(ThingDef thingDef, IntVec3 c, Map map, Rot4? rot = null, bool canWipeEdifices = true)
	{
		Rot4 valueOrDefault = rot.GetValueOrDefault();
		if (!rot.HasValue)
		{
			valueOrDefault = thingDef.defaultPlacingRot;
			rot = valueOrDefault;
		}
		if (!thingDef.CanSpawnAt(c, rot.Value, map))
		{
			return false;
		}
		foreach (IntVec3 item in GenAdj.OccupiedRect(c, rot.Value, thingDef.Size))
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			if (!c.Walkable(map))
			{
				return false;
			}
			if (!canWipeEdifices && map.edificeGrid[item] != null)
			{
				return false;
			}
			foreach (Thing thing in c.GetThingList(map))
			{
				if (SpawningWipes(thingDef, thing.def, ignoreDestroyable: true) && !thing.def.destroyable)
				{
					return false;
				}
			}
		}
		if (thingDef.category == ThingCategory.Building && !GenConstruct.CanBuildOnTerrain(thingDef, c, map, rot.Value))
		{
			return false;
		}
		if (thingDef.HasSingleOrMultipleInteractionCells && !GenConstruct.InteractionCellStandable(thingDef, c, rot.Value, map))
		{
			return false;
		}
		if (!GenConstruct.NotBlockingAnyInteractionCells(thingDef, c, rot.Value, map))
		{
			return false;
		}
		return true;
	}
}
