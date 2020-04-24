using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ThingOwnerUtility
	{
		private static Stack<IThingHolder> tmpStack = new Stack<IThingHolder>();

		private static List<IThingHolder> tmpHolders = new List<IThingHolder>();

		private static List<Thing> tmpThings = new List<Thing>();

		private static List<IThingHolder> tmpMapChildHolders = new List<IThingHolder>();

		public static bool ThisOrAnyCompIsThingHolder(this ThingDef thingDef)
		{
			if (typeof(IThingHolder).IsAssignableFrom(thingDef.thingClass))
			{
				return true;
			}
			for (int i = 0; i < thingDef.comps.Count; i++)
			{
				if (typeof(IThingHolder).IsAssignableFrom(thingDef.comps[i].compClass))
				{
					return true;
				}
			}
			return false;
		}

		public static ThingOwner TryGetInnerInteractableThingOwner(this Thing thing)
		{
			IThingHolder thingHolder = thing as IThingHolder;
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingHolder != null)
			{
				ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
				if (directlyHeldThings != null)
				{
					return directlyHeldThings;
				}
			}
			if (thingWithComps != null)
			{
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					IThingHolder thingHolder2 = allComps[i] as IThingHolder;
					if (thingHolder2 != null)
					{
						ThingOwner directlyHeldThings2 = thingHolder2.GetDirectlyHeldThings();
						if (directlyHeldThings2 != null)
						{
							return directlyHeldThings2;
						}
					}
				}
			}
			tmpHolders.Clear();
			if (thingHolder != null)
			{
				thingHolder.GetChildHolders(tmpHolders);
				if (tmpHolders.Any())
				{
					ThingOwner directlyHeldThings3 = tmpHolders[0].GetDirectlyHeldThings();
					if (directlyHeldThings3 != null)
					{
						tmpHolders.Clear();
						return directlyHeldThings3;
					}
				}
			}
			if (thingWithComps != null)
			{
				List<ThingComp> allComps2 = thingWithComps.AllComps;
				for (int j = 0; j < allComps2.Count; j++)
				{
					IThingHolder thingHolder3 = allComps2[j] as IThingHolder;
					if (thingHolder3 == null)
					{
						continue;
					}
					thingHolder3.GetChildHolders(tmpHolders);
					if (tmpHolders.Any())
					{
						ThingOwner directlyHeldThings4 = tmpHolders[0].GetDirectlyHeldThings();
						if (directlyHeldThings4 != null)
						{
							tmpHolders.Clear();
							return directlyHeldThings4;
						}
					}
				}
			}
			tmpHolders.Clear();
			return null;
		}

		public static bool SpawnedOrAnyParentSpawned(IThingHolder holder)
		{
			return SpawnedParentOrMe(holder) != null;
		}

		public static Thing SpawnedParentOrMe(IThingHolder holder)
		{
			while (holder != null)
			{
				Thing thing = holder as Thing;
				if (thing != null && thing.Spawned)
				{
					return thing;
				}
				ThingComp thingComp = holder as ThingComp;
				if (thingComp != null && thingComp.parent.Spawned)
				{
					return thingComp.parent;
				}
				holder = holder.ParentHolder;
			}
			return null;
		}

		public static IntVec3 GetRootPosition(IThingHolder holder)
		{
			IntVec3 result = IntVec3.Invalid;
			while (holder != null)
			{
				Thing thing = holder as Thing;
				if (thing != null && thing.Position.IsValid)
				{
					result = thing.Position;
				}
				else
				{
					ThingComp thingComp = holder as ThingComp;
					if (thingComp != null && thingComp.parent.Position.IsValid)
					{
						result = thingComp.parent.Position;
					}
				}
				holder = holder.ParentHolder;
			}
			return result;
		}

		public static Map GetRootMap(IThingHolder holder)
		{
			while (holder != null)
			{
				Map map = holder as Map;
				if (map != null)
				{
					return map;
				}
				holder = holder.ParentHolder;
			}
			return null;
		}

		public static int GetRootTile(IThingHolder holder)
		{
			while (holder != null)
			{
				WorldObject worldObject = holder as WorldObject;
				if (worldObject != null && worldObject.Tile >= 0)
				{
					return worldObject.Tile;
				}
				holder = holder.ParentHolder;
			}
			return -1;
		}

		public static bool ContentsSuspended(IThingHolder holder)
		{
			while (holder != null)
			{
				if (holder is Building_CryptosleepCasket || holder is ImportantPawnComp)
				{
					return true;
				}
				holder = holder.ParentHolder;
			}
			return false;
		}

		public static bool IsEnclosingContainer(this IThingHolder holder)
		{
			if (holder != null && !(holder is Pawn_CarryTracker) && !(holder is Corpse) && !(holder is Map) && !(holder is Caravan) && !(holder is Settlement_TraderTracker))
			{
				return !(holder is TradeShip);
			}
			return false;
		}

		public static bool ShouldAutoRemoveDestroyedThings(IThingHolder holder)
		{
			if (!(holder is Corpse))
			{
				return !(holder is Caravan);
			}
			return false;
		}

		public static bool ShouldAutoExtinguishInnerThings(IThingHolder holder)
		{
			return !(holder is Map);
		}

		public static bool ShouldRemoveDesignationsOnAddedThings(IThingHolder holder)
		{
			return holder.IsEnclosingContainer();
		}

		public static void AppendThingHoldersFromThings(List<IThingHolder> outThingsHolders, IList<Thing> container)
		{
			if (container == null)
			{
				return;
			}
			int i = 0;
			for (int count = container.Count; i < count; i++)
			{
				IThingHolder thingHolder = container[i] as IThingHolder;
				if (thingHolder != null)
				{
					outThingsHolders.Add(thingHolder);
				}
				ThingWithComps thingWithComps = container[i] as ThingWithComps;
				if (thingWithComps == null)
				{
					continue;
				}
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int j = 0; j < allComps.Count; j++)
				{
					IThingHolder thingHolder2 = allComps[j] as IThingHolder;
					if (thingHolder2 != null)
					{
						outThingsHolders.Add(thingHolder2);
					}
				}
			}
		}

		public static bool AnyParentIs<T>(Thing thing) where T : class, IThingHolder
		{
			return GetAnyParent<T>(thing) != null;
		}

		public static T GetAnyParent<T>(Thing thing) where T : class, IThingHolder
		{
			T val = thing as T;
			if (val != null)
			{
				return val;
			}
			for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
			{
				T val2 = parentHolder as T;
				if (val2 != null)
				{
					return val2;
				}
			}
			return null;
		}

		public static Thing GetFirstSpawnedParentThing(Thing thing)
		{
			if (thing.Spawned)
			{
				return thing;
			}
			for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
			{
				Thing thing2 = parentHolder as Thing;
				if (thing2 != null && thing2.Spawned)
				{
					return thing2;
				}
				ThingComp thingComp = parentHolder as ThingComp;
				if (thingComp != null && thingComp.parent.Spawned)
				{
					return thingComp.parent;
				}
			}
			return null;
		}

		public static void GetAllThingsRecursively(IThingHolder holder, List<Thing> outThings, bool allowUnreal = true, Predicate<IThingHolder> passCheck = null)
		{
			outThings.Clear();
			if (passCheck != null && !passCheck(holder))
			{
				return;
			}
			tmpStack.Clear();
			tmpStack.Push(holder);
			while (tmpStack.Count != 0)
			{
				IThingHolder thingHolder = tmpStack.Pop();
				if (allowUnreal || AreImmediateContentsReal(thingHolder))
				{
					ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
					if (directlyHeldThings != null)
					{
						outThings.AddRange(directlyHeldThings);
					}
				}
				tmpHolders.Clear();
				thingHolder.GetChildHolders(tmpHolders);
				for (int i = 0; i < tmpHolders.Count; i++)
				{
					if (passCheck == null || passCheck(tmpHolders[i]))
					{
						tmpStack.Push(tmpHolders[i]);
					}
				}
			}
			tmpStack.Clear();
			tmpHolders.Clear();
		}

		public static void GetAllThingsRecursively<T>(Map map, ThingRequest request, List<T> outThings, bool allowUnreal = true, Predicate<IThingHolder> passCheck = null, bool alsoGetSpawnedThings = true) where T : Thing
		{
			outThings.Clear();
			if (alsoGetSpawnedThings)
			{
				List<Thing> list = map.listerThings.ThingsMatching(request);
				for (int i = 0; i < list.Count; i++)
				{
					T val = list[i] as T;
					if (val != null)
					{
						outThings.Add(val);
					}
				}
			}
			tmpMapChildHolders.Clear();
			map.GetChildHolders(tmpMapChildHolders);
			for (int j = 0; j < tmpMapChildHolders.Count; j++)
			{
				tmpThings.Clear();
				GetAllThingsRecursively(tmpMapChildHolders[j], tmpThings, allowUnreal, passCheck);
				for (int k = 0; k < tmpThings.Count; k++)
				{
					T val2 = tmpThings[k] as T;
					if (val2 != null && request.Accepts(val2))
					{
						outThings.Add(val2);
					}
				}
			}
			tmpThings.Clear();
			tmpMapChildHolders.Clear();
		}

		public static List<Thing> GetAllThingsRecursively(IThingHolder holder, bool allowUnreal = true)
		{
			List<Thing> list = new List<Thing>();
			GetAllThingsRecursively(holder, list, allowUnreal);
			return list;
		}

		public static bool AreImmediateContentsReal(IThingHolder holder)
		{
			if (!(holder is Corpse))
			{
				return !(holder is MinifiedThing);
			}
			return false;
		}

		public static bool TryGetFixedTemperature(IThingHolder holder, Thing forThing, out float temperature)
		{
			if (holder is Pawn_InventoryTracker && forThing.TryGetComp<CompHatcher>() != null)
			{
				temperature = 14f;
				return true;
			}
			if (holder is CompLaunchable || holder is ActiveDropPodInfo || holder is TravelingTransportPods)
			{
				temperature = 14f;
				return true;
			}
			if (holder is Settlement_TraderTracker || holder is TradeShip)
			{
				temperature = 14f;
				return true;
			}
			temperature = 21f;
			return false;
		}
	}
}
