using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenSpawn
	{
		public static Thing Spawn(ThingDef def, IntVec3 loc, Map map, WipeMode wipeMode = WipeMode.Vanish)
		{
			return Spawn(ThingMaker.MakeThing(def), loc, map, wipeMode);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, WipeMode wipeMode = WipeMode.Vanish)
		{
			return Spawn(newThing, loc, map, Rot4.North, wipeMode);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false)
		{
			if (map == null)
			{
				Log.Error("Tried to spawn " + newThing.ToStringSafe() + " in a null map.");
				return null;
			}
			if (!loc.InBounds(map))
			{
				Log.Error(string.Concat("Tried to spawn ", newThing.ToStringSafe(), " out of bounds at ", loc, "."));
				return null;
			}
			if (newThing.def.randomizeRotationOnSpawn)
			{
				rot = Rot4.Random;
			}
			CellRect occupiedRect = GenAdj.OccupiedRect(loc, rot, newThing.def.Size);
			if (!occupiedRect.InBounds(map))
			{
				Log.Error(string.Concat("Tried to spawn ", newThing.ToStringSafe(), " out of bounds at ", loc, " (out of bounds because size is ", newThing.def.Size, ")."));
				return null;
			}
			if (newThing.Spawned)
			{
				Log.Error(string.Concat("Tried to spawn ", newThing, " but it's already spawned."));
				return newThing;
			}
			switch (wipeMode)
			{
			case WipeMode.Vanish:
				WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
				break;
			case WipeMode.FullRefund:
				WipeAndRefundExistingThings(loc, rot, newThing.def, map);
				break;
			case WipeMode.VanishOrMoveAside:
				CheckMoveItemsAside(loc, rot, newThing.def, map);
				WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
				break;
			}
			if (newThing.def.category == ThingCategory.Item)
			{
				foreach (IntVec3 item in occupiedRect)
				{
					foreach (Thing item2 in item.GetThingList(map).ToList())
					{
						if (item2 != newThing && item2.def.category == ThingCategory.Item)
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
				foreach (IntVec3 item3 in occupiedRect)
				{
					foreach (Thing item4 in item3.GetThingList(map).ToList())
					{
						if (item4 != newThing)
						{
							(item4 as Pawn)?.pather.TryRecoverFromUnwalkablePosition(error: false);
						}
					}
				}
				return newThing;
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

		public static void Refund(Thing thing, Map map, CellRect avoidThisRect)
		{
			bool flag = false;
			if (thing.def.Minifiable)
			{
				MinifiedThing minifiedThing = thing.MakeMinified();
				if (GenPlace.TryPlaceThing(minifiedThing, thing.Position, map, ThingPlaceMode.Near, null, (IntVec3 x) => !avoidThisRect.Contains(x)))
				{
					flag = true;
				}
				else
				{
					minifiedThing.GetDirectlyHeldThings().Clear();
					minifiedThing.Destroy();
				}
			}
			if (!flag)
			{
				GenLeaving.DoLeavingsFor(thing, map, DestroyMode.Refund, thing.OccupiedRect(), (IntVec3 x) => !avoidThisRect.Contains(x));
				thing.Destroy();
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

		public static void WipeAndRefundExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map)
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
					}
					else
					{
						Refund(item2, map, occupiedRect);
					}
				}
			}
		}

		public static void CheckMoveItemsAside(IntVec3 thingPos, Rot4 thingRot, ThingDef thingDef, Map map)
		{
			if (thingDef.surfaceType != 0 || thingDef.passability == Traversability.Standable)
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

		public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
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
			if (!thingDef2.destroyable)
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Plant)
			{
				return false;
			}
			if (thingDef2.category == ThingCategory.Filth && thingDef.passability != 0)
			{
				return true;
			}
			if (thingDef2.category == ThingCategory.Item && thingDef.passability == Traversability.Impassable && thingDef.surfaceType == SurfaceType.None)
			{
				return true;
			}
			if (thingDef.EverTransmitsPower && thingDef2 == ThingDefOf.PowerConduit)
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
			ThingDef thingDef3 = thingDef.entityDefToBuild as ThingDef;
			if (thingDef2.IsBlueprint)
			{
				if (thingDef.IsBlueprint)
				{
					if (thingDef3 != null && thingDef3.building != null && thingDef3.building.canPlaceOverWall && thingDef2.entityDefToBuild is ThingDef && (ThingDef)thingDef2.entityDefToBuild == ThingDefOf.Wall)
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
				if (thingDef2.entityDefToBuild == ThingDefOf.PowerConduit && thingDef.entityDefToBuild is ThingDef && (thingDef.entityDefToBuild as ThingDef).EverTransmitsPower)
				{
					return true;
				}
				return false;
			}
			if ((thingDef2.IsFrame || thingDef2.IsBlueprint) && thingDef2.entityDefToBuild is TerrainDef)
			{
				ThingDef thingDef4 = buildableDef as ThingDef;
				if (thingDef4 != null && !thingDef4.CoexistsWithFloors)
				{
					return true;
				}
			}
			if (thingDef2 == ThingDefOf.ActiveDropPod || thingDef == ThingDefOf.ActiveDropPod)
			{
				return false;
			}
			if (thingDef.IsEdifice())
			{
				if (thingDef.BlockPlanting && thingDef2.category == ThingCategory.Plant)
				{
					return true;
				}
				if (!(buildableDef is TerrainDef) && buildableDef2.IsEdifice())
				{
					return true;
				}
			}
			return false;
		}
	}
}
