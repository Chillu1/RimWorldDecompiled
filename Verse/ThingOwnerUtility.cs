using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class ThingOwnerUtility
{
	private static readonly Stack<IThingHolder> tmpStack = new Stack<IThingHolder>();

	private static readonly List<IThingHolder> tmpHolders = new List<IThingHolder>();

	private static readonly List<Thing> tmpThings = new List<Thing>();

	private static readonly List<IThingHolder> tmpMapChildHolders = new List<IThingHolder>();

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
				if (allComps[i] is IThingHolder thingHolder2)
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
				if (!(allComps2[j] is IThingHolder thingHolder3))
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
			if (holder is Thing { Spawned: not false } thing)
			{
				return thing;
			}
			if (holder is ThingComp thingComp && thingComp.parent.Spawned)
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
			if (holder is Thing { Position: { IsValid: not false } } thing)
			{
				result = thing.Position;
			}
			else if (holder is ThingComp thingComp && thingComp.parent.Position.IsValid)
			{
				result = thingComp.parent.Position;
			}
			holder = holder.ParentHolder;
		}
		return result;
	}

	public static Map GetRootMap(IThingHolder holder)
	{
		while (holder != null)
		{
			if (holder is Map result)
			{
				return result;
			}
			holder = holder.ParentHolder;
		}
		return null;
	}

	public static PlanetTile GetRootTile(IThingHolder holder)
	{
		while (holder != null)
		{
			if (holder is WorldObject { Tile: { Valid: not false } } worldObject)
			{
				return worldObject.Tile;
			}
			holder = holder.ParentHolder;
		}
		return PlanetTile.Invalid;
	}

	public static bool ContentsSuspended(IThingHolder holder)
	{
		while (holder != null)
		{
			if (holder is Building_CryptosleepCasket || holder is ISuspendableThingHolder { IsContentsSuspended: not false })
			{
				return true;
			}
			holder = holder.ParentHolder;
		}
		return false;
	}

	public static bool ContentsInCryptosleep(IThingHolder holder)
	{
		while (holder != null)
		{
			if (holder is Building_CryptosleepCasket)
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
			if (container[i] is IThingHolder item)
			{
				outThingsHolders.Add(item);
			}
			if (!(container[i] is ThingWithComps { AllComps: var allComps }))
			{
				continue;
			}
			for (int j = 0; j < allComps.Count; j++)
			{
				if (allComps[j] is IThingHolder item2)
				{
					outThingsHolders.Add(item2);
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
		if (thing is T result)
		{
			return result;
		}
		for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
		{
			if (parentHolder is T result2)
			{
				return result2;
			}
		}
		return null;
	}

	public static Thing GetFirstParentThing(Thing thing)
	{
		for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
		{
			if (parentHolder is Thing result)
			{
				return result;
			}
			if (parentHolder is ThingComp thingComp)
			{
				return thingComp.parent;
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
			if (parentHolder is Thing { Spawned: not false } thing2)
			{
				return thing2;
			}
			if (parentHolder is ThingComp thingComp && thingComp.parent.Spawned)
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
				if (list[i] is T item)
				{
					outThings.Add(item);
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
				if (tmpThings[k] is T val && request.Accepts(val))
				{
					outThings.Add(val);
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
		if (holder is CompLaunchable || holder is ActiveTransporterInfo || holder is TravellingTransporters)
		{
			temperature = 14f;
			return true;
		}
		if (holder is Settlement_TraderTracker || holder is TradeShip)
		{
			temperature = 14f;
			return true;
		}
		if (holder is CompTransporter)
		{
			temperature = 14f;
			return true;
		}
		temperature = 21f;
		return false;
	}
}
