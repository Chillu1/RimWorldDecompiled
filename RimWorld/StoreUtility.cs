using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class StoreUtility
	{
		public static IHaulDestination CurrentHaulDestinationOf(Thing t)
		{
			if (t.Spawned)
			{
				return t.Map.haulDestinationManager.SlotGroupParentAt(t.Position);
			}
			return t.ParentHolder as IHaulDestination;
		}

		public static StoragePriority CurrentStoragePriorityOf(Thing t)
		{
			return StoragePriorityAtFor(CurrentHaulDestinationOf(t), t);
		}

		public static StoragePriority StoragePriorityAtFor(IntVec3 c, Thing t)
		{
			return StoragePriorityAtFor(t.Map.haulDestinationManager.SlotGroupParentAt(c), t);
		}

		public static StoragePriority StoragePriorityAtFor(IHaulDestination at, Thing t)
		{
			if (at == null || !at.Accepts(t))
			{
				return StoragePriority.Unstored;
			}
			return at.GetStoreSettings().Priority;
		}

		public static bool IsInAnyStorage(this Thing t)
		{
			return CurrentHaulDestinationOf(t) != null;
		}

		public static bool IsInValidStorage(this Thing t)
		{
			return CurrentHaulDestinationOf(t)?.Accepts(t) ?? false;
		}

		public static bool IsInValidBestStorage(this Thing t)
		{
			IHaulDestination haulDestination = CurrentHaulDestinationOf(t);
			if (haulDestination == null || !haulDestination.Accepts(t))
			{
				return false;
			}
			if (TryFindBestBetterStorageFor(t, null, t.Map, haulDestination.GetStoreSettings().Priority, Faction.OfPlayer, out IntVec3 _, out IHaulDestination _, needAccurateResult: false))
			{
				return false;
			}
			return true;
		}

		public static Thing StoringThing(this Thing t)
		{
			return CurrentHaulDestinationOf(t) as Thing;
		}

		public static SlotGroup GetSlotGroup(this Thing thing)
		{
			if (!thing.Spawned)
			{
				return null;
			}
			return thing.Position.GetSlotGroup(thing.Map);
		}

		public static SlotGroup GetSlotGroup(this IntVec3 c, Map map)
		{
			if (map.haulDestinationManager == null)
			{
				return null;
			}
			return map.haulDestinationManager.SlotGroupAt(c);
		}

		public static bool IsValidStorageFor(this IntVec3 c, Map map, Thing storable)
		{
			if (!NoStorageBlockersIn(c, map, storable))
			{
				return false;
			}
			SlotGroup slotGroup = c.GetSlotGroup(map);
			if (slotGroup == null || !slotGroup.parent.Accepts(storable))
			{
				return false;
			}
			return true;
		}

		private static bool NoStorageBlockersIn(IntVec3 c, Map map, Thing thing)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2.def.EverStorable(willMinifyIfPossible: false))
				{
					if (!thing2.CanStackWith(thing))
					{
						return false;
					}
					if (thing2.stackCount >= thing.def.stackLimit)
					{
						return false;
					}
				}
				if (thing2.def.entityDefToBuild != null && thing2.def.entityDefToBuild.passability != 0)
				{
					return false;
				}
				if (thing2.def.surfaceType == SurfaceType.None && thing2.def.passability != 0)
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindBestBetterStorageFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, out IHaulDestination haulDestination, bool needAccurateResult = true)
		{
			IntVec3 foundCell2 = IntVec3.Invalid;
			StoragePriority storagePriority = StoragePriority.Unstored;
			if (TryFindBestBetterStoreCellFor(t, carrier, map, currentPriority, faction, out foundCell2, needAccurateResult))
			{
				storagePriority = foundCell2.GetSlotGroup(map).Settings.Priority;
			}
			if (!TryFindBestBetterNonSlotGroupStorageFor(t, carrier, map, currentPriority, faction, out IHaulDestination haulDestination2))
			{
				haulDestination2 = null;
			}
			if (storagePriority == StoragePriority.Unstored && haulDestination2 == null)
			{
				foundCell = IntVec3.Invalid;
				haulDestination = null;
				return false;
			}
			if (haulDestination2 != null && (storagePriority == StoragePriority.Unstored || (int)haulDestination2.GetStoreSettings().Priority > (int)storagePriority))
			{
				foundCell = IntVec3.Invalid;
				haulDestination = haulDestination2;
				return true;
			}
			foundCell = foundCell2;
			haulDestination = foundCell2.GetSlotGroup(map).parent;
			return true;
		}

		public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, bool needAccurateResult = true)
		{
			List<SlotGroup> allGroupsListInPriorityOrder = map.haulDestinationManager.AllGroupsListInPriorityOrder;
			if (allGroupsListInPriorityOrder.Count == 0)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			StoragePriority foundPriority = currentPriority;
			float closestDistSquared = 2.14748365E+09f;
			IntVec3 closestSlot = IntVec3.Invalid;
			int count = allGroupsListInPriorityOrder.Count;
			for (int i = 0; i < count; i++)
			{
				SlotGroup slotGroup = allGroupsListInPriorityOrder[i];
				StoragePriority priority = slotGroup.Settings.Priority;
				if ((int)priority < (int)foundPriority || (int)priority <= (int)currentPriority)
				{
					break;
				}
				TryFindBestBetterStoreCellForWorker(t, carrier, map, faction, slotGroup, needAccurateResult, ref closestSlot, ref closestDistSquared, ref foundPriority);
			}
			if (!closestSlot.IsValid)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			foundCell = closestSlot;
			return true;
		}

		public static bool TryFindBestBetterStoreCellForIn(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, SlotGroup slotGroup, out IntVec3 foundCell, bool needAccurateResult = true)
		{
			foundCell = IntVec3.Invalid;
			float closestDistSquared = 2.14748365E+09f;
			TryFindBestBetterStoreCellForWorker(t, carrier, map, faction, slotGroup, needAccurateResult, ref foundCell, ref closestDistSquared, ref currentPriority);
			return foundCell.IsValid;
		}

		private static void TryFindBestBetterStoreCellForWorker(Thing t, Pawn carrier, Map map, Faction faction, SlotGroup slotGroup, bool needAccurateResult, ref IntVec3 closestSlot, ref float closestDistSquared, ref StoragePriority foundPriority)
		{
			if (slotGroup == null || !slotGroup.parent.Accepts(t))
			{
				return;
			}
			IntVec3 a = t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld;
			List<IntVec3> cellsList = slotGroup.CellsList;
			int count = cellsList.Count;
			int num = needAccurateResult ? Mathf.FloorToInt((float)count * Rand.Range(0.005f, 0.018f)) : 0;
			for (int i = 0; i < count; i++)
			{
				IntVec3 intVec = cellsList[i];
				float num2 = (a - intVec).LengthHorizontalSquared;
				if (!(num2 > closestDistSquared) && IsGoodStoreCell(intVec, map, t, carrier, faction))
				{
					closestSlot = intVec;
					closestDistSquared = num2;
					foundPriority = slotGroup.Settings.Priority;
					if (i >= num)
					{
						break;
					}
				}
			}
		}

		public static bool TryFindBestBetterNonSlotGroupStorageFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IHaulDestination haulDestination, bool acceptSamePriority = false)
		{
			List<IHaulDestination> allHaulDestinationsListInPriorityOrder = map.haulDestinationManager.AllHaulDestinationsListInPriorityOrder;
			IntVec3 intVec = t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld;
			float num = float.MaxValue;
			StoragePriority storagePriority = StoragePriority.Unstored;
			haulDestination = null;
			for (int i = 0; i < allHaulDestinationsListInPriorityOrder.Count; i++)
			{
				if (allHaulDestinationsListInPriorityOrder[i] is ISlotGroupParent)
				{
					continue;
				}
				StoragePriority priority = allHaulDestinationsListInPriorityOrder[i].GetStoreSettings().Priority;
				if ((int)priority < (int)storagePriority || (acceptSamePriority && (int)priority < (int)currentPriority) || (!acceptSamePriority && (int)priority <= (int)currentPriority))
				{
					break;
				}
				float num2 = intVec.DistanceToSquared(allHaulDestinationsListInPriorityOrder[i].Position);
				if (num2 > num || !allHaulDestinationsListInPriorityOrder[i].Accepts(t))
				{
					continue;
				}
				Thing thing = allHaulDestinationsListInPriorityOrder[i] as Thing;
				if (thing != null && thing.Faction != faction)
				{
					continue;
				}
				if (thing != null)
				{
					if (carrier != null)
					{
						if (thing.IsForbidden(carrier))
						{
							continue;
						}
					}
					else if (faction != null && thing.IsForbidden(faction))
					{
						continue;
					}
				}
				if (thing != null)
				{
					if (carrier != null)
					{
						if (!carrier.CanReserveNew(thing))
						{
							continue;
						}
					}
					else if (faction != null && map.reservationManager.IsReservedByAnyoneOf(thing, faction))
					{
						continue;
					}
				}
				if (carrier != null)
				{
					if (thing != null)
					{
						if (!carrier.Map.reachability.CanReach(intVec, thing, PathEndMode.ClosestTouch, TraverseParms.For(carrier)))
						{
							continue;
						}
					}
					else if (!carrier.Map.reachability.CanReach(intVec, allHaulDestinationsListInPriorityOrder[i].Position, PathEndMode.ClosestTouch, TraverseParms.For(carrier)))
					{
						continue;
					}
				}
				num = num2;
				storagePriority = priority;
				haulDestination = allHaulDestinationsListInPriorityOrder[i];
			}
			return haulDestination != null;
		}

		public static bool IsGoodStoreCell(IntVec3 c, Map map, Thing t, Pawn carrier, Faction faction)
		{
			if (carrier != null && c.IsForbidden(carrier))
			{
				return false;
			}
			if (!NoStorageBlockersIn(c, map, t))
			{
				return false;
			}
			if (carrier != null)
			{
				if (!carrier.CanReserveNew(c))
				{
					return false;
				}
			}
			else if (faction != null && map.reservationManager.IsReservedByAnyoneOf(c, faction))
			{
				return false;
			}
			if (c.ContainsStaticFire(map))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is IConstructible && GenConstruct.BlocksConstruction(thingList[i], t))
				{
					return false;
				}
			}
			if (carrier != null && !carrier.Map.reachability.CanReach(t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld, c, PathEndMode.ClosestTouch, TraverseParms.For(carrier)))
			{
				return false;
			}
			return true;
		}

		public static bool TryFindStoreCellNearColonyDesperate(Thing item, Pawn carrier, out IntVec3 storeCell)
		{
			if (TryFindBestBetterStoreCellFor(item, carrier, carrier.Map, StoragePriority.Unstored, carrier.Faction, out storeCell))
			{
				return true;
			}
			for (int i = -4; i < 20; i++)
			{
				int num = (i < 0) ? Rand.RangeInclusive(0, 4) : i;
				IntVec3 intVec = carrier.Position + GenRadial.RadialPattern[num];
				if (intVec.InBounds(carrier.Map) && carrier.Map.areaManager.Home[intVec] && carrier.CanReach(intVec, PathEndMode.ClosestTouch, Danger.Deadly) && intVec.GetSlotGroup(carrier.Map) == null && IsGoodStoreCell(intVec, carrier.Map, item, carrier, carrier.Faction))
				{
					storeCell = intVec;
					return true;
				}
			}
			if (RCellFinder.TryFindRandomSpotJustOutsideColony(carrier.Position, carrier.Map, carrier, out storeCell, (IntVec3 x) => x.GetSlotGroup(carrier.Map) == null && IsGoodStoreCell(x, carrier.Map, item, carrier, carrier.Faction)))
			{
				return true;
			}
			storeCell = IntVec3.Invalid;
			return false;
		}
	}
}
