using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class ThingOwner<T> : ThingOwner, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : Thing
{
	private List<T> innerList = new List<T>();

	public List<T> InnerListForReading => innerList;

	public new T this[int index] => innerList[index];

	public override int Count => innerList.Count;

	T IList<T>.this[int index]
	{
		get
		{
			return innerList[index];
		}
		set
		{
			throw new InvalidOperationException("ThingOwner doesn't allow setting individual elements.");
		}
	}

	bool ICollection<T>.IsReadOnly => true;

	public ThingOwner()
	{
	}

	public ThingOwner(IThingHolder owner)
		: base(owner)
	{
	}

	public ThingOwner(IThingHolder owner, LookMode contentsLookMode = LookMode.Deep, bool removeContentsIfDestroyed = true)
		: base(owner)
	{
	}

	public ThingOwner(IThingHolder owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep, bool removeContentsIfDestroyed = true)
		: base(owner, oneStackOnly, contentsLookMode, removeContentsIfDestroyed)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref innerList, "innerList", true, contentsLookMode);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			int num = innerList.RemoveAll((T x) => x == null || (x is MinifiedThing minifiedThing && minifiedThing.InnerThing == null));
			if (num > 0)
			{
				Log.Warning($"ThingOwner removed {num} invalid entries during PostLoadInit.");
			}
		}
		if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num2 = 0; num2 < innerList.Count; num2++)
		{
			if (innerList[num2] != null)
			{
				innerList[num2].holdingOwner = this;
			}
		}
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return innerList.GetEnumerator();
	}

	public override int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
	{
		if (!(item is T))
		{
			return 0;
		}
		return base.GetCountCanAccept(item, canMergeWithExistingStacks);
	}

	public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
	{
		if (count <= 0)
		{
			return 0;
		}
		if (item == null)
		{
			Log.Warning("Tried to add null item to ThingOwner.");
			return 0;
		}
		if (Contains(item))
		{
			Log.Warning("Tried to add " + item?.ToString() + " to ThingOwner but this item is already here.");
			return 0;
		}
		if (item.holdingOwner != null)
		{
			Log.Warning("Tried to add " + count + " of " + item.ToStringSafe() + " to ThingOwner but this thing is already in another container. owner=" + owner.ToStringSafe() + ", current container owner=" + item.holdingOwner.Owner.ToStringSafe() + ". Use TryAddOrTransfer, TryTransferToContainer, or remove the item before adding it.");
			return 0;
		}
		if (!CanAcceptAnyOf(item, canMergeWithExistingStacks))
		{
			return 0;
		}
		int stackCount = item.stackCount;
		int num = Mathf.Min(stackCount, count);
		Thing thing = item.SplitOff(num);
		if (!TryAdd((T)thing, canMergeWithExistingStacks))
		{
			if (thing != item)
			{
				int result = stackCount - item.stackCount - thing.stackCount;
				item.TryAbsorbStack(thing, respectStackLimit: false);
				return result;
			}
			return stackCount - item.stackCount;
		}
		CompPushable compPushable = item.TryGetComp<CompPushable>();
		if (compPushable != null && owner is Pawn pawn)
		{
			compPushable.OnStartedCarrying(pawn);
		}
		return num;
	}

	public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
	{
		if (item == null)
		{
			Log.Warning("Tried to add null item to ThingOwner.");
			return false;
		}
		if (!(item is T item2))
		{
			return false;
		}
		if (Contains(item))
		{
			Log.Warning("Tried to add " + item.ToStringSafe() + " to ThingOwner but this item is already here.");
			return false;
		}
		if (item.holdingOwner != null)
		{
			Log.Warning("Tried to add " + item.ToStringSafe() + " to ThingOwner but this thing is already in another container. owner=" + owner.ToStringSafe() + ", current container owner=" + item.holdingOwner.Owner.ToStringSafe() + ". Use TryAddOrTransfer, TryTransferToContainer, or remove the item before adding it.");
			return false;
		}
		if (!CanAcceptAnyOf(item, canMergeWithExistingStacks))
		{
			return false;
		}
		if (canMergeWithExistingStacks)
		{
			for (int i = 0; i < innerList.Count; i++)
			{
				T val = innerList[i];
				if (!val.CanStackWith(item))
				{
					continue;
				}
				int num = Mathf.Min(item.stackCount, val.def.stackLimit - val.stackCount);
				if (num > 0)
				{
					Thing other = item.SplitOff(num);
					int stackCount = val.stackCount;
					val.TryAbsorbStack(other, respectStackLimit: true);
					if (val.stackCount > stackCount)
					{
						NotifyAddedAndMergedWith(val, val.stackCount - stackCount);
					}
					if (item.Destroyed || item.stackCount == 0)
					{
						return true;
					}
				}
			}
		}
		if (Count >= maxStacks)
		{
			return false;
		}
		item.holdingOwner = this;
		innerList.Add(item2);
		NotifyAdded(item2);
		return true;
	}

	protected override void NotifyAdded(Thing item)
	{
		if (owner is IThingHolderEvents<T> thingHolderEvents)
		{
			thingHolderEvents.Notify_ItemAdded(item as T);
		}
		base.NotifyAdded(item);
	}

	protected override void NotifyRemoved(Thing item)
	{
		if (owner is IThingHolderEvents<T> thingHolderEvents)
		{
			thingHolderEvents.Notify_ItemRemoved(item as T);
		}
		base.NotifyRemoved(item);
	}

	public void TryAddRangeOrTransfer(IEnumerable<T> things, bool canMergeWithExistingStacks = true, bool destroyLeftover = false)
	{
		if (things == this)
		{
			return;
		}
		if (things is ThingOwner thingOwner)
		{
			thingOwner.TryTransferAllToContainer(this, canMergeWithExistingStacks);
			if (destroyLeftover)
			{
				thingOwner.ClearAndDestroyContents();
			}
			return;
		}
		if (things is IList<T> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!TryAddOrTransfer(list[i], canMergeWithExistingStacks) && destroyLeftover)
				{
					list[i].Destroy();
				}
			}
			return;
		}
		foreach (T thing in things)
		{
			if (!TryAddOrTransfer(thing, canMergeWithExistingStacks) && destroyLeftover)
			{
				thing.Destroy();
			}
		}
	}

	public override int IndexOf(Thing item)
	{
		if (!(item is T item2))
		{
			return -1;
		}
		return innerList.IndexOf(item2);
	}

	public override bool Remove(Thing item)
	{
		if (!Contains(item))
		{
			return false;
		}
		if (item.holdingOwner == this)
		{
			item.holdingOwner = null;
		}
		int index = innerList.LastIndexOf((T)item);
		innerList.RemoveAt(index);
		NotifyRemoved(item);
		return true;
	}

	public int RemoveAll(Predicate<T> predicate)
	{
		int num = 0;
		for (int num2 = innerList.Count - 1; num2 >= 0; num2--)
		{
			if (predicate(innerList[num2]))
			{
				Remove(innerList[num2]);
				num++;
			}
		}
		return num;
	}

	protected override Thing GetAt(int index)
	{
		return innerList[index];
	}

	public void GetThingsOfType<J>(List<J> list) where J : Thing
	{
		for (int i = 0; i < innerList.Count; i++)
		{
			if (innerList[i] is J item)
			{
				list.Add(item);
			}
		}
	}

	public int TryTransferToContainer(Thing item, ThingOwner otherContainer, int stackCount, out T resultingTransferredItem, bool canMergeWithExistingStacks = true)
	{
		Thing resultingTransferredItem2;
		int result = TryTransferToContainer(item, otherContainer, stackCount, out resultingTransferredItem2, canMergeWithExistingStacks);
		resultingTransferredItem = (T)resultingTransferredItem2;
		return result;
	}

	public new T Take(Thing thing, int count)
	{
		return (T)base.Take(thing, count);
	}

	public new T Take(Thing thing)
	{
		return (T)base.Take(thing);
	}

	public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count, out T resultingThing, Action<T, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		Action<Thing, int> placedAction2 = null;
		if (placedAction != null)
		{
			placedAction2 = delegate(Thing t, int c)
			{
				placedAction((T)t, c);
			};
		}
		Thing resultingThing2;
		bool result = TryDrop(thing, dropLoc, map, mode, count, out resultingThing2, placedAction2, nearPlaceValidator);
		resultingThing = (T)resultingThing2;
		return result;
	}

	public bool TryDrop(Thing thing, ThingPlaceMode mode, out T lastResultingThing, Action<T, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		Action<Thing, int> placedAction2 = null;
		if (placedAction != null)
		{
			placedAction2 = delegate(Thing t, int c)
			{
				placedAction((T)t, c);
			};
		}
		Thing lastResultingThing2;
		bool result = TryDrop(thing, mode, out lastResultingThing2, placedAction2, nearPlaceValidator);
		lastResultingThing = (T)lastResultingThing2;
		return result;
	}

	public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, out T lastResultingThing, Action<T, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		Action<Thing, int> placedAction2 = null;
		if (placedAction != null)
		{
			placedAction2 = delegate(Thing t, int c)
			{
				placedAction((T)t, c);
			};
		}
		Thing lastResultingThing2;
		bool result = TryDrop(thing, dropLoc, map, mode, out lastResultingThing2, placedAction2, nearPlaceValidator, playDropSound: true);
		lastResultingThing = (T)lastResultingThing2;
		return result;
	}

	int IList<T>.IndexOf(T item)
	{
		return innerList.IndexOf(item);
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new InvalidOperationException("ThingOwner doesn't allow inserting individual elements at any position.");
	}

	void ICollection<T>.Add(T item)
	{
		TryAdd(item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		innerList.CopyTo(array, arrayIndex);
	}

	bool ICollection<T>.Contains(T item)
	{
		return innerList.Contains(item);
	}

	bool ICollection<T>.Remove(T item)
	{
		return Remove(item);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return innerList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return innerList.GetEnumerator();
	}
}
public abstract class ThingOwner : IExposable, IList<Thing>, ICollection<Thing>, IEnumerable<Thing>, IEnumerable
{
	protected IThingHolder owner;

