using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public sealed class ListerThings
{
	private readonly Dictionary<ThingDef, List<Thing>> listsByDef = new Dictionary<ThingDef, List<Thing>>(ThingDefComparer.Instance);

	private readonly List<Thing>[] listsByGroup;

	private readonly List<IHaulSource> haulSources;

	private readonly int[] stateHashByGroup;

	public ListerThingsUse use;

	public ThingListChangedCallbacks thingListChangedCallbacks;

	private static readonly List<Thing> EmptyList = new List<Thing>();

	private static readonly List<Thing> tmpThingsMatchingFilter = new List<Thing>(1024);

	public List<Thing> AllThings => listsByGroup[2];

	public ListerThings(ListerThingsUse use, ThingListChangedCallbacks thingListChangedCallbacks = null)
	{
		this.use = use;
		this.thingListChangedCallbacks = thingListChangedCallbacks;
		listsByGroup = new List<Thing>[ThingListGroupHelper.AllGroups.Length];
		stateHashByGroup = new int[ThingListGroupHelper.AllGroups.Length];
		haulSources = new List<IHaulSource>();
		listsByGroup[2] = new List<Thing>();
	}

	public List<Thing> ThingsInGroup(ThingRequestGroup group)
	{
		return ThingsMatching(ThingRequest.ForGroup(group));
	}

	public int StateHashOfGroup(ThingRequestGroup group)
	{
		if (use == ListerThingsUse.Region && !group.StoreInRegion())
		{
			Log.ErrorOnce("Tried to get state hash of group " + group.ToString() + " in a region, but this group is never stored in regions. Most likely a global query should have been used.", 1968738832);
			return -1;
		}
		return Gen.HashCombineInt(85693994, stateHashByGroup[(uint)group]);
	}

	public List<Thing> ThingsOfDef(ThingDef def)
	{
		if (def == ThingDefOf.MinifiedThing)
		{
			Log.ErrorOnce("Tried to get ThingsOfDef of MinifiedThing, use ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.MinifiedThing)) instead", 1927834012);
		}
		return ThingsMatching(ThingRequest.ForDef(def));
	}

	public bool AnyThingWithDef(ThingDef def)
	{
		if (listsByDef.ContainsKey(def))
		{
			return listsByDef[def].Count > 0;
		}
		return false;
	}

	public List<Thing> ThingsMatching(ThingRequest req)
	{
		if (req.singleDef != null)
		{
			return listsByDef.GetValueOrDefault(req.singleDef, EmptyList);
		}
		if (req.group != ThingRequestGroup.Undefined)
		{
			if (use == ListerThingsUse.Region && !req.group.StoreInRegion())
			{
				Log.ErrorOnce("Tried to get things in group " + req.group.ToString() + " in a region, but this group is never stored in regions. Most likely a global query should have been used.", 1968735132);
				return EmptyList;
			}
			return listsByGroup[(uint)req.group] ?? EmptyList;
		}
		ThingRequest thingRequest = req;
		throw new InvalidOperationException("Invalid ThingRequest " + thingRequest.ToString());
	}

	public List<Thing> ThingsMatchingFilter(ThingFilter filter)
	{
		tmpThingsMatchingFilter.Clear();
		foreach (ThingDef allowedThingDef in filter.AllowedThingDefs)
		{
			tmpThingsMatchingFilter.AddRange(ThingsOfDef(allowedThingDef));
		}
		return tmpThingsMatchingFilter;
	}

	public void GetThingsOfType<T>(List<T> list) where T : Thing
	{
		if (typeof(T) == typeof(Thing))
		{
			Log.Error("Do not call this method with type 'Thing' directly, as it will return all things currently registered.");
			return;
		}
		List<Thing> allThings = AllThings;
		for (int i = 0; i < AllThings.Count; i++)
		{
			if (allThings[i] is T item)
			{
				list.Add(item);
			}
		}
	}

	public IEnumerable<T> GetThingsOfType<T>() where T : Thing
	{
		if (typeof(T) == typeof(Thing))
		{
			Log.Error("Do not call this method with type 'Thing' directly, as it will return all things currently registered.");
			yield break;
		}
		List<Thing> things = AllThings;
		for (int i = 0; i < AllThings.Count; i++)
		{
			if (things[i] is T val)
			{
				yield return val;
			}
		}
	}

	public IEnumerable<Thing> GetAllThings(Predicate<Thing> validator = null, bool lookInHaulSources = false)
	{
		foreach (Thing allThing in AllThings)
		{
			if (validator == null || validator(allThing))
			{
				yield return allThing;
			}
		}
		if (!lookInHaulSources)
		{
			yield break;
		}
		foreach (IHaulSource haulSource in haulSources)
		{
			foreach (Thing item in (IEnumerable<Thing>)haulSource.GetDirectlyHeldThings())
			{
				if (validator == null || validator(item))
				{
					yield return item;
				}
			}
		}
	}

	public void GetAllThings(in List<Thing> list, Predicate<Thing> validator = null, bool lookInHaulSources = false)
	{
		if (validator != null)
		{
			foreach (Thing allThing in AllThings)
			{
				if (validator(allThing))
				{
					list.Add(allThing);
				}
			}
		}
		else
		{
			list.AddRange(AllThings);
		}
		if (!lookInHaulSources)
		{
			return;
		}
		foreach (IHaulSource haulSource in haulSources)
		{
			foreach (Thing item in (IEnumerable<Thing>)haulSource.GetDirectlyHeldThings())
			{
				if (validator == null || validator(item))
				{
					list.Add(item);
				}
			}
		}
	}

	public void GetAllThings(in List<Thing> list, ThingRequestGroup group, Predicate<Thing> validator = null, bool lookInHaulSources = false)
	{
		if (validator != null)
		{
			foreach (Thing item in ThingsInGroup(group))
			{
				if (validator(item))
				{
					list.Add(item);
				}
			}
		}
		else
		{
			list.AddRange(ThingsInGroup(group));
		}
		if (!lookInHaulSources)
		{
			return;
		}
		foreach (IHaulSource haulSource in haulSources)
		{
			foreach (Thing item2 in (IEnumerable<Thing>)haulSource.GetDirectlyHeldThings())
			{
				if (GroupIncludes(item2, group) && (validator == null || validator(item2)))
				{
					list.Add(item2);
				}
			}
		}
	}

	public bool Contains(Thing t)
	{
		return AllThings.Contains(t);
	}

	public void Add(Thing t)
	{
		if (!EverListable(t.def, use))
		{
			return;
		}
		if (!listsByDef.TryGetValue(t.def, out var value))
		{
			value = new List<Thing>();
			listsByDef.Add(t.def, value);
		}
		value.Add(t);
		if (t is IHaulSource item)
		{
			haulSources.Add(item);
		}
		ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
		foreach (ThingRequestGroup thingRequestGroup in allGroups)
		{
			if (GroupIncludes(t, thingRequestGroup))
			{
				List<Thing> list = listsByGroup[(uint)thingRequestGroup];
				if (list == null)
				{
					list = new List<Thing>();
					listsByGroup[(uint)thingRequestGroup] = list;
					stateHashByGroup[(uint)thingRequestGroup] = 0;
				}
				list.Add(t);
				stateHashByGroup[(uint)thingRequestGroup]++;
			}
		}
		thingListChangedCallbacks?.onThingAdded?.Invoke(t);
	}

	private bool GroupIncludes(Thing thing, ThingRequestGroup group)
	{
		if (use == ListerThingsUse.Region && !group.StoreInRegion())
		{
			return false;
		}
		return group.Includes(thing.def);
	}

	public void Remove(Thing t)
	{
		if (!EverListable(t.def, use))
		{
			return;
		}
		if (listsByDef.TryGetValue(t.def, out var value))
		{
			value.Remove(t);
		}
		if (t is IHaulSource item)
		{
			haulSources.Remove(item);
		}
		ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
		for (int i = 0; i < allGroups.Length; i++)
		{
			ThingRequestGroup thingRequestGroup = allGroups[i];
			if ((use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion()) && thingRequestGroup.Includes(t.def))
			{
				listsByGroup[i].Remove(t);
				stateHashByGroup[(uint)thingRequestGroup]++;
			}
		}
		thingListChangedCallbacks?.onThingRemoved?.Invoke(t);
	}

	public static bool EverListable(ThingDef def, ListerThingsUse use)
	{
		if (def.category == ThingCategory.Mote && (!def.drawGUIOverlay || use == ListerThingsUse.Region))
		{
			return false;
		}
		if (def.category == ThingCategory.Projectile && use == ListerThingsUse.Region)
		{
			return false;
		}
		return true;
	}

	public void Clear()
	{
		listsByDef.Clear();
		for (int i = 0; i < listsByGroup.Length; i++)
		{
			if (listsByGroup[i] != null)
			{
				listsByGroup[i].Clear();
			}
			stateHashByGroup[i] = 0;
		}
	}
}