	protected int maxStacks = 999999;

	public LookMode contentsLookMode = LookMode.Deep;

	public bool removeContentsIfDestroyed = true;

	public bool dontTickContents;

	private const int InfMaxStacks = 999999;

	public IThingHolder Owner => owner;

	public abstract int Count { get; }

	public Thing this[int index] => GetAt(index);

	public bool Any => Count > 0;

	public int TotalStackCount
	{
		get
		{
			int num = 0;
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				num += GetAt(i).stackCount;
			}
			return num;
		}
	}

	public string ContentsString
	{
		get
		{
			if (Any)
			{
				return GenThing.ThingsToCommaList(this);
			}
			return "NothingLower".Translate();
		}
	}

	Thing IList<Thing>.this[int index]
	{
		get
		{
			return GetAt(index);
		}
		set
		{
			throw new InvalidOperationException("ThingOwner doesn't allow setting individual elements.");
		}
	}

	bool ICollection<Thing>.IsReadOnly => true;

	public event Action OnContentsChanged;

	public ThingOwner()
	{
	}

	public ThingOwner(IThingHolder owner)
	{
		this.owner = owner;
	}

	public ThingOwner(IThingHolder owner, LookMode contentsLookMode = LookMode.Deep, bool removeContentsIfDestroyed = true)
	{
		this.owner = owner;
		this.contentsLookMode = contentsLookMode;
		this.removeContentsIfDestroyed = removeContentsIfDestroyed;
	}

	public ThingOwner(IThingHolder owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep, bool removeContentsIfDestroyed = true)
		: this(owner)
	{
		maxStacks = (oneStackOnly ? 1 : 999999);
		this.contentsLookMode = contentsLookMode;
		this.removeContentsIfDestroyed = removeContentsIfDestroyed;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref maxStacks, "maxStacks", 999999);
		Scribe_Values.Look(ref contentsLookMode, "contentsLookMode", LookMode.Deep);
		Scribe_Values.Look(ref removeContentsIfDestroyed, "removeContentsIfDestroyed", defaultValue: true);
		Scribe_Values.Look(ref dontTickContents, "dontTickContents", defaultValue: false);
	}

	public void DoTick()
	{
		if (dontTickContents)
		{
			return;
		}
		for (int num = Count - 1; num >= 0; num--)
		{
			Thing at = GetAt(num);
			at.DoTick();
			if (at.Destroyed && removeContentsIfDestroyed)
			{
				Remove(at);
			}
		}
	}

	public void Clear()
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			Remove(GetAt(num));
		}
	}

	public void ClearAndDestroyContents(DestroyMode mode = DestroyMode.Vanish)
	{
		while (Any)
		{
			for (int num = Count - 1; num >= 0; num--)
			{
				Thing at = GetAt(num);
				at.Destroy(mode);
				Remove(at);
			}
		}
	}

	public void ClearAndDestroyContentsOrPassToWorld(DestroyMode mode = DestroyMode.Vanish)
	{
		while (Any)
		{
			for (int num = Count - 1; num >= 0; num--)
			{
				Thing at = GetAt(num);
				at.DestroyOrPassToWorld(mode);
				Remove(at);
			}
		}
	}

	public bool CanAcceptAnyOf(Thing item, bool canMergeWithExistingStacks = true)
	{
		return GetCountCanAccept(item, canMergeWithExistingStacks) > 0;
	}

	public virtual int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
	{
		if (item == null || item.stackCount <= 0)
		{
			return 0;
		}
		if (maxStacks == 999999)
		{
			return item.stackCount;
		}
		int num = 0;
		if (Count < maxStacks)
		{
			num += (maxStacks - Count) * item.def.stackLimit;
		}
		if (num >= item.stackCount)
		{
			return Mathf.Min(num, item.stackCount);
		}
		if (canMergeWithExistingStacks)
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				Thing at = GetAt(i);
				if (at.stackCount < at.def.stackLimit && at.CanStackWith(item))
				{
					num += at.def.stackLimit - at.stackCount;
					if (num >= item.stackCount)
					{
						return Mathf.Min(num, item.stackCount);
					}
				}
			}
		}
		return Mathf.Min(num, item.stackCount);
	}

	public abstract int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true);

	public abstract bool TryAdd(Thing item, bool canMergeWithExistingStacks = true);

	public abstract int IndexOf(Thing item);

	public abstract bool Remove(Thing item);

	protected abstract Thing GetAt(int index);

	public bool Contains(Thing item)
	{
		if (item == null)
		{
			return false;
		}
		return item.holdingOwner == this;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Remove(GetAt(index));
	}

	public int TryAddOrTransfer(Thing item, int count, bool canMergeWithExistingStacks = true)
	{
		if (item == null)
		{
			Log.Warning("Tried to add or transfer null item to ThingOwner.");
			return 0;
		}
		if (item.holdingOwner != null)
		{
			return item.holdingOwner.TryTransferToContainer(item, this, count, canMergeWithExistingStacks);
		}
		return TryAdd(item, count, canMergeWithExistingStacks);
	}

	public bool TryAddOrTransfer(Thing item, bool canMergeWithExistingStacks = true)
	{
		if (item == null)
		{
			Log.Warning("Tried to add or transfer null item to ThingOwner.");
			return false;
		}
		if (item.holdingOwner != null)
		{
			return item.holdingOwner.TryTransferToContainer(item, this, canMergeWithExistingStacks);
		}
		return TryAdd(item, canMergeWithExistingStacks);
	}

	public void TryAddRangeOrTransfer(IEnumerable<Thing> things, bool canMergeWithExistingStacks = true, bool destroyLeftover = false)
	{
		if (things == this)
		{
			return;
		}
		if (things is ThingOwner thingOwner)
		{
			thingOwner.TryTransferAllToContainer(this, canMergeWithExistingStacks);
			if (destroyLeftover)
			{
				thingOwner.ClearAndDestroyContents();
			}
			return;
		}
		if (things is IList<Thing> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!TryAddOrTransfer(list[i], canMergeWithExistingStacks) && destroyLeftover)
				{
					list[i].Destroy();
				}
			}
			return;
		}
		foreach (Thing thing in things)
		{
			if (!TryAddOrTransfer(thing, canMergeWithExistingStacks) && destroyLeftover)
			{
				thing.Destroy();
			}
		}
	}

	public int RemoveAll(Predicate<Thing> predicate)
	{
		int num = 0;
		for (int num2 = Count - 1; num2 >= 0; num2--)
		{
			if (predicate(GetAt(num2)))
			{
				Remove(GetAt(num2));
				num++;
			}
		}
		return num;
	}

	public bool TryTransferToContainer(Thing item, ThingOwner otherContainer, bool canMergeWithExistingStacks = true)
	{
		return TryTransferToContainer(item, otherContainer, item.stackCount, canMergeWithExistingStacks) == item.stackCount;
	}

	public int TryTransferToContainer(Thing item, ThingOwner otherContainer, int count, bool canMergeWithExistingStacks = true)
	{
		Thing resultingTransferredItem;
		return TryTransferToContainer(item, otherContainer, count, out resultingTransferredItem, canMergeWithExistingStacks);
	}

	public int TryTransferToContainer(Thing item, ThingOwner otherContainer, int count, out Thing resultingTransferredItem, bool canMergeWithExistingStacks = true)
	{
		if (!Contains(item))
		{
			Log.Error("Can't transfer item " + item?.ToString() + " because it's not here. owner=" + owner.ToStringSafe());
			resultingTransferredItem = null;
			return 0;
		}
		if (otherContainer == this && count > 0)
		{
			resultingTransferredItem = item;
			return item.stackCount;
		}
		if (!otherContainer.CanAcceptAnyOf(item, canMergeWithExistingStacks))
		{
			resultingTransferredItem = null;
			return 0;
		}
		if (count <= 0)
		{
			resultingTransferredItem = null;
			return 0;
		}
		if (owner is Map || otherContainer.owner is Map)
		{
			Log.Warning("Can't transfer items to or from Maps directly. They must be spawned or despawned manually. Use TryAdd(item.SplitOff(count))");
			resultingTransferredItem = null;
			return 0;
		}
		int num = Mathf.Min(item.stackCount, count);
		Thing thing = item.SplitOff(num);
		if (Contains(thing))
		{
			Remove(thing);
		}
		if (otherContainer.TryAdd(thing, canMergeWithExistingStacks))
		{
			resultingTransferredItem = thing;
			item.MapHeld?.resourceCounter?.CheckUpdateResource(thing);
			return thing.stackCount;
		}
		resultingTransferredItem = null;
		if (!otherContainer.Contains(thing) && thing.stackCount > 0 && !thing.Destroyed)
		{
			int result = num - thing.stackCount;
			if (item != thing)
			{
				item.TryAbsorbStack(thing, respectStackLimit: false);
			}
			else
			{
				TryAdd(thing, canMergeWithExistingStacks: false);
			}
			Map mapHeld = item.MapHeld;
			if (mapHeld != null)
			{
				ResourceCounter resourceCounter = mapHeld.resourceCounter;
				if (resourceCounter != null)
				{
					resourceCounter.CheckUpdateResource(thing);
					return result;
				}
				return result;
			}
			return result;
		}
		return thing.stackCount;
	}

	public void TryTransferAllToContainer(ThingOwner other, bool canMergeWithExistingStacks = true)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			TryTransferToContainer(GetAt(num), other, canMergeWithExistingStacks);
		}
	}

	public Thing Take(Thing thing, int count)
	{
		if (!Contains(thing))
		{
			Log.Error("Tried to take " + thing.ToStringSafe() + " but it's not here.");
			return null;
		}
		if (count > thing.stackCount)
		{
			Log.Error("Tried to get " + count + " of " + thing.ToStringSafe() + " while only having " + thing.stackCount);
			count = thing.stackCount;
		}
		if (count == thing.stackCount)
		{
			Remove(thing);
			return thing;
		}
		Thing thing2 = thing.SplitOff(count);
		thing2.holdingOwner = null;
		return thing2;
	}

	public Thing Take(Thing thing)
	{
		return Take(thing, thing.stackCount);
	}

	public bool TryDrop(Thing thing, ThingPlaceMode mode, int count, out Thing lastResultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		Map rootMap = ThingOwnerUtility.GetRootMap(owner);
		IntVec3 rootPosition = ThingOwnerUtility.GetRootPosition(owner);
		if (rootMap == null || !rootPosition.IsValid)
		{
			Log.Error("Cannot drop " + thing?.ToString() + " without a dropLoc and with an owner whose map is null.");
			lastResultingThing = null;
			return false;
		}
		return TryDrop(thing, rootPosition, rootMap, mode, count, out lastResultingThing, placedAction, nearPlaceValidator);
	}

	public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count, out Thing resultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		if (!Contains(thing))
		{
			Log.Error("Tried to drop " + thing.ToStringSafe() + " but it's not here.");
			resultingThing = null;
			return false;
		}
		if (thing.stackCount < count)
		{
			Log.Error("Tried to drop " + count + " of " + thing?.ToString() + " while only having " + thing.stackCount);
			count = thing.stackCount;
		}
		if (count == thing.stackCount)
		{
			if (GenDrop.TryDropSpawn(thing, dropLoc, map, mode, out resultingThing, placedAction, nearPlaceValidator))
			{
				Remove(thing);
				return true;
			}
			return false;
		}
		Thing thing2 = thing.SplitOff(count);
		if (GenDrop.TryDropSpawn(thing2, dropLoc, map, mode, out resultingThing, placedAction, nearPlaceValidator))
		{
			return true;
		}
		thing.TryAbsorbStack(thing2, respectStackLimit: false);
		return false;
	}

	public bool TryDrop(Thing thing, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null)
	{
		Map rootMap = ThingOwnerUtility.GetRootMap(owner);
		IntVec3 rootPosition = ThingOwnerUtility.GetRootPosition(owner);
		if (rootMap == null || !rootPosition.IsValid)
		{
			Log.Error("Cannot drop " + thing?.ToString() + " without a dropLoc and with an owner whose map is null.");
			lastResultingThing = null;
			return false;
		}
		return TryDrop(thing, rootPosition, rootMap, mode, out lastResultingThing, placedAction, nearPlaceValidator);
	}

	public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null, Predicate<IntVec3> nearPlaceValidator = null, bool playDropSound = true)
	{
		if (!Contains(thing))
		{
			Log.Error(owner.ToStringSafe() + " container tried to drop  " + thing.ToStringSafe() + " which it didn't contain.");
			lastResultingThing = null;
			return false;
		}
		if (GenDrop.TryDropSpawn(thing, dropLoc, map, mode, out lastResultingThing, placedAction, nearPlaceValidator, playDropSound))
		{
			Remove(thing);
			return true;
		}
		return false;
	}

	public bool TryDropAll(IntVec3 dropLoc, Map map, ThingPlaceMode mode, Action<Thing, int> placeAction = null, Predicate<IntVec3> nearPlaceValidator = null, bool playDropSound = true)
	{
		bool result = true;
		for (int num = Count - 1; num >= 0; num--)
		{
			if (!TryDrop(GetAt(num), dropLoc, map, mode, out var _, placeAction, nearPlaceValidator, playDropSound))
			{
				result = false;
			}
		}
		return result;
	}

	public bool Contains(ThingDef def)
	{
		return Contains(def, 1);
	}

	public bool Contains(ThingDef def, int minCount)
	{
		if (minCount <= 0)
		{
			return true;
		}
		int num = 0;
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			if (GetAt(i).def == def)
			{
				num += GetAt(i).stackCount;
			}
			if (num >= minCount)
			{
				return true;
			}
		}
		return false;
	}

	public int TotalStackCountOfDef(ThingDef def)
	{
		int num = 0;
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			if (GetAt(i).def == def)
			{
				num += GetAt(i).stackCount;
			}
		}
		return num;
	}

	public void Notify_ContainedItemDestroyed(Thing t)
	{
		if (ThingOwnerUtility.ShouldAutoRemoveDestroyedThings(owner))
		{
			Remove(t);
		}
	}

	protected virtual void NotifyAdded(Thing item)
	{
		if (ThingOwnerUtility.ShouldAutoExtinguishInnerThings(owner) && item.HasAttachment(ThingDefOf.Fire))
		{
			item.GetAttachment(ThingDefOf.Fire).Destroy();
		}
		if (ThingOwnerUtility.ShouldRemoveDesignationsOnAddedThings(owner))
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].designationManager.RemoveAllDesignationsOn(item);
			}
		}
		if ((owner is Thing thing && thing.Faction.IsPlayerSafe()) || (owner is WorldObject worldObject && worldObject.Faction.IsPlayerSafe()) || (owner is Pawn_InventoryTracker pawn_InventoryTracker && pawn_InventoryTracker.pawn.Faction.IsPlayerSafe()))
		{
			item.EverSeenByPlayer = true;
		}
		if (owner is CompTransporter compTransporter)
		{
			compTransporter.Notify_ThingAdded(item);
		}
		if (owner is Caravan caravan)
		{
			caravan.Notify_PawnAdded((Pawn)item);
		}
		if (owner is Pawn_ApparelTracker pawn_ApparelTracker)
		{
			pawn_ApparelTracker.Notify_ApparelAdded((Apparel)item);
			if (pawn_ApparelTracker.pawn.Faction.IsPlayerSafe())
			{
				item.EverSeenByPlayer = true;
			}
		}
		if (owner is Pawn_EquipmentTracker pawn_EquipmentTracker)
		{
			pawn_EquipmentTracker.Notify_EquipmentAdded((ThingWithComps)item);
			if (pawn_EquipmentTracker.pawn.Faction.IsPlayerSafe())
			{
				item.EverSeenByPlayer = true;
			}
		}
		NotifyColonistBarIfColonistCorpse(item);
		this.OnContentsChanged?.Invoke();
	}

	protected void NotifyAddedAndMergedWith(Thing item, int mergedCount)
	{
		if (owner is CompTransporter compTransporter)
		{
			compTransporter.Notify_ThingAddedAndMergedWith(item, mergedCount);
		}
	}

	protected virtual void NotifyRemoved(Thing item)
	{
		if (owner is Pawn_InventoryTracker pawn_InventoryTracker)
		{
			pawn_InventoryTracker.Notify_ItemRemoved(item);
		}
		if (owner is Pawn_ApparelTracker pawn_ApparelTracker)
		{
			pawn_ApparelTracker.Notify_ApparelRemoved((Apparel)item);
		}
		if (owner is Pawn_EquipmentTracker pawn_EquipmentTracker)
		{
			pawn_EquipmentTracker.Notify_EquipmentRemoved((ThingWithComps)item);
		}
		if (owner is Caravan caravan)
		{
			caravan.Notify_PawnRemoved((Pawn)item);
		}
		NotifyColonistBarIfColonistCorpse(item);
		this.OnContentsChanged?.Invoke();
	}

	private void NotifyColonistBarIfColonistCorpse(Thing thing)
	{
		if (thing is Corpse { Bugged: false } corpse && corpse.InnerPawn.Faction != null && corpse.InnerPawn.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
	}

	void IList<Thing>.Insert(int index, Thing item)
	{
		throw new InvalidOperationException("ThingOwner doesn't allow inserting individual elements at any position.");
	}

	void ICollection<Thing>.Add(Thing item)
	{
		TryAdd(item);
	}

	void ICollection<Thing>.CopyTo(Thing[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[i + arrayIndex] = GetAt(i);
		}
	}

	IEnumerator<Thing> IEnumerable<Thing>.GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return GetAt(i);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return GetAt(i);
		}
	}
}
